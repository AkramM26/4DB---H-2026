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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
        private readonly HashSet<Guid> _playableArmyIds = new HashSet<Guid>();
        private Guid? _selectedArmyId;
        private bool _isArmySelectionMode;
        private string _turnPrompt = "Select an army or choose an action";

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
            PreviewKeyDown += GameDisplay_PreviewKeyDown;
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
                                AudioManager.Movements.Play();
                                var army = await AwaitArmySelectionAsync();
                                if (_windowClosing) return;
                                if (army != null)
                                    await HandleArmyActionsAsync(army);
                                break;

                            case 2: // Save game
                                AudioManager.Movements.Play();    
                                await _gameService.SaveGameAsync();
                                Log("Game saved.");
                                break;

                            case 3: // Main menu
                                AudioManager.Movements.Play();
                                Close();
                                return;

                            case 4: // End turn
                                AudioManager.Movements.Play();
                                _selectedArmyId = null;
                                ClearArmySelectionState();
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
                        //Button sound 
                        AudioManager.Movements.Play();

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
                        try
                        {
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
                        catch (Exception ex)
                        {
                            Log($"Move failed: {ex.Message}");
                            return;
                        }
                    }
                    break;
                }
                case 2:
                    {
                        //Button sound 
                        AudioManager.Movements.Play();
                        var result = await _installationService.BuildCampAsync(army.Id);
                        Log(result.Success ? "Camp built." : $"Cannot build camp: {result.Message}");
                        break;
                    }
                case 3:
                    {
                        //Button sound 
                        AudioManager.Movements.Play();
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
                        //Button sound 
                        AudioManager.Movements.Play();
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
                    //Button sound 
                    AudioManager.Movements.Play();
                    Log("Pass.");
                    break;
            }

            _selectedArmyId = null;
            _turnPrompt = "Select an army or choose an action";
            await RefreshBoardAsync();
        }

        // ============================================================
        // Board rendering
        // ============================================================
        private void BuildBoard()
        {
            var game = _context.Games.FirstOrDefault();
            int w = game.GameSizeX;
            int h = game.GameSizeY;

            grdBoard.RowDefinitions.Clear();
            grdBoard.ColumnDefinitions.Clear();
            grdBoard.Children.Clear();

            const int RightReserve  = 288;
            const int TopReserve    = 110;
            const int BotReserve    = 132;
            const int ChromeReserve = 40;
            const int Margins       = 32;
            const int RowHeaderW    = 26;
            const int ColHeaderH    = 22;

            var screen   = SystemParameters.WorkArea;
            double availW = screen.Width  - RightReserve - RowHeaderW - Margins;
            double availH = screen.Height - TopReserve  - BotReserve - ColHeaderH - Margins - ChromeReserve;

            int cellSize = (int)Math.Floor(Math.Min(availW / w, availH / h));
            cellSize = Math.Clamp(cellSize, 20, 64);

            // Resize and re-centre the window on the current screen
            double newW = Math.Min(RowHeaderW  + cellSize * w + RightReserve + Margins, screen.Width);
            double newH = Math.Min(ColHeaderH + cellSize * h + TopReserve + BotReserve + Margins + ChromeReserve, screen.Height);
            Width  = newW;
            Height = newH;
            Left   = screen.Left + (screen.Width  - newW) / 2;
            Top    = screen.Top  + (screen.Height - newH) / 2;

            grdBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RowHeaderW) });
            for (int i = 0; i < w; i++)
                grdBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grdBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ColHeaderH) });
            for (int i = 0; i < h; i++)
                grdBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Hide axis-header labels on very dense maps to avoid clutter
            bool showHeaders = cellSize >= 24;
            var headerFg = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

            for (int x = 0; x < w; x++)
            {
                var header = new TextBlock
                {
                    Text = x.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    FontSize            = 10,
                    Foreground          = headerFg,
                    Visibility          = showHeaders ? Visibility.Visible : Visibility.Hidden,
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
                    VerticalAlignment   = VerticalAlignment.Center,
                    FontSize            = 10,
                    Foreground          = headerFg,
                    Visibility          = showHeaders ? Visibility.Visible : Visibility.Hidden,
                };
                Grid.SetRow(header, y + 1);
                Grid.SetColumn(header, 0);
                grdBoard.Children.Add(header);
            }

            _cells      = new Border[w, h];
            _cellLabels = new TextBlock[w, h];

            var cellBorder = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            var cellBg     = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tb = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center,
                        FontFamily          = new FontFamily("Consolas"),
                        FontSize            = 11,
                        Foreground          = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    };
                    var border = new Border
                    {
                        BorderBrush     = cellBorder,
                        BorderThickness = new Thickness(0.5),
                        Background      = cellBg,
                        Cursor          = Cursors.Arrow,
                        Child           = tb,
                    };
                    Grid.SetColumn(border, x + 1);
                    Grid.SetRow(border, y + 1);

                    int capturedX = x;
                    int capturedY = y;
                    border.MouseLeftButtonUp += (_, _) => OnCellClick(capturedX, capturedY);

                    grdBoard.Children.Add(border);
                    _cells[x, y]      = border;
                    _cellLabels[x, y] = tb;
                }
            }
        }

        private async Task RefreshBoardAsync()
        {
            var game = await FetchGameGraphAsync();
            if (game == null || _cells == null || _cellLabels == null) return;

            var currentPlayer = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);
            txtStatus.Text =
                $"Turn {game.TurnNumber} - Player {game.PlayerTurn}   |   " +
                $"Gold: {currentPlayer.Gold}   Income: +{currentPlayer.Income}   Cost: -{currentPlayer.Cost}   " +
                $"Debt turns: {currentPlayer.TurnInDept}";
            UpdateTurnBanner(game);

            int w = game.GameSizeX;
            int h = game.GameSizeY;
            //int w = GameConstants.gameSizeX;
            //int h = GameConstants.gameSizeY;

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
                        TerritoryType.Plain => Brushes.LightGreen,
                        TerritoryType.Forest => Brushes.DarkGreen,
                        TerritoryType.Mountain => Brushes.Gray,
                        TerritoryType.Ocean => Brushes.LightBlue,
                        _ => Brushes.LightGreen,
                    };
                    cell.BorderBrush = Brushes.Black;
                    cell.BorderThickness = new Thickness(1);
                    cell.Effect = null;
                    cell.Cursor = Cursors.Arrow;

                    label.Text = FormatCell(t);
                    Brush defaultForeground = t.TerritoryType == TerritoryType.Forest ? Brushes.White : Brushes.Black;
                    label.Foreground = t.MilitaryDetachment?.Player.PlayerNumber == 1
                        ? Brushes.DarkBlue
                        : t.MilitaryDetachment != null ? Brushes.DarkRed : defaultForeground;

                    if (t.MilitaryDetachment == null)
                        continue;

                    bool isPlayable = _isArmySelectionMode && _playableArmyIds.Contains(t.MilitaryDetachment.Id);
                    bool isSelected = _selectedArmyId == t.MilitaryDetachment.Id;

                    if (isPlayable)
                        ApplyCellHighlight(cell, false);

                    if (isSelected)
                        ApplyCellHighlight(cell, true);
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

        private void UpdateTurnBanner(Game game)
        {
            bool isPlayerOne = game.PlayerTurn == 1;
            brdTurnBanner.Background = isPlayerOne
                ? new SolidColorBrush(Color.FromRgb(47, 111, 219))
                : new SolidColorBrush(Color.FromRgb(181, 54, 72));
            txtTurnBanner.Text = $"Player {game.PlayerTurn} turn";
            txtTurnPrompt.Text = _turnPrompt;
        }

        private bool IsPlayableArmy(MilitaryDetachment army, Game game)
            => army.Player.PlayerNumber == game.PlayerTurn
                && army.MilitaryForce >= GameConstants.MinimumArmyForceForActions;

        private void SetPlayableArmyState(Game game)
        {
            _playableArmyIds.Clear();
            foreach (var army in game.MilitaryDetachments)
            {
                if (IsPlayableArmy(army, game))
                    _playableArmyIds.Add(army.Id);
            }
        }

        private void ClearArmySelectionState()
        {
            _isArmySelectionMode = false;
            _playableArmyIds.Clear();
        }

        private void ApplyCellHighlight(Border cell, bool isSelected)
        {
            cell.BorderBrush = isSelected ? Brushes.Gold : Brushes.Goldenrod;
            cell.BorderThickness = isSelected ? new Thickness(3) : new Thickness(2);
            cell.Cursor = Cursors.Hand;
            cell.Effect = new DropShadowEffect
            {
                Color = isSelected ? Colors.Gold : Colors.Khaki,
                BlurRadius = isSelected ? 18 : 12,
                ShadowDepth = 0,
                Opacity = isSelected ? 0.95 : 0.8
            };
        }

        private async Task<MilitaryDetachment?> TryGetPlayableArmyAtAsync(int x, int y, bool logFailures)
        {
            var game = await _context.Games
                .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Territory)
                .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Player)
                .FirstOrDefaultAsync();
            if (game == null)
            {
                if (logFailures)
                    Log("Game state missing.");
                return null;
            }

            var army = game.MilitaryDetachments.FirstOrDefault(m =>
                m.Territory.PositionX == x && m.Territory.PositionY == y);

            if (army == null)
            {
                if (logFailures)
                    Log($"No army at ({x},{y}).");
                return null;
            }

            if (!IsPlayableArmy(army, game))
            {
                if (logFailures)
                    Log($"Army at ({x},{y}) cannot be selected right now.");
                return null;
            }

            return army;
        }

        private async Task UpdateArmyActionButtonsAsync(MilitaryDetachment army)
        {
            var player = (await FetchGameGraphAsync())?.Players.FirstOrDefault(p => p.Id == army.PlayerId);
            var moveTargets = await _armyService.TryMove(army, army.Territory.PositionX, army.Territory.PositionY);
            bool hasCamp = army.Territory.Installation?.InstallationType == InstallationType.Camp;
            bool hasAnyInstallation = army.Territory.Installation != null;

            btnMoveAction.IsEnabled = army.CanMove && army.Energy > 0 && moveTargets.Count > 0;
            btnBuildCampAction.IsEnabled = army.CanAct
                && army.MilitaryForce >= GameConstants.MinimumArmyForceForActions
                && !hasAnyInstallation
                && player != null
                && player.Gold >= GameConstants.CampConstructionCost;
            btnUpgradeCampAction.IsEnabled = army.CanAct
                && army.MilitaryForce >= GameConstants.MinimumArmyForceForActions
                && hasCamp
                && army.Territory.Installation!.PlayerId == army.PlayerId
                && player != null
                && player.Gold >= GameConstants.FortificationUpgradeCost;
            btnReinforceAction.IsEnabled = army.MilitaryForce >= GameConstants.MinimumArmyForceForActions
                && hasAnyInstallation
                && player != null
                && player.Gold > 2;
            btnSplitArmyAction.IsEnabled = army.CanMove && army.Energy > 0 && army.MilitaryForce >= 20 && moveTargets.Count > 0;
            btnPassArmyAction.IsEnabled = true;
        }

        private async Task<int> AwaitMainActionAsync()
        {
            _mainActionTcs = new TaskCompletionSource<int>();
            _turnPrompt = "Select an army or choose an action";
            _selectedArmyId = null;
            ClearArmySelectionState();
            ShowPanel(pnlMainActions);
            await RefreshBoardAsync();
            return await _mainActionTcs.Task;
        }

        private async Task<MilitaryDetachment?> AwaitArmySelectionAsync()
        {
            _armySelectTcs = new TaskCompletionSource<MilitaryDetachment?>();
            txtArmyX.Text = string.Empty;
            txtArmyY.Text = string.Empty;

            var game = await FetchGameGraphAsync();
            _isArmySelectionMode = true;
            _selectedArmyId = null;
            _turnPrompt = "Choose an army to play";
            _playableArmyIds.Clear();
            if (game != null)
                SetPlayableArmyState(game);

            ShowPanel(pnlArmySelect);
            await RefreshBoardAsync();
            return await _armySelectTcs.Task;
        }

        private async Task<int> AwaitArmyActionAsync(MilitaryDetachment army)
        {
            _armyActionTcs = new TaskCompletionSource<int>();
            army = await _context.MilitaryDetachments
                .Include(m => m.Player)
                .Include(m => m.Territory)
                .ThenInclude(t => t.Installation)
                .FirstAsync(m => m.Id == army.Id);

            string inst = army.Territory.Installation?.InstallationType.ToString() ?? "None";
            _selectedArmyId = army.Id;
            ClearArmySelectionState();
            _turnPrompt = "Choose an action for the selected army";
            txtArmyInfo.Text =
                $"Army at ({army.Territory.PositionX},{army.Territory.PositionY})\n" +
                $"Force: {army.MilitaryForce}   Energy: {army.Energy}\n" +
                $"Can act: {army.CanAct}   Can move: {army.CanMove}\n" +
                $"Installation: {inst}";
            await UpdateArmyActionButtonsAsync(army);
            ShowPanel(pnlArmyActions);
            await RefreshBoardAsync();
            return await _armyActionTcs.Task;
        }

        private async Task<char> AwaitMoveDirectionAsync(MilitaryDetachment army, List<Territory> adjacents)
        {
            _moveDirTcs = new TaskCompletionSource<char>();
            _turnPrompt = "Choose a direction (WASD or Arrow keys)";
            txtMoveInfo.Text = BuildMoveInfo(army, adjacents);
            ShowPanel(pnlMove);
            await RefreshBoardAsync();
            return await _moveDirTcs.Task;
        }

        private static string BuildMoveInfo(MilitaryDetachment army, List<Territory> adjacents)
        {
            int x = army.Territory.PositionX;
            int y = army.Territory.PositionY;
            var lines = new List<string>
            {
                $"Army at ({x},{y}) - Energy: {army.Energy}",
                "Use WASD or Arrow keys, or click a direction.",
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
                int energyCost = ArmyService.GetTerrainEnergyCost(t.TerritoryType);
                string info = $"[{dir}] ({t.PositionX},{t.PositionY}) {t.TerritoryType} - Cost: {energyCost} energy";
                if (t.MilitaryDetachment != null)
                {
                    bool enemy = t.MilitaryDetachment.PlayerId != army.PlayerId;
                    if (enemy)
                    {
                        info += $" - Enemy ({t.MilitaryDetachment.MilitaryForce}), defender terrain x{GetTerrainDefenseMultiplier(t.TerritoryType):F1}";
                    }
                    else
                    {
                        info += $" - Ally ({t.MilitaryDetachment.MilitaryForce})";
                    }
                }
                lines.Add(info);
            }
            return string.Join("\n", lines);
        }

        private static float GetTerrainDefenseMultiplier(TerritoryType territoryType)
            => territoryType switch
            {
                TerritoryType.Forest => CombatConstants.ForestMultiplayer,
                TerritoryType.Mountain => CombatConstants.MountainMultiplayer,
                _ => CombatConstants.PlainMultiplayer,
            };

        private static string DescribeTerrainDefense(TerritoryType territoryType)
            => $"defender terrain x{GetTerrainDefenseMultiplier(territoryType):F1}";

        private static string DescribeInstallationDefense(CombatResult result)
            => result.InstallationMultiplier > 1f
                ? $", installation x{result.InstallationMultiplier:F1}"
                : string.Empty;

        private static string DescribeTerrainNameFromMultiplier(float terrainMultiplier)
            => terrainMultiplier switch
            {
                >= 1.3f => "Mountain",
                >= 1.2f => "Forest",
                _ => "Plain",
            };

        private static string DescribeDefenderTerrainFromResult(CombatResult result)
            => $"Defender terrain: {DescribeTerrainNameFromMultiplier(result.TerritoryMultiplier)} ({DescribeTerrainDefenseMultiplier(result)})";

        private static string DescribeTerrainDefenseMultiplier(CombatResult result)
            => $"x{result.TerritoryMultiplier:F1}";

        private static string DescribeMovementOutcome(MoveResult result)
        {
            if (result.Fusion)
                return "Armies fused after paying the terrain cost.";
            if (result.move)
                return "Army moved and paid the terrain cost.";
            return "Move cancelled or invalid.";
        }

        private async Task<int?> AwaitNumberAsync(string prompt, int min)
        {
            _numberTcs = new TaskCompletionSource<int?>();
            _turnPrompt = prompt;
            txtNumberPrompt.Text = prompt;
            txtNumber.Text = string.Empty;
            txtNumber.Tag = min;
            ShowPanel(pnlNumber);
            await RefreshBoardAsync();
            txtNumber.Focus();
            return await _numberTcs.Task;
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

        private bool IsMoveDirectionPromptActive()
            => _moveDirTcs != null
                && !_moveDirTcs.Task.IsCompleted
                && pnlMove.Visibility == Visibility.Visible;

        private void SubmitMoveDirection(char direction)
            => _moveDirTcs?.TrySetResult(direction);

        private void GameDisplay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMoveDirectionPromptActive())
                return;

            char? direction = e.Key switch
            {
                Key.W or Key.Up => 'N',
                Key.S or Key.Down => 'S',
                Key.D or Key.Right => 'E',
                Key.A or Key.Left => 'W',
                Key.Escape => 'X',
                _ => null,
            };

            if (direction == null)
                return;

            SubmitMoveDirection(direction.Value);
            e.Handled = true;
        }

        private void Log(string line)
        {
            string stamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.Text += $"[{stamp}] {line}\n";
            scrLog.ScrollToEnd();
        }

        private void LogMoveResult(MoveResult? r)
        {
            if (r == null)
            {
                Log("Move returned nothing.");
                return;
            }

            if (r.Fight && r.CombatResult is CombatResult cr)
            {
                Log($"Combat: Atk {cr.AttackInitialForce} vs Def {cr.DefenceInitialForce}");
                Log($"  {DescribeDefenderTerrainFromResult(cr)}{DescribeInstallationDefense(cr)}");
                Log($"  Random: atk x{cr.AttackRandomFactor:F2}, def x{cr.DefenceRandomFactor:F2}");
                Log($"  Effective: atk {cr.EffectiveAttackForce:F1} / def {cr.EffectiveDefenceForce:F1}");
                Log(cr.DefenceVictory ? "  Defender wins." : "  Attacker wins.");
                Log($"  Loot: {cr.GoldGain} gold. Atk left: {cr.AttackForce}, Def left: {cr.DefenceForce}");
                return;
            }

            Log(DescribeMovementOutcome(r));
        }

        // ============================================================
        // Event handlers wired from XAML
        // ============================================================
        private void btnSelectArmy_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(1);
        private void btnSaveGame_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(2);
        private void btnMainMenu_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(3);
        private void btnEndTurn_Click(object sender, RoutedEventArgs e) => _mainActionTcs?.TrySetResult(4);

        private async void btnArmySelectConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtArmyX.Text, out int x) || !int.TryParse(txtArmyY.Text, out int y))
            {
                Log("Invalid coordinates.");
                return;
            }

            var army = await TryGetPlayableArmyAtAsync(x, y, true);
            if (army == null) return;

            _selectedArmyId = army.Id;
            await RefreshBoardAsync();
            _armySelectTcs?.TrySetResult(army);
        }

        private void btnArmySelectCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearArmySelectionState();
            _selectedArmyId = null;
            _armySelectTcs?.TrySetResult(null);
            _ = RefreshBoardAsync();
        }

        private async void OnCellClick(int x, int y)
        {
            if (_armySelectTcs == null || _armySelectTcs.Task.IsCompleted)
                return;

            var army = await TryGetPlayableArmyAtAsync(x, y, false);
            if (army == null)
                return;

            txtArmyX.Text = x.ToString();
            txtArmyY.Text = y.ToString();
            _selectedArmyId = army.Id;
            await RefreshBoardAsync();
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
                SubmitMoveDirection(tag[0]);
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

        private void btnForceEvolution_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();

            var dlg = new ForceEvolution(_context, _gameId) { Owner = this };
            if (dlg.ShowDialog() != true) return;
        }
    }
}
