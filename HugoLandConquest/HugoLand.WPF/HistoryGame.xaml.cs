using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HugoLand.WPF
{
    public partial class HistoryGame : Window
    {
        private readonly HugoLandContext _context;
        private List<HistoryRow> _rows = new();

        public Guid? SelectedGameId { get; private set; }

        public HistoryGame(HugoLandContext context)
        {
            InitializeComponent();
            _context = context;
            Loaded += async (_, _) => await PopulateAsync();

        }

        private async System.Threading.Tasks.Task PopulateAsync()
        {
            var games = await _context.Games
                .IgnoreQueryFilters()
                .Where(g => !g.IsTemplate && g.IsFinished)
                .OrderByDescending(g => g.EndedAt)
                .ToListAsync();

            _rows = games.Select(g => new HistoryRow
            {
                Id = g.Id,
                GameName = g.GameName,
                Map = "test", // mettre le nom de la map
                Date = g.EndedAt,
                WinnerNumber = g.WinnerPlayerNumber,
                Turns = g.TurnNumber
            }).ToList();

            lstGames.ItemsSource = _rows;
            if (_rows.Count > 0)
                lstGames.SelectedIndex = 0;
        }
        private void lstGames_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (lstGames.SelectedItem is HistoryRow row)
            {
                SelectedGameId = row.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Select a game first.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();
            DialogResult = false;
        }


        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();
            DialogResult = false;
        }
        private class HistoryRow
        {
            public Guid Id { get; set; }
            public string GameName { get; set; } = string.Empty;
            public string Map { get; set; } = string.Empty;
            public DateTime? Date { get; set; } = DateTime.UtcNow;
            public int? WinnerNumber { get; set; } = 0;
            public int Turns { get; set; } = 0;
        }
    }
}
