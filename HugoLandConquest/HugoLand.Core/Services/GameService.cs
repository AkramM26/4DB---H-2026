using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class GameService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private int _currentPlayerNumber = 1;
        private Game? _game = null;

        public async Task CreateGameAsync(string gameName = Constants.GameConstants.CurrentGame)
        {
            await Seed.SeedGameAsync(Context);
            _game = await Context.Games.Include(g => g.Players)
                                            .Include(g => g.Territories)
                                            .ThenInclude(t => t.MilitaryDetachment)
                                            .Include(t => t.Territories)
                                            .ThenInclude(t => t.Installation)
                                        .Include(g => g.Installations).FirstAsync(g => g.SaveName == gameName);
        }

        public async Task StartTurnAsync()
        {
            // Récupérer le joueur actuel
            var player = await Context.Players.Include(p => p.MilitaryDetachments)
                                        .FirstAsync(p => p.PlayerNumber == _currentPlayerNumber);

            foreach (MilitaryDetachment m in player.MilitaryDetachments)
            {
                // Recupération d'énergie pour chaque armée
                m.Energy += Constants.GameConstants.energyRecuperation;

                // Chaque fortification occupée (armée ≥10 soldats) génère 5 or
                foreach (Installation i in _game!.Installations)
                {
                    if (i.InstallationType == InstallationType.Fortification && m.TerritoryId == i.TerritoryId)
                        player.Gold += Constants.GameConstants.ForticationGain;
                }
            }

            // Le joueur commande ses armées une par une, dans l'ordre de son choix. 1 action par armée par tour
            foreach (MilitaryDetachment m in player.MilitaryDetachments)
            {

            }

            // Un instantané de tour (TurnSnapshot) est enregistré automatiquement pour chaque joueur

        }

        public async Task LoadGameAsync(string gameName)
        {
            await CreateGameAsync(gameName);
        }

        public async Task SaveGameAsync()
        {
            await Context.SaveChangesAsync();
        }

        public async Task EndTurnAsync()
        {
            // Sauvegarde automatique
            await SaveGameAsync();

            // Passage au joueur suivant
            _currentPlayerNumber = (_currentPlayerNumber == 1) ? 2 : 1;
        }
    }
}
