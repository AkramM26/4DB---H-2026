using HugoLand.Core.Data;
using HugoLand.WPF.Views;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HugoLand.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HugoLandContext Context;
        public MainWindow()
        {
            InitializeComponent();
            var context = new HugoLandContextFactory().CreateDbContext([]);
            // need to be changed when installing to a db not in memory!!!
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            Context = context;
        }

        private void btnOpenGame_Click(object sender, RoutedEventArgs e)
        {
            GameDisplay fenetreJeu = new GameDisplay();
            fenetreJeu.Show();
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            GameDisplay fenetreJeu = new GameDisplay();
            fenetreJeu.Show();
        }

        private void btnMapTemplate_Click(object sender, RoutedEventArgs e)
        {
            var window = new MapEditorOptions(Context);
            window.Show();
            this.Close();
        }

    }
}