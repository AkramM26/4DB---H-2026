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
        public static async Task SeedGameAsync(HugoLandContext context, int gameSizeX, int gameSizeY, string gameName,bool isTemplate, string description)
        {
            if (gameSizeX < 10)
                gameSizeX = 10;
            else if (gameSizeX > 50)
                gameSizeX = 50;
            if (gameSizeY < 10)
                gameSizeY = 10;
            else if (gameSizeY > 50)
                gameSizeY = 50;

            Game game = Game.Create(gameName, gameSizeX, gameSizeY);
            if (!string.IsNullOrEmpty(description))
                game.GameDescription = description;

            context.CurrentGameId = game.Id;
            game.IsTemplate = isTemplate;

            Player player1 = Player.Create(50, game.Id, 1);
            Player player2 = Player.Create(50, game.Id, 2);

            await context.AddAsync(game);
            await context.AddAsync(player1);
            await context.AddAsync(player2);

            await SeedMapAsync(context, game.Id, player1.Id, player2.Id, gameSizeX, gameSizeY, isTemplate);

            await context.SaveChangesAsync();
        }
        private static TerritoryType GetRandomTerrainType(Random rng)
        {
            int roll = rng.Next(100);
            if (roll < 60) return TerritoryType.Plain;
            if (roll < 80) return TerritoryType.Forest;
            if (roll < 92) return TerritoryType.Mountain;
            return TerritoryType.Ocean;
        }

        private static bool IsNearStartPosition(int x, int y, int startX, int startY, int radius = 2)
        {
            return Math.Abs(x - startX) <= radius && Math.Abs(y - startY) <= radius;
        }

        private static async Task SeedMapAsync(HugoLandContext context, Guid gameId, Guid player1Id, Guid player2Id, int gameSizeX, int gameSizeY, bool isTemplate = false)
        {
            int gamesizex = gameSizeX;
            int gamesizey = gameSizeY;
            int player1initialx = 0;
            int player1initialy = 0;
            int player2initialx = gamesizex - 1;
            int player2initialy = gamesizey - 1;

            var rng = new Random();
            Territory[,] territories = new Territory[gamesizex, gamesizey];
            for (int y = 0; y < gamesizey; y++)
            {
                for (int x = 0; x < gamesizex; x++)
                {
                    TerritoryType terrainType;
                    if (IsNearStartPosition(x, y, player1initialx, player1initialy) ||
                        IsNearStartPosition(x, y, player2initialx, player2initialy))
                    {
                        terrainType = TerritoryType.Plain;
                    }
                    else
                    {
                        terrainType = GetRandomTerrainType(rng);
                    }

                    Territory territory = Territory.Create(terrainType, x, y, gameId);
                    territories[x, y] = territory;
                    await context.AddAsync(territory);
                }
            }

            if (!isTemplate)
            {
                MilitaryDetachment militaryDetachment1 = MilitaryDetachment.Create(5, 30, player1Id, territories[player1initialx, player1initialy].Id, gameId);
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
}
