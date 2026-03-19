using HugoLand.Core.Constants;
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
            int gamesizex = GameConstants.gameSizeX;
            int gamesizey = GameConstants.gameSizeY;
            int player1initialx = GameConstants.player1intitialx;
            int player1initialy = GameConstants.player1intitialy;
            int player2initialx = GameConstants.player2intitialx;
            int player2initialy = GameConstants.player2intitialy;

            Territory[,] territories = new Territory[gamesizex, gamesizey]; //à changer temporairement 
            for (int y = 0; y < gamesizey; y++)
            {
                for (int x = 0; x < gamesizex; x++)
                {
                    Territory territory = Territory.Create(TerritoryType.Plain, x, y, gameId);
                    territories[x, y] = territory;
                    await context.AddAsync(territory);
                }
            }

            MilitaryDetachment militaryDetachment1 = MilitaryDetachment.Create(5, 30, player1Id, territories[player1initialx, player1initialy].Id,gameId);
            MilitaryDetachment militaryDetachment2 = MilitaryDetachment.Create(5, 30, player2Id, territories[player2initialx, player2initialy].Id, gameId);
            Installation installation1 = Installation.Create(InstallationType.Fortification, territories[player1initialx, player1initialy].Id, player1Id, gameId);
            Installation installation2 = Installation.Create(InstallationType.Fortification, territories[player2initialx, player2initialy].Id, player2Id, gameId);

            await context.AddAsync(militaryDetachment1);
            await context.AddAsync(militaryDetachment2);
            await context.AddAsync(installation1);
            await context.AddAsync(installation2);
        }
    }
}
