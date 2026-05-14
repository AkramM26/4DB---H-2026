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
    public partial class PlayerHistory : Window
    {
        private readonly HugoLandContext _context;
        private List<PlayerStats> _playerStats;

        public PlayerHistory(HugoLandContext context)
        {
            InitializeComponent();
            _context = context;
            _playerStats = new()
            {
                new PlayerStats(1),
                new PlayerStats(2)
            };
            cmbPlayer.SelectionChanged += cmbPlayer_SelectionChanged;
            Loaded += async (_, _) => await PopulateAsync();
        }

        private async System.Threading.Tasks.Task PopulateAsync()
        {
            var games = await _context.Games
                .IgnoreQueryFilters()
                .Where(g => g.IsFinished)
                .ToListAsync();

            var turnSnapShots = await _context.TurnSnapShots
                .IgnoreQueryFilters()
                .Include(t => t.Game)
                .Where(t => t.Game.IsFinished)
                .ToListAsync();

            foreach (PlayerStats player in _playerStats)
            {
                List<Game> gameGagnes = games.Where(g => g.WinnerPlayerNumber == player.Number).ToList();
                player.TauxVictoire = ((decimal)gameGagnes.Count / games.Count) * 100;
                player.ToursVictoire = (decimal)gameGagnes.Average(g => g.TurnNumber);
                player.OrMoyen = turnSnapShots.Where(t => t.PlayerNumber == player.Number).Sum(t => t.Gold) / games.Count;
            }

            UpdateSelectionResult();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();
            DialogResult = false;
        }

        private void cmbPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionResult();
        }

        private void UpdateSelectionResult()
        {
            int option = cmbPlayer.SelectedIndex + 1;
            if (option < 1)
                return;

            PlayerStats player = _playerStats.First(p => p.Number == option);
            if (player == null) return;

            string resultat = $"- Taux de victoire (%) : {player.TauxVictoire}\n- Tours par victoire : {player.ToursVictoire}\n- Or moyen par partie : {player.OrMoyen}";
            txtPlayerhistory.Text = resultat;
        }

        public class PlayerStats
        {
            public int Number { get; private set; }
            public decimal TauxVictoire { get; set; }
            public decimal ToursVictoire { get; set; }
            public decimal OrMoyen { get; set; }

            public PlayerStats(int playerNumber)
            {
                Number = playerNumber;
            }
        }
    }
}
