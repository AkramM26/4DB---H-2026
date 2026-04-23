using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static HugoLand.Core.Services.ArmyService;
using static HugoLand.Core.Services.CombatService;

namespace HugoLand.WPF
{
    public partial class GameDisplay : Window
    {
        private readonly HugoLandContext _context;
        private readonly Guid _gameId;
        private readonly GameService _gameService;
        private readonly ArmyService _armyService;
        private readonly InstallationService _installationService;
        private readonly bool _startTurnOnOpen;

        private Border[,]? _cells;
        private TextBlock[,]? _cellLabels;

        private TaskCompletionSource<int>? _mainActionTcs;
        private TaskCompletionSource<MilitaryDetachment?>? _armySelectTcs;
        private TaskCompletionSource<int>? _armyActionTcs;
        private TaskCompletionSource<char>? _moveDirTcs;
        private TaskCompletionSource<int?>? _numberTcs;
        private TaskCompletionSource<bool>? _victoryTcs;

        private bool _windowClosing;

        public GameDisplay(HugoLandContext context, Guid gameId, bool startTurnOnOpen)
        {
            InitializeComponent();
            _context = context;
            _gameId = gameId;
            _startTurnOnOpen = startTurnOnOpen;
            _context.CurrentGameId = gameId;

            _gameService = new GameService(context);
            _armyService = new ArmyService(context);
            _installationService = new InstallationService(context);

            Closing += GameDisplay_Closing;
            Loaded += async (_, _) => await RunGameAsync();
        }

        private async Task StartTurnIfNeededAsync()
        {
            if (_startTurnOnOpen)
            {
                await _gameService.StartTurnAsync();
            }
        }

        private void GameDisplay_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _windowClosing = true;
            // Cancel any pending prompt so the awaiting loop unblocks.
            _mainActionTcs?.TrySetResult(-1);
            _armySelectTcs?.TrySetResult(null);
            _armyActionTcs?.TrySetResult(-1);
            _moveDirTcs?.TrySetResult('Q');
            _numberTcs?.TrySetResult(null);
            _victoryTcs?.TrySetResult(true);
        }

        private async Task RunGameAsync()
        {
            try
            {
                BuildBoard();
                await RefreshBoardAsync();

                await StartTurnIfNeededAsync();
                await RefreshBoardAsync();

                while (!_windowClosing)
                {

                    var game = await FetchGameGraphAsync();
                    if (game == null) return;

                    if (game.IsFinished)
                    {
                        await ShowVictoryAsync(game);
                        Close();
                        return;
                    }

                    bool inTurn = true;
                    while (inTurn && !_windowClosing)
                    {
                        await RefreshBoardAsync();
                        int action = await AwaitMainActionAsync();
                        if (_windowClosing) return;

                        switch (action)
                        {
                            case 1: // Select army
                                var army = await AwaitArmySelectionAsync();
                                if (_windowClosing) return;
                                if (army != null)
                                    await HandleArmyActionsAsync(army);
                                break;

                            case 2: // Save game
                                await _gameService.SaveGameAsync();
                                Log("Game saved.");
                                break;

                            case 3: // Main menu
                                Close();
                                return;

                            case 4: // End turn
                                await _gameService.EndTurnAsync();
                                await _gameService.StartTurnAsync();
                                await RefreshBoardAsync();
                                inTurn = false;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_windowClosing)
                    MessageBox.Show($"Error in game loop:\n{ex}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Game?> FetchGameGraphAsync()
        {
            return await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Territories).ThenInclude(t => t.MilitaryDetachment).ThenInclude(m => m!.Player)
                .Include(g => g.Territories).ThenInclude(t => t.Installation)
                .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Territory)
                .FirstOrDefaultAsync();
        }

        // ============================================================
        // Army action handling
        // ============================================================
        private async Task HandleArmyActionsAsync(MilitaryDetachment army)
        {
            int choice = await AwaitArmyActionAsync(army);
            if (_windowClosing) return;

            switch (choice)
            {
                case 1: // Move
                {
                    char moveChoice = ' ';
                    while (moveChoice != 'X' && !_windowClosing)
                    {
                        var territories = await _armyService.TryMove(army, army.Territory.PositionX, army.Territory.PositionY);
                        moveChoice = await AwaitMoveDirectionAsync(army, territories);
                        if (_windowClosing) return;
                        if (moveChoice == 'X' || moveChoice == 'Q') break;

                        Movements dir = moveChoice switch
                        {
                            'N' => Movements.North,
                            'S' => Movements.South,
                            'E' => Movements.East,
                            'W' => Movements.West,
                            _ => Movements.North,
                        };
                        var result = await _armyService.Move(army.Id, dir);
                        LogMoveResult(result);
                        await RefreshBoardAsync();
                        var refreshed = await _context.MilitaryDetachments
                            .Include(m => m.Territory).FirstOrDefaultAsync(m => m.Id == army.Id);
                        if (refreshed == null)
                        {
                            Log("Army no longer exists.");
                            return;
                        }
                        army = refreshed;
                    }
                    break;
                }
                case 2:
                {
                    var result = await _installationService.BuildCampAsync(army.Id);
                    Log(result.Success ? "Camp built." : $"Cannot build camp: {result.Message}");
                    break;
                }
                case 3:
                {
                    var result = await _installationService.UpgradeCampToFortificationAsync(army.Id);
                    Log(result.Success ? "Upgraded to fortification." : $"Cannot upgrade: {result.Message}");
                    break;
                }
                case 4:
                {
                    int? n = await AwaitNumberAsync("Number of soldiers to buy (2g each):", 1);
                    if (_windowClosing) return;
                    if (n == null || n <= 0) break;
                    var player = (await FetchGameGraphAsync())!.Players.First(p => p.Id == army.PlayerId);
                    var result = await _armyService.Reinforce(army, n.Value, player);
                    Log(result.Success ? $"Reinforced by {n}." : $"Cannot reinforce: {result.Message}");
                    break;
                }
                case 5:
                {
                    int? splitNumber = await AwaitNumberAsync("Soldiers to split off (min 10, 0 to cancel):", 10);
                    if (_windowClosing) return;
                    if (splitNumber == null || splitNumber == 0) break;

                    var territories = await _armyService.TryMove(army, army.Territory.PositionX, army.Territory.PositionY);
                    char moveChoice = await AwaitMoveDirectionAsync(army, territories);
                    if (_windowClosing) return;
                    if (moveChoice == 'X' || moveChoice == 'Q') break;

                    Movements dir = moveChoice switch
                    {
                        'N' => Movements.North,
                        'S' => Movements.South,
                        'E' => Movements.East,
                        'W' => Movements.West,
                        _ => Movements.North,
                    };
                    var result = await _armyService.Split(army.Id, dir, splitNumber.Value);
                    LogMoveResult(result);
                    break;
                }
                case 6:
                    Log("Pass.");
                    break;
            }

            await RefreshBoardAsync();
        }

        // ============================================================
        // Board rendering
        // ============================================================
        private void BuildBoard()
        {
            int w = GameConstants.gameSizeX;
            int h = GameConstants.gameSizeY;

            grdBoard.RowDefinitions.Clear();
            grdBoard.ColumnDefinitions.Clear();
            grdBoard.Children.Clear();

            grdBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            for (int i = 0; i < w; i++)
                grdBoard.ColumnDefinitions.Add(new ColumnDefinition());
            grdBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
            for (int i = 0; i < h; i++)
                grdBoard.RowDefinitions.Add(new RowDefinition());

            for (int x = 0; x < w; x++)
            {
                var header = new TextBlock
                {
                    Text = x.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, x + 1);
                grdBoard.Children.Add(header);
            }
            for (int y = 0; y < h; y++)
            {
                var header = new TextBlock
                {
                    Text = y.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                };
                Grid.SetRow(header, y + 1);
                Grid.SetColumn(header, 0);
                grdBoard.Children.Add(header);
            }

            _cells = new Border[w, h];
            _cellLabels = new TextBlock[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tb = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = 11,
                    };
                    var border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.White,
                        Child = tb,
                    };
                    Grid.SetColumn(border, x + 1);
                    Grid.SetRow(border, y + 1);

                    int capturedX = x;
                    int capturedY = y;
                    border.MouseLeftButtonUp += (_, _) => OnCellClick(capturedX, capturedY);

                    grdBoard.Children.Add(border);
                    _cells[x, y] = border;
                    _cellLabels[x, y] = tb;
                }
            }
        }

        private async Task RefreshBoardAsync()
        {
            var game = await FetchGameGraphAsync();
            if (game == null || _cells == null || _cellLabels == null) return;

            var currentPlayer = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);
            char letter = game.PlayerTurn == 1 ? 'A' : 'B';
            txtStatus.Text =
                $"Turn {game.TurnNumber} - Player {letter}   |   " +
                $"Gold: {currentPlayer.Gold}   Income: +{currentPlayer.Income}   Cost: -{currentPlayer.Cost}   " +
                $"Debt turns: {currentPlayer.TurnInDept}";

            int w = GameConstants.gameSizeX;
            int h = GameConstants.gameSizeY;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var t = game.Territories.FirstOrDefault(tt => tt.PositionX == x && tt.PositionY == y);
                    if (t == null) continue;

                    var cell = _cells[x, y];
                    var label = _cellLabels[x, y];

                    cell.Background = t.TerritoryType switch
                    {
                        TerritoryType.Forest => Brushes.LightGreen,
                        TerritoryType.Mountain => Brushes.LightGray,
                        _ => Brushes.PapayaWhip,
                    };

                    label.Text = FormatCell(t);
                    label.Foreground = t.MilitaryDetachment?.Player.PlayerNumber == 1
                        ? Brushes.DarkBlue
                        : t.MilitaryDetachment != null ? Brushes.DarkRed : Brushes.Black;
                }
            }
        }

        private static string FormatCell(Territory t)
        {
            var inst = t.Installation;
            var md = t.MilitaryDetachment;

            string armyStr = string.Empty;
            if (md != null)
            {
                char letter = md.Player.PlayerNumber == 1 ? 'A' : 'B';
                if (md.MilitaryForce < 10) letter = char.ToLower(letter);
                armyStr = $"{letter}{md.MilitaryForce}";
            }

            if (inst == null)
                return string.IsNullOrEmpty(armyStr) ? "." : armyStr;

            (char open, char close) = inst.InstallationType == InstallationType.Fortification
                ? ('[', ']')
                : ('(', ')');

            return $"{open}{armyStr}{close}";
        }

        private Task<int> AwaitMainActionAsync()
        {
            _mainActionTcs = new TaskCompletionSource<int>();
            ShowPanel(pnlMainActions);
            return _mainActionTcs.Task;
        }

        private Task<MilitaryDetachment?> AwaitArmySelectionAsync()
        {
            _armySelectTcs = new TaskCompletionSource<MilitaryDetachment?>();
            txtArmyX.Text = string.Empty;
            txtArmyY.Text = string.Empty;
            ShowPanel(pnlArmySelect);
            return _armySelectTcs.Task;
        }

        private Task<int> AwaitArmyActionAsync(MilitaryDetachment army)
        {
            _armyActionTcs = new TaskCompletionSource<int>();
            string inst = army.Territory.Installation?.InstallationType.ToString() ?? "None";
            txtArmyInfo.Text =
                $"Army at ({army.Territory.PositionX},{army.Territory.PositionY})\n" +
                $"Force: {army.MilitaryForce}   Energy: {army.Energy}\n" +
                $"Can act: {army.CanAct}   Can move: {army.CanMove}\n" +
                $"Installation: {inst}";
            ShowPanel(pnlArmyActions);
            return _armyActionTcs.Task;
        }

        private Task<char> AwaitMoveDirectionAsync(MilitaryDetachment army, List<Territory> adjacents)
        {
            _moveDirTcs = new TaskCompletionSource<char>();
            txtMoveInfo.Text = BuildMoveInfo(army, adjacents);
            ShowPanel(pnlMove);
            return _moveDirTcs.Task;
        }

        private static string BuildMoveInfo(MilitaryDetachment army, List<Territory> adjacents)
        {
            int x = army.Territory.PositionX;
            int y = army.Territory.PositionY;
            var lines = new List<string>
            {
                $"Army at ({x},{y}) - Energy: {army.Energy}",
                "Adjacent territories:",
            };
            foreach (var t in adjacents)
            {
                if (t == null) continue;
                string dir =
                    (t.PositionX == x && t.PositionY == y - 1) ? "N" :
                    (t.PositionX == x && t.PositionY == y + 1) ? "S" :
                    (t.PositionX == x + 1 && t.PositionY == y) ? "E" :
                    (t.PositionX == x - 1 && t.PositionY == y) ? "W" : "?";
                string info = $"[{dir}] ({t.PositionX},{t.PositionY}) {t.TerritoryType}";
                if (t.MilitaryDetachment != null)
                {
                    bool enemy = t.MilitaryDetachment.PlayerId != army.PlayerId;
                    info += enemy
                        ? $" - Enemy ({t.MilitaryDetachment.MilitaryForce})"
                        : $" - Ally ({t.MilitaryDetachment.MilitaryForce})";
                }
                lines.Add(info);
            }
            return string.Join("\n", lines);
        }

        private Task<int?> AwaitNumberAsync(string prompt, int min)
        {
            _numberTcs = new TaskCompletionSource<int?>();
            txtNumberPrompt.Text = prompt;
            txtNumber.Text = string.Empty;
            txtNumber.Tag = min;
            ShowPanel(pnlNumber);
            txtNumber.Focus();
            return _numberTcs.Task;
        }

        private Task ShowVictoryAsync(Game game)
        {
            _victoryTcs = new TaskCompletionSource<bool>();
            char letter = game.WinnerPlayerNumber == 1 ? 'A' : 'B';
            string cond = game.MilitaryVictory ? "military" : "economic";
            txtVictory.Text = $"Player {letter} wins!\nVictory condition: {cond}.";
            ShowPanel(pnlVictory);
            return _victoryTcs.Task;
        }

        private void ShowPanel(StackPanel panel)
        {
            pnlMainActions.Visibility = Visibility.Collapsed;
            pnlArmySelect.Visibility = Visibility.Collapsed;
            pnlArmyActions.Visibility = Visibility.Collapsed;
            pnlMove.Visibility = Visibility.Collapsed;
            pnlNumber.Visibility = Visibility.Collapsed;
            pnlVictory.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
        }

        private void Log(string line)
        {
            string stamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.Text += $"[{stamp}] {line}\n";
            scrLog.ScrollToEnd();
        }

        private void LogMoveResult(MoveResult? r)
        {
            if (r == null) { Log("Move returned nothing."); return; }
            if (r.Fusion) { Log("Armies fused."); return; }
            if (r.Fight && r.CombatResult is CombatResult cr)
            {
                Log($"Combat: Atk {cr.AttackInitialForce} vs Def {cr.DefenceInitialForce} " +
                    $"(terrain x{cr.TerritoryMultiplier}, installation x{cr.InstallationMultiplier})");
                Log($"  Random: atk x{cr.AttackRandomFactor:F2}, def x{cr.DefenceRandomFactor:F2}");
                Log($"  Effective: atk {cr.EffectiveAttackForce:F1} / def {cr.EffectiveDefenceForce:F1}");
                Log(cr.DefenceVictory ? "  Defender wins." : "  Attacker wins.");
                Log($"  Loot: {cr.GoldGain} gold. Atk left: {cr.AttackForce}, Def left: {cr.DefenceForce}");
                return;
            }
            if (r.move) { Log("Army moved."); return; }
            Log("Move cancelled or invalid.");
        }

        // ============================================================
        // Event handlers wired from XAML
        // ============================================================
        private void btnSelectArmy_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(1);
        private void btnSaveGame_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(2);
        private void btnMainMenu_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(3);
        private void btnEndTurn_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(4);

        private void btnArmySelectConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtArmyX.Text, out int x) || !int.TryParse(txtArmyY.Text, out int y))
            {
                Log("Invalid coordinates.");
                return;
            }
            TrySelectArmyAt(x, y);
        }

        private void btnArmySelectCancel_Click(object sender, RoutedEventArgs e)
            => _armySelectTcs?.TrySetResult(null);

        private void OnCellClick(int x, int y)
        {
            if (_armySelectTcs != null && !_armySelectTcs.Task.IsCompleted)
                TrySelectArmyAt(x, y);
        }

        private void TrySelectArmyAt(int x, int y)
        {
            if (_armySelectTcs == null || _armySelectTcs.Task.IsCompleted) return;

            var game = _context.Games
                .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Territory)
                .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Player)
                .FirstOrDefault();
            if (game == null) { Log("Game state missing."); return; }

            var army = game.MilitaryDetachments.FirstOrDefault(m =>
                m.Territory.PositionX == x && m.Territory.PositionY == y);

            if (army == null)
            {
                Log($"No army at ({x},{y}).");
                return;
            }
            if (army.Player.PlayerNumber != game.PlayerTurn)
            {
                Log($"Army at ({x},{y}) belongs to the other player.");
                return;
            }
            if (army.MilitaryForce < 10)
            {
                Log($"Army at ({x},{y}) has less than 10 soldiers (isolated).");
                return;
            }
            _armySelectTcs.TrySetResult(army);
        }

        private void btnArmyAct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string tag && int.TryParse(tag, out int choice))
                _armyActionTcs?.TrySetResult(choice);
        }

        private void btnMoveDir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string tag && tag.Length == 1)
                _moveDirTcs?.TrySetResult(tag[0]);
        }

        private void btnNumberOk_Click(object sender, RoutedEventArgs e)
        {
            int min = txtNumber.Tag is int m ? m : 0;
            if (!int.TryParse(txtNumber.Text, out int n))
            {
                Log("Invalid number.");
                return;
            }
            if (n != 0 && n < min)
            {
                Log($"Value must be >= {min} (or 0 to cancel).");
                return;
            }
            _numberTcs?.TrySetResult(n);
        }

        private void btnNumberCancel_Click(object sender, RoutedEventArgs e)
            => _numberTcs?.TrySetResult(null);

        private void btnVictoryOk_Click(object sender, RoutedEventArgs e)
            => _victoryTcs?.TrySetResult(true);
    }
}
