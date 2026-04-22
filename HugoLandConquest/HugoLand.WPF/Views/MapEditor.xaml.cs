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
        private Game Game;
        private MapEditorService MapEditorService;

        public MapEditor(int gameSizeX, int gameSizeY, string gameName, string gameDescription, HugoLandContext context)
        {
            InitializeComponent();
            Context = context;
            MapEditorService = new MapEditorService(Context);
            CreateGrid(gameSizeX, gameSizeY, gameName, gameDescription);
        }

        private async void CreateGrid(int gameSizeX, int gameSizeY, string gameName, string gameDescription)
        {
            Game = await MapEditorService.CreateGameTemplateAsync(gameSizeX, gameSizeY, gameName, gameDescription);

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
                    int x = i;
                    int y = j;

                    var territory = Game.Territories.First(t => t.PositionX == x && t.PositionY == y);

                    var button = new Button
                    {
                        //Content = $"{x},{y}"
                    };

                    if (territory.TerritoryType == TerritoryType.Plain)
                        button.Background = Brushes.LightGreen;
                    else if (territory.TerritoryType == TerritoryType.Forest)
                        button.Background = Brushes.ForestGreen;
                    else if (territory.TerritoryType == TerritoryType.Mountain)
                        button.Background = Brushes.Gray;

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
                        MessageBox.Show($"Clicked: {x},{y}");
                    };

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);

                    grdMap.Children.Add(button);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await MapEditorService.DeleteGameAsync(Game);
            var window = new MapEditorOptions(Context);
            window.Show();
            this.Close();
        }
    }
}
