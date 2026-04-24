using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
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
    /// Interaction logic for MapEditor.xaml
    /// </summary>
    public partial class MapEditor : Window
    {
        private HugoLandContext Context;
        private Game? Game;
        private readonly MapEditorService MapEditorService;
        private readonly string ConnectionString;
        private readonly int InitialGameSizeX;
        private readonly int InitialGameSizeY;
        private readonly string InitialGameName;
        private readonly string InitialGameDescription;

        public MapEditor(Game? game,int gameSizeX, int gameSizeY, string gameName, string gameDescription, string connectionString)
        {
            InitializeComponent();
            Context = HugoLandContextFactory.Create(connectionString);
            ConnectionString = connectionString;
            MapEditorService = new MapEditorService(Context);
            Game = game;
            InitialGameSizeX = gameSizeX;
            InitialGameSizeY = gameSizeY;
            InitialGameName = gameName;
            InitialGameDescription = gameDescription;
            Loaded += MapEditor_Loaded;
        }

        private async void MapEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MapEditor_Loaded;

            if (Game == null)
                Game = await MapEditorService.CreateGameTemplateAsync(InitialGameSizeX, InitialGameSizeY, InitialGameName, InitialGameDescription);

            CreateGrid();
        }

        private void CreateGrid()
        {
            int gameSizeX = Game!.GameSizeX;
            int gameSizeY = Game.GameSizeY;

            for (int i = 0; i < gameSizeY; i++)
            {
                grdMap.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < gameSizeX; j++)
            {
                grdMap.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < gameSizeY; i++)
            {
                for (int j = 0; j < gameSizeX; j++)
                {
                    int x = j;
                    int y = i;

                    var territory = Game!.Territories.First(t => t.PositionX == x && t.PositionY == y);

                    var button = new Button
                    {
                        //Content = $"{x},{y}"
                    };

                    if (territory.TerritoryType == TerritoryType.Plain)
                        button.Background = Brushes.LightGreen;
                    else if (territory.TerritoryType == TerritoryType.Forest)
                        button.Background = Brushes.DarkGreen;
                    else if (territory.TerritoryType == TerritoryType.Mountain)
                        button.Background = Brushes.Gray;
                    else if (territory.TerritoryType == TerritoryType.Ocean)
                        button.Background = Brushes.LightBlue;

                    if (territory.MilitaryDetachment != null)
                    {
                        button.Content = territory.MilitaryDetachment.MilitaryForce;

                        if (territory.MilitaryDetachment.Player.PlayerNumber == 1)
                            button.Foreground = Brushes.Red;
                        else if (territory.MilitaryDetachment.Player.PlayerNumber == 2)
                            button.Foreground = Brushes.Blue;
                    }

                    button.Click += (s, e) =>
                    {
                        if (radioForest.IsChecked == true)
                            MapEditorService.ChangeTerritoryType(x, y, TerritoryType.Forest, Game!);
                        else if (radioMountain.IsChecked == true)
                            MapEditorService.ChangeTerritoryType(x, y, TerritoryType.Mountain, Game!);
                        else if (radioPlain.IsChecked == true)
                            MapEditorService.ChangeTerritoryType(x, y, TerritoryType.Plain, Game!);
                        else if (radioOcean.IsChecked == true)
                            MapEditorService.ChangeTerritoryType(x, y, TerritoryType.Ocean, Game!);
                        else if (radioPlayer1.IsChecked == true)
                            MapEditorService.ChangeStartPosition(1, x, y, Game!);
                        else if (radioPlayer2.IsChecked == true)
                            MapEditorService.ChangeStartPosition(2, x, y, Game!);
                        UpdateGrid();
                    };

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);

                    grdMap.Children.Add(button);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await MapEditorService.SaveGameAsync(Game!);
            var window = new MainWindow();
            window.Show();
            this.Close();
        }

        private void UpdateGrid()
        {
            foreach (var child in grdMap.Children)
            {
                if (child is Button button)
                {
                    int y = Grid.GetRow(button);
                    int x = Grid.GetColumn(button);
                    var territory = Game!.Territories.First(t => t.PositionX == x && t.PositionY == y);
                    if (territory.TerritoryType == TerritoryType.Plain)
                        button.Background = Brushes.LightGreen;
                    else if (territory.TerritoryType == TerritoryType.Forest)
                        button.Background = Brushes.DarkGreen;
                    else if (territory.TerritoryType == TerritoryType.Mountain)
                        button.Background = Brushes.Gray;
                    else if (territory.TerritoryType == TerritoryType.Ocean)
                        button.Background = Brushes.LightBlue;
                    if (territory.MilitaryDetachment != null)
                    {
                        button.Content = territory.MilitaryDetachment.MilitaryForce;
                        if (territory.MilitaryDetachment.Player.PlayerNumber == 1)
                            button.Foreground = Brushes.Red;
                        else if (territory.MilitaryDetachment.Player.PlayerNumber == 2)
                            button.Foreground = Brushes.Blue;
                    }
                    else
                    {
                        button.Content = null;
                    }
                }
            }
        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            await MapEditorService.DeleteGameAsync(Game!);
            var window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
