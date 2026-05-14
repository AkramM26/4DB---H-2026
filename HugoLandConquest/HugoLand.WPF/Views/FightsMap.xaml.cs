using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HugoLand.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour FightsMap.xaml
    /// </summary>
    public partial class FightsMap : Window
    {
        private HugoLandContext _context;
        private Guid _gameId;
        private Border[,]? _cells;
        private TextBlock[,]? _cellLabels;

        private TaskCompletionSource<MilitaryDetachment?>? _armySelectTcs;
        private bool _windowClosing;
        private ICollection<Territory> _territories;
        private ICollection<CombatEvent> _combatevents;

        public FightsMap(HugoLandContext context, Guid gameId)
        {
            InitializeComponent();
            _context = context;
            _gameId = gameId;
            InitializeMap();
        }

        private void InitializeMap()
        {
            _windowClosing = false;
            try
            {
                BuildBoard();
                FightsIntensity();
            }
            catch (Exception ex)
            {
                if (!_windowClosing)
                    MessageBox.Show($"Error in game loop:\n{ex}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private void BuildBoard()
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == _gameId);
            int w = game.GameSizeX;
            int h = game.GameSizeY;

            _territories = game.Territories;
            _combatevents = game.CombatEvents;


            grdBoard.RowDefinitions.Clear();
            grdBoard.ColumnDefinitions.Clear();
            grdBoard.Children.Clear();

            const int RightReserve = 288;
            const int TopReserve = 110;
            const int BotReserve = 132;
            const int ChromeReserve = 40;
            const int Margins = 32;
            const int RowHeaderW = 26;
            const int ColHeaderH = 22;

            var screen = SystemParameters.WorkArea;
            double availW = screen.Width - RightReserve - RowHeaderW - Margins;
            double availH = screen.Height - TopReserve - BotReserve - ColHeaderH - Margins - ChromeReserve;

            int cellSize = (int)Math.Floor(Math.Min(availW / w, availH / h));
            cellSize = Math.Clamp(cellSize, 20, 64);

            // Resize and re-centre the window on the current screen
            double newW = Math.Min(RowHeaderW + cellSize * w + RightReserve + Margins, screen.Width);
            double newH = Math.Min(ColHeaderH + cellSize * h + TopReserve + BotReserve + Margins + ChromeReserve, screen.Height);
            Width = newW;
            Height = newH;
            Left = screen.Left + (screen.Width - newW) / 2;
            Top = screen.Top + (screen.Height - newH) / 2;

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
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 10,
                    Foreground = headerFg,
                    Visibility = showHeaders ? Visibility.Visible : Visibility.Hidden,
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
                    FontSize = 10,
                    Foreground = headerFg,
                    Visibility = showHeaders ? Visibility.Visible : Visibility.Hidden,
                };
                Grid.SetRow(header, y + 1);
                Grid.SetColumn(header, 0);
                grdBoard.Children.Add(header);
            }

            _cells = new Border[w, h];
            _cellLabels = new TextBlock[w, h];

            var cellBorder = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            var cellBg = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));

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
                        Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    };
                    var border = new Border
                    {
                        BorderBrush = cellBorder,
                        BorderThickness = new Thickness(0.5),
                        Background = cellBg,
                        Cursor = Cursors.Arrow,
                        Child = tb,
                    };
                    Grid.SetColumn(border, x + 1);
                    Grid.SetRow(border, y + 1);

                    int capturedX = x;
                    int capturedY = y;
                    //border.MouseLeftButtonUp += (_, _) => OnCellClick(capturedX, capturedY);

                    grdBoard.Children.Add(border);
                    _cells[x, y] = border;
                    _cellLabels[x, y] = tb;
                }
            }
        }

        private async Task RefreshBoardAsync()
        {
            var game = _context.Games.FirstOrDefault(g => g.Id == _gameId);
            //    $"Turn {game.TurnNumber} - Player {game.PlayerTurn}   |   " +
            //    $"Gold: {currentPlayer.Gold}   Income: +{currentPlayer.Income}   Cost: -{currentPlayer.Cost}   " +
            //    $"Debt turns: {currentPlayer.TurnInDept}";
            //UpdateTurnBanner(game);

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


                    //cell.Background = t.TerritoryType switch
                    //{
                    //    TerritoryType.Plain => Brushes.LightGreen,
                    //    TerritoryType.Forest => Brushes.DarkGreen,
                    //    TerritoryType.Mountain => Brushes.Gray,
                    //    TerritoryType.Ocean => Brushes.LightBlue,
                    //    _ => Brushes.LightGreen,
                    //};
                    cell.BorderBrush = Brushes.Black;
                    cell.BorderThickness = new Thickness(1);
                    cell.Effect = null;
                    cell.Cursor = Cursors.Arrow;



                    label.Text = FormatCell(t);
                    Brush defaultForeground = t.TerritoryType == TerritoryType.Forest ? Brushes.White : Brushes.Black;
                    label.Foreground = t.MilitaryDetachment?.Player.PlayerNumber == 1
                        ? Brushes.DarkBlue
                        : t.MilitaryDetachment != null ? Brushes.DarkRed : defaultForeground;
                }
            }
        }

        private void FightsIntensity()
        {
            foreach (var c in _combatevents)
            {
                var cell = _cells[c.TerritoryDefendedPosX, c.TerritoryDefendedPosY];
                var label = _cellLabels[c.TerritoryDefendedPosX, c.TerritoryDefendedPosY];

                var t = _territories.FirstOrDefault(tt => tt.PositionX == c.TerritoryDefendedPosX && tt.PositionY == c.TerritoryDefendedPosY);
                cell.Background = c.VictorPlayerNumber == 1 ? new SolidColorBrush(Colors.DarkBlue): new SolidColorBrush(Colors.DarkRed);
                label.Text = c.VictorPlayerNumber == 1 ? "P1" : "P2";

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


    }
}
