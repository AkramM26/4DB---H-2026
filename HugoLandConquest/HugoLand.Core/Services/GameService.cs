using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class GameService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private readonly EconomyService _economyService = new(context);

        private async Task<Game> GetCurrentGameAsync()
        {
            return await Context.Games.FirstAsync();
        }

        public async Task<ResultService> CreateGameAsync(int gameSizeX, int gameSizeY, string gameName, bool isTemplate, string description = "")
        {
            if (isTemplate)
            {
                var games = await Context.Games
                .IgnoreQueryFilters()
                .AnyAsync(g => g.IsTemplate == true && g.GameName == gameName);
                if (games)
                    return ResultService.FailureResult("A template with the same name already exists. Please choose a different name.");
            }

            await Seed.SeedGameAsync(Context, gameSizeX, gameSizeY, gameName, isTemplate, description);
            return ResultService.SuccessResult();
        }

        public async Task StartTurnAsync()
        {
            var game = await GetCurrentGameAsync();
            if (game.IsFinished)
                return;

            var player = await Context.Players
                .Include(p => p.MilitaryDetachments)
                .FirstAsync(p => p.PlayerNumber == game.PlayerTurn);

            foreach (MilitaryDetachment m in player.MilitaryDetachments)
            {
                m.Energy += GameConstants.energyRecuperation;
                m.CanAct = true;
                m.CanMove = true;
            }

            await _economyService.CollectRevenue(player);
            await _economyService.PayMaintenance(player);
            await Context.SaveChangesAsync();

        }

        public void LoadGame(Guid id)
        {
            Context.CurrentGameId = id;
        }

        public async Task<Guid> SaveGameAsync()
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
            gameClone.SaveName = DateTime.Now.ToString();
            gameClone.IsTemplate = false;
            await Context.AddAsync(gameClone);
            await Context.SaveChangesAsync();

            return gameClone.Id;
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
            var game = await GetCurrentGameAsync();
            var currentPlayerNumber = game.PlayerTurn;

            var player = await Context.Players
                .Include(p => p.MilitaryDetachments)
                .Include(p => p.Installations)
                .FirstAsync(p => p.PlayerNumber == currentPlayerNumber);

            var snapshot = TurnSnapShot.Create(player.GameId, player.PlayerNumber, player.Gold,
                player.MilitaryDetachments.Count, player.MilitaryDetachments.Sum(m => m.MilitaryForce),
                player.Installations.Count(i => i.InstallationType == InstallationType.Fortification));


            await Context.TurnSnapShots.AddAsync(snapshot);
            //Ajouter la force militaire actuelle au tableau 
            if (game.TurnNumber == 1)
            {
                if (player.ForcesTable == null) player.ForcesTable = new List<int>();
                player.ForcesTable.Add(Constants.GameConstants.baseMilitaryForce);

            }
            player.ForcesTable.Add(player.MilitaryDetachments.Sum(m => m.MilitaryForce));

            var otherPlayerNumber = currentPlayerNumber == 1 ? 2 : 1;


            var otherPlayer = await Context.Players
                .Include(p => p.MilitaryDetachments)
                .FirstAsync(p => p.PlayerNumber == otherPlayerNumber);

            //Ajouter la force militaire actuelle au tableau 
            if (game.TurnNumber == 1)
            {
                if (otherPlayer.ForcesTable == null) otherPlayer.ForcesTable = new List<int>();
                otherPlayer.ForcesTable.Add(Constants.GameConstants.baseMilitaryForce);

            }
            otherPlayer.ForcesTable.Add(otherPlayer.MilitaryDetachments.Sum(m => m.MilitaryForce));

            if (!otherPlayer.MilitaryDetachments.Any())
            {
                game.IsFinished = true;
                game.MilitaryVictory = true;
                game.WinnerPlayerNumber = currentPlayerNumber;
                game.EndedAt = DateTime.UtcNow;
            }
            else if (player.TurnInDept >= 5)
            {
                game.IsFinished = true;
                game.MilitaryVictory = false;
                game.WinnerPlayerNumber = otherPlayerNumber;
                game.EndedAt = DateTime.UtcNow;
            }
            else if (otherPlayer.TurnInDept >= 5)
            {
                game.IsFinished = true;
                game.MilitaryVictory = false;
                game.WinnerPlayerNumber = currentPlayerNumber;
                game.EndedAt = DateTime.UtcNow;
            }
            else
            {
                game.PlayerTurn = otherPlayerNumber;
            }

            game.TurnNumber++;
            await Context.SaveChangesAsync();
        }

        private int FinalForce(Player player)
        {
            int force = 0;
            foreach (var militarydetachement in player.MilitaryDetachments)
            {
                force += militarydetachement.MilitaryForce;
            }
            return force;
        }
    }
}
