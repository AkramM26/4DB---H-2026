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
    /// Interaction logic for MapTemplateList.xaml
    /// </summary>
    public partial class MapTemplateList : Window
    {
        private readonly string ConnectionString;
        private readonly HugoLandContext Context;
        public MapTemplateList(string connectionString)
        {
            InitializeComponent();
            ConnectionString = connectionString;
            Context = HugoLandContextFactory.Create(ConnectionString);
            Loaded += (_, _) => CreateTemplateList();
        }

        private void CreateTemplateList()
        {

            var maps = Context.Games.IgnoreQueryFilters()
                .Where(m => m.IsTemplate == true)
                .Select(g => new
                {
                    g.Id,
                    g.GameName,
                    g.GameDescription,
                })
                .ToList();

            lstMap.ItemsSource = maps;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var id = (Guid)btn.Tag;
                var map = Context.Games.IgnoreQueryFilters()
                    .FirstOrDefault(m => m.Id == id);

                if (map != null)
                {
                    Context.Games.Remove(map);
                    Context.SaveChanges();
                }

                CreateTemplateList();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var id = (Guid)btn.Tag;
                var map = Context.Games.IgnoreQueryFilters()
                    .Include(g => g.Territories)
                    .FirstOrDefault(m => m.Id == id);

                int gameSizeX = map.Territories.Max(t => t.PositionX) -1;
                int gameSizeY = map.Territories.Max(t => t.PositionY) -1;
                string gameName = map.GameName;
                string gameDescription = map.GameDescription;

                if (map != null)
                {
                    var window = new MapEditor(map, gameSizeX, gameSizeY, gameName, gameDescription, ConnectionString);
                    window.Show();
                    this.Close();
                }
            }
        }
    }
}
