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


        public async Task ChangeTerritoryTypeAsync(int x, int y, TerritoryType territoryType)
        {
            var territory = await Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstOrDefaultAsync(t => t.PositionX == x && t.PositionY == y);

            if (territory == null)
                throw new Exception("The territory has not been found");
            if (territory.MilitaryDetachment != null)
                throw new Exception("Cannot change the type of a territory that has a military detachment on it.");

            territory.TerritoryType = territoryType;
            await Context.SaveChangesAsync();
        }

        public async Task ChangeStartPositionAsync(int playerNumber, int x, int y)
        {
            var game = await Context.Games
                .Include(g => g.Players)
                .Include(g => g.Territories)
                .ThenInclude(t => t.MilitaryDetachment)
                .FirstOrDefaultAsync(g => g.Players.Any(p => p.PlayerNumber == playerNumber));

            var player = game!.Players.FirstOrDefault(p => p.PlayerNumber == playerNumber);

            if (player == null)
                throw new Exception($"Player with number {playerNumber} not found in the game.");

            var oldTerritory = game.Territories.FirstOrDefault(t => t.MilitaryDetachment != null && t.MilitaryDetachment!.PlayerId == player!.Id);
            var newTerritory = game.Territories.FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            if (newTerritory == null)
                throw new Exception($"Territory at position ({x}, {y}) not found in the game.");
            if (oldTerritory == null)
                throw new Exception($"Current territory for player {playerNumber} not found in the game.");
            if (newTerritory.TerritoryType != TerritoryType.Plain)
                throw new Exception($"New territory at position ({x}, {y}) is not a plain and cannot be used as a starting position.");

            oldTerritory.MilitaryDetachment = null;
            newTerritory.MilitaryDetachment = MilitaryDetachment.Create(5, GameConstants.baseMilitaryForce, player.Id, newTerritory.Id, game.Id);

            oldTerritory.Installation = null;
            newTerritory.Installation = Installation.Create(InstallationType.Fortification, newTerritory.Id, player.Id, game.Id);

            game.SaveName = DateTime.Now.ToString();

            await Context.SaveChangesAsync();
        }
    }
}
