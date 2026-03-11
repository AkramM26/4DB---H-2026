using HugoLand.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Data
{
    public class Seed
    {
        public static async Task SeedGameAsync(HugoLandContext context)
        {
            Game game = Game.Create();
            context.CurrentGameId = game.Id;
            Player player1 = Player.Create(50, game.Id, 1);
            Player player2 = Player.Create(50, game.Id, 2);

            await context.AddAsync(game);
            await context.AddAsync(player1);
            await context.AddAsync(player2);

            await SeedMapAsync(context, game.Id, player1.Id, player2.Id);

            await context.SaveChangesAsync();
        }
        private static async Task SeedMapAsync(HugoLandContext context, Guid gameId, Guid player1Id, Guid player2Id)
        {
            Territory[,] territories = new Territory[15, 10];
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    Territory territory = Territory.Create(TerritoryType.Plain, x, y, gameId);
                    territories[x, y] = territory;
                    await context.AddAsync(territory);
                }
            }

            MilitaryDetachment militaryDetachment1 = MilitaryDetachment.Create(5, 30, player1Id, territories[3, 3].Id,gameId);
            MilitaryDetachment militaryDetachment2 = MilitaryDetachment.Create(5, 30, player2Id, territories[11, 6].Id, gameId);
            Installation installation1 = Installation.Create(InstallationType.Fortification, territories[3,3].Id, player1Id, gameId);
            Installation installation2 = Installation.Create(InstallationType.Fortification, territories[11,6].Id, player2Id, gameId);

            await context.AddAsync(militaryDetachment1);
            await context.AddAsync(militaryDetachment2);
            await context.AddAsync(installation1);
            await context.AddAsync(installation2);
        }
    }
}
