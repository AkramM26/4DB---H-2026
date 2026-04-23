using HugoLand.Core.Data;
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
    /// Interaction logic for MapEditorOptions.xaml
    /// </summary>
    public partial class MapEditorOptions : Window
    {
        private HugoLandContext Context;
        private string ConnectionString;

        public MapEditorOptions(string connectionString)
        {
            InitializeComponent();
            Context = HugoLandContextFactory.Create(connectionString);
            ConnectionString = connectionString;
        }

        private void btnCreateMap_Click(object sender, RoutedEventArgs e)
        {
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

            string gameName = txtGameName.Text;
            var window = new MapEditor(null,gameSizeX, gameSizeY, gameName, gameDescription, ConnectionString);
            window.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow();
            window.Show();
            this.Close();
        }
    }
}
