using HugoLand.Core.Constants;
using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class MapEditorService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private readonly GameService GameService = new GameService(context);

        public async Task<ResultService> CreateGameTemplateAsync(int gameSizeX, int gameSizeY, string gameName, string description = "")
        {
            ResultService result = await GameService.CreateGameAsync(gameSizeX, gameSizeY, gameName, true, description);

            return result;
        }
        public async Task<Game> GetGame()
        {
            var game = await Context.Games
            .Include(g => g.Players)
            .Include(g => g.Territories)
            .ThenInclude(t => t.MilitaryDetachment)
            .FirstAsync();

            return game;
        }

        public ResultService ChangeTerritoryType(int x, int y, TerritoryType territoryType, Game game)
        {
            var territory = game.Territories.FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            if (territory == null)
                return ResultService.FailureResult("The territory has not been found");
            if (territory.MilitaryDetachment != null)
                return ResultService.FailureResult("Cannot change the type of a territory that has a military detachment on it.");

            territory.TerritoryType = territoryType;

            return ResultService.SuccessResult();
        }

        public ResultService ChangeStartPosition(int playerNumber, int x, int y, Game game)
        {

            var player = game.Players.FirstOrDefault(p => p.PlayerNumber == playerNumber);

            if (player == null)
                return ResultService.FailureResult($"Player with number {playerNumber} not found in the game.");

            var oldTerritory = game.Territories.FirstOrDefault(t => t.MilitaryDetachment != null && t.MilitaryDetachment!.PlayerId == player!.Id);
            var newTerritory = game.Territories.FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            if (newTerritory == null)
                return ResultService.FailureResult($"Territory at position ({x}, {y}) not found in the game.");
            if (newTerritory.TerritoryType != TerritoryType.Plain)
                return ResultService.FailureResult($"New territory at position ({x}, {y}) is not a plain and cannot be used as a starting position.");

            if (oldTerritory != null)
            {
                if (oldTerritory.MilitaryDetachment != null)
                {
                    var entry = Context.Entry(oldTerritory.MilitaryDetachment);
                    if (entry.State == EntityState.Added)
                        entry.State = EntityState.Detached;
                    else
                        Context.Remove(oldTerritory.MilitaryDetachment);
                    oldTerritory.MilitaryDetachment = null;
                }
                if (oldTerritory.Installation != null)
                {
                    var entry = Context.Entry(oldTerritory.Installation);
                    if (entry.State == EntityState.Added)
                        entry.State = EntityState.Detached;
                    else
                        Context.Remove(oldTerritory.Installation);
                    oldTerritory.Installation = null;
                }
            }

            var newDetachment = MilitaryDetachment.Create(5, GameConstants.baseMilitaryForce, player.Id, newTerritory.Id, game.Id);
            newDetachment.Player = player;
            var newInstallation = Installation.Create(InstallationType.Fortification, newTerritory.Id, player.Id, game.Id);
            newInstallation.Player = player;

            Context.MilitaryDetachments.Add(newDetachment);
            Context.Installations.Add(newInstallation);

            newTerritory.MilitaryDetachment = newDetachment;
            newTerritory.Installation = newInstallation;

            return ResultService.SuccessResult();
        }
        public async Task<ResultService> SaveGameAsync(Game game)
        {
            int NumberOfTerritories = game.GameSizeX * game.GameSizeY;
            if ((double)game.Territories.Where(t => t.TerritoryType != TerritoryType.Ocean).Count() / NumberOfTerritories <= 0.6)
                return ResultService.FailureResult("The map must have at least 60% of accessible territories.");
            else if (!game.MilitaryDetachments.Any(m => m.Player.PlayerNumber == 1) || !game.MilitaryDetachments.Any(m => m.Player.PlayerNumber == 2))
                return ResultService.FailureResult("The map must have at least one starting position for each player.");


            if (Context.Entry(game).State == EntityState.Detached)
            {
                Context.Games.Update(game);
            }

            game.SaveName = DateTime.Now.ToString();
            await Context.SaveChangesAsync();
            return ResultService.SuccessResult();
        }

        public async Task<ResultService> DeleteGameAsync(Game game)
        {
            // Detach all tracked entities to avoid conflicts with unsaved in-memory changes,
            // then reload the game fresh from the DB before deleting.

            Context.ChangeTracker.Clear();

            var trackedGame = await Context.Games
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.Id == game.Id);

            if (trackedGame != null)
                Context.Games.Remove(trackedGame);

            await Context.SaveChangesAsync();
            return ResultService.SuccessResult();
        }
    }
}
