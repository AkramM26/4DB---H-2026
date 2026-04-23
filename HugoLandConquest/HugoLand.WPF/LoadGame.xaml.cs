using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HugoLand.WPF
{
    public partial class LoadGame : Window
    {
        private readonly HugoLandContext _context;
        private List<SaveRow> _rows = new();

        public Guid? SelectedGameId { get; private set; }

        public LoadGame(HugoLandContext context)
        {
            InitializeComponent();
            _context = context;
            Loaded += async (_, _) => await PopulateAsync();
        }

        private async System.Threading.Tasks.Task PopulateAsync()
        {
            var games = await _context.Games
                .IgnoreQueryFilters()
                .Where(g => !g.IsTemplate)
                .OrderByDescending(g => g.SaveName)
                .ToListAsync();

            _rows = games.Select(g => new SaveRow
            {
                Id = g.Id,
                SaveName = g.SaveName,
                Summary = g.IsFinished
                    ? $"Turn {g.TurnNumber} - Finished (Winner: Player {g.WinnerPlayerNumber?.ToString() ?? "?"})"
                    : $"Turn {g.TurnNumber} - In progress (Player {g.PlayerTurn})",
            }).ToList();

            lstGames.ItemsSource = _rows;
            if (_rows.Count > 0)
                lstGames.SelectedIndex = 0;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e) => ConfirmSelection();
        private void lstGames_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => ConfirmSelection();

        private void ConfirmSelection()
        {
            if (lstGames.SelectedItem is SaveRow row)
            {
                SelectedGameId = row.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Select a save first.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private class SaveRow
        {
            public Guid Id { get; set; }
            public string SaveName { get; set; } = string.Empty;
            public string Summary { get; set; } = string.Empty;
        }
    }
}
