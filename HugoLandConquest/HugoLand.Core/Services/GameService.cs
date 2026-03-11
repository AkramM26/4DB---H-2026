using Castle.Components.DictionaryAdapter.Xml;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class GameService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private int _currentPlayerNumber = 1;
        private EconomyService _economyService = new EconomyService(context);

        public async Task CreateGameAsync()
        {
            await Seed.SeedGameAsync(Context);
        }

        public async Task StartTurnAsync()
        {
            // Récupérer le joueur actuel
            var player = await Context.Players
                .Include(p => p.MilitaryDetachments)
                .FirstAsync(p => p.PlayerNumber == _currentPlayerNumber);

            foreach (MilitaryDetachment m in player.MilitaryDetachments)
            {
                // Recupération d'énergie pour chaque armée
                m.Energy += Constants.GameConstants.energyRecuperation;
                m.CanAct = true;
            }
            await _economyService.CollectRevenue(player);
            await _economyService.PayMaintenance(player);
            await Context.SaveChangesAsync();
        }

        public void LoadGameAsync(Guid id)
        {
            Context.CurrentGameId = id;
        }

        public async Task SaveGameAsync()
        {
            var gameClone = await Context.Games.AsNoTracking()
                .Include(game => game.Territories)
                    .ThenInclude(t => t.MilitaryDetachment)
                .Include(game => game.Territories)
                    .ThenInclude(t => t.Installation)
                .Include(game => game.Players)
                .Include(game => game.CombatEvents)
                .Include(game => game.PlayerActions)
                .Include(game => game.TurnSnapShots)
                .FirstAsync();

            gameClone.Id = Guid.NewGuid();
            foreach (PlayerAction pA in gameClone.PlayerActions)
            {
                pA.Id = Guid.NewGuid();
                pA.GameId = gameClone.Id;
            }
            foreach (TurnSnapShot tS in gameClone.TurnSnapShots)
            {
                tS.Id = Guid.NewGuid();
                tS.GameId = gameClone.Id;
            }
            foreach (CombatEvent c in gameClone.CombatEvents)
            {
                c.Id = Guid.NewGuid();
                c.GameId = gameClone.Id;
            }
            foreach (Player p in gameClone.Players)
            {
                p.Id = Guid.NewGuid();
                p.GameId = gameClone.Id;
            }
            foreach (Territory t in gameClone.Territories)
                CopyTerritoryAndSiblings(t, gameClone);
            await Context.AddAsync(gameClone);
            await Context.SaveChangesAsync();
        }

        private void CopyTerritoryAndSiblings(Territory t, Game gameClone)
        {
            t.Id = Guid.NewGuid();
            t.GameId = gameClone.Id;
            if (t.MilitaryDetachment != null)
            {
                int playerNumber = t.MilitaryDetachment.Player.PlayerNumber;
                var newPlayer = gameClone.Players.First(p => p.GameId == gameClone.Id && p.PlayerNumber == playerNumber);
                t.MilitaryDetachment.PlayerId = newPlayer.Id;
                t.MilitaryDetachment.Player = newPlayer;
                t.MilitaryDetachment.TerritoryId = t.Id;
                t.MilitaryDetachment.Id = Guid.NewGuid();
                t.MilitaryDetachment.GameId = gameClone.Id;
            }
            if (t.Installation != null)
            {
                int playerNumber = t.Installation.Player.PlayerNumber;
                var newPlayer = gameClone.Players.First(p => p.GameId == gameClone.Id && p.PlayerNumber == playerNumber);
                t.Installation.PlayerId = newPlayer.Id;
                t.Installation.Player = newPlayer;
                t.Installation.TerritoryId = t.Id;
                t.Installation.Id = Guid.NewGuid();
                t.Installation.GameId = gameClone.Id;
            }
        }

        public async Task EndTurnAsync()
        {
            // Un instantané de tour (TurnSnapshot) est enregistré automatiquement pour chaque joueur

            // verification si victoire

            // Passage au joueur suivant
            _currentPlayerNumber = (_currentPlayerNumber == 1) ? 2 : 1;
            await Task.CompletedTask;
        }
    }
}
