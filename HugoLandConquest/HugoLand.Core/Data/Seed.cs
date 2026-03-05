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
            Player player1 = Player.Create(50, game.Id, 1);
            Player player2 = Player.Create(50, game.Id, 2);

            await SeedMapAsync(context, game.Id, player1.Id, player2.Id);

            await context.AddAsync(game);
            await context.AddAsync(player1);
            await context.AddAsync(player2);

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

            MilitaryDetachment militaryDetachment1 = MilitaryDetachment.Create(5, 30, player1Id, territories[3, 3].Id);
            MilitaryDetachment militaryDetachment2 = MilitaryDetachment.Create(5, 30, player2Id, territories[11, 7].Id);

            await context.AddAsync(militaryDetachment1);
            await context.AddAsync(militaryDetachment2);
        }
    }
}
