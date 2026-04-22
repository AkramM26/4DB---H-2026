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

        public MapEditor(int gameSizeX, int gameSizeY,string gameName, HugoLandContext context)
        {
            InitializeComponent();
            Context = context;
            MapEditorService = new MapEditorService(Context);
            CreateGrid(gameSizeX, gameSizeY, gameName);
        }

        private async void CreateGrid(int gameSizeX, int gameSizeY, string gameName)
        {
            Game = await MapEditorService.CreateGameTemplateAsync(gameSizeX,gameSizeY, gameName, "Game Description");

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

                    var button = new Button
                    {
                        Content = $"{x},{y}"
                    };

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
    }
}
