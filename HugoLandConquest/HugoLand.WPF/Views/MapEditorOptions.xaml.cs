using HugoLand.Core.Data;
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
using System.Windows.Media;


namespace HugoLand.WPF.Views
{
    /// <summary>
    /// Interaction logic for MapEditorOptions.xaml
    /// </summary>
    public partial class MapEditorOptions : Window
    {
        private HugoLandContext Context;
        private string ConnectionString;
        private MapEditorService MapEditorService;

        private MediaPlayer smpnoise = new MediaPlayer();


        public MapEditorOptions(string connectionString)
        {
            InitializeComponent();
            Context = HugoLandContextFactory.Create(connectionString);
            MapEditorService = new MapEditorService(Context);
            ConnectionString = connectionString;

            smpnoise.Open(new Uri(@"sound/menubutton.mp3", UriKind.Relative));

        }

        private async void btnCreateMap_Click(object sender, RoutedEventArgs e)
        {
            smpnoise.Play();

            string gameDescription = txtGameDescription.Text;
            int gameSizeX;
            if (int.TryParse(txtGameSizeX.Text, out gameSizeX))
            {
                if (gameSizeX < 10 || gameSizeX > 50)
                {
                    MessageBox.Show("Game size X must be between 10 and 50.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number for Game Size X.");
                return;
            }

            int gameSizeY;
            if (int.TryParse(txtGameSizeY.Text, out gameSizeY))
            {
                if (gameSizeY < 10 || gameSizeY > 50)
                {
                    MessageBox.Show("Game size Y must be between 10 and 50.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number for Game Size Y.");
                return;
            }

            string gameName = txtGameName.Text.Trim();
            var result = await MapEditorService.CreateGameTemplateAsync(gameSizeX, gameSizeY, gameName, gameDescription);
            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }
            if (string.IsNullOrEmpty(gameName))
            {
                MessageBox.Show("Please enter a name for the map template.");
                return;
            }

            var window = new MapEditor(Context.CurrentGameId, ConnectionString);
            window.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            smpnoise.Play();
            var window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
