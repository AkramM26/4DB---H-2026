using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HugoLand.WPF
{
    public partial class HistoryGame : Window
    {
        private readonly HugoLandContext _context;
        private List<HistoryRow> _source = new();
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
                .Where(g => g.IsFinished)
                .OrderByDescending(g => g.EndedAt)
                .ToListAsync();

            _source = games.Select(g => new HistoryRow
            {
                Id = g.Id,
                GameName = g.GameName,
                Map = (g.IsTemplate) ? "Template" : "Normal",
                Date = g.EndedAt,
                WinnerNumber = g.WinnerPlayerNumber,
                Turns = g.TurnNumber,
                IsTemplate = g.IsTemplate
            }).ToList();
            _rows = _source.ToList();

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
            public bool IsTemplate {  get; set; } = false;
        }

        private void optTri_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null) return;

            switch (rb.Tag.ToString())
            {
                case "Date":
                    _source = _source.OrderByDescending(g => g.Date).ToList();
                    _rows = _rows.OrderByDescending(g => g.Date).ToList();
                   lstGames.ItemsSource  = _rows;
                    break;
                case "Tours":
                    _source = _source.OrderByDescending(g => g.Turns).ToList();
                    _rows = _rows.OrderByDescending(g => g.Turns).ToList();
                    lstGames.ItemsSource = _rows.OrderByDescending(g => g.Turns).ToList();
                    break;
            }
        }

        private void Filters_Changed(object sender, RoutedEventArgs e)
        {
            bool isTemplate = (chkTemplate.IsChecked == true) ? true : false, 
                isNotTemplate = (chkNotTemplate.IsChecked == true) ? true : false, 
                isWinner1 = (chkPlayer1.IsChecked == true) ? true : false, 
                isWinner2 = (chkPlayer2.IsChecked == true) ? true : false;

            _rows = _source.Where(g => (isTemplate == false || g.IsTemplate) &&
                                        (isNotTemplate == false || !g.IsTemplate) &&
                                        (isWinner1 == false || g.WinnerNumber == 1) &&
                                        (isWinner2 == false || g.WinnerNumber == 2) 
                ).ToList();

            lstGames.ItemsSource = _rows;
        }
    }
}
