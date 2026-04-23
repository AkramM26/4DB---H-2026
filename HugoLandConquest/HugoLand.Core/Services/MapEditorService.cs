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

        public async Task<Game> CreateGameTemplateAsync(int gameSizeX, int gameSizeY, string gameName, string description = "")
        {
            await GameService.CreateGameAsync(gameSizeX, gameSizeY, gameName, description);

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

            //var territory = await Context.Territories
            //    .Include(t => t.MilitaryDetachment)
            //    .FirstOrDefaultAsync(t => t.PositionX == x && t.PositionY == y);

            if (territory == null)
                return ResultService.FailureResult("The territory has not been found");
            if (territory.MilitaryDetachment != null)
                return ResultService.FailureResult("Cannot change the type of a territory that has a military detachment on it.");

            territory.TerritoryType = territoryType;
            // Context.SaveChanges();

            return ResultService.SuccessResult();
        }

        public ResultService ChangeStartPosition(int playerNumber, int x, int y, Game game)
        {
            //var game = await Context.Games
            //    .Include(g => g.Players)
            //    .Include(g => g.Territories)
            //    .ThenInclude(t => t.MilitaryDetachment)
            //    .FirstAsync();

            var player = game.Players.FirstOrDefault(p => p.PlayerNumber == playerNumber);

            if (player == null)
                return ResultService.FailureResult($"Player with number {playerNumber} not found in the game.");

            var oldTerritory = game.Territories.FirstOrDefault(t => t.MilitaryDetachment != null && t.MilitaryDetachment!.PlayerId == player!.Id);
            var newTerritory = game.Territories.FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            if (newTerritory == null)
                return ResultService.FailureResult($"Territory at position ({x}, {y}) not found in the game.");
            if (oldTerritory == null)
                return ResultService.FailureResult($"Current territory for player {playerNumber} not found in the game.");
            if (newTerritory.TerritoryType != TerritoryType.Plain)
                return ResultService.FailureResult($"New territory at position ({x}, {y}) is not a plain and cannot be used as a starting position.");

            oldTerritory.MilitaryDetachment = null;
            newTerritory.MilitaryDetachment = MilitaryDetachment.Create(5, GameConstants.baseMilitaryForce, player, newTerritory, game);

            oldTerritory.Installation = null;
            newTerritory.Installation = Installation.Create(InstallationType.Fortification, newTerritory, player, game);

             //Context.SaveChanges();

            return ResultService.SuccessResult();
        }
        public async Task<ResultService> SaveGameAsync(Game game)
        {
            game.SaveName = DateTime.Now.ToString();
            await Context.SaveChangesAsync();
            return ResultService.SuccessResult();
        }

        public async Task<ResultService> DeleteGameAsync(Game game)
        {
            Context.Games.Remove(game);
            await Context.SaveChangesAsync();
            return ResultService.SuccessResult();
        }
    }
}
