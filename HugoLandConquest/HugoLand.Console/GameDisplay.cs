using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace HugoLand
{
    public class GameDisplay
    {

        public static void ShowGame(HugoLandContext context)
        {
            int gamesizex = GameConstants.gameSizeX;
            int gamesizey = GameConstants.gameSizeY;
            Game game = context.Games
                .Include(g => g.Players)
                .Include(g => g.Territories)
                .ThenInclude(t => t.MilitaryDetachment)
                .Include(t => t.Territories)
                .ThenInclude(t => t.Installation)
                .First();

            Player player = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);

            Territory[,] territories = new Territory[gamesizex, gamesizey];

            for (int y = 0; y < gamesizey; y++)
            {
                for (int x = 0; x < gamesizex; x++)
                {
                    territories[x, y] = game.Territories.First(t => t.PositionX == x && t.PositionY == y);
                }
            }
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            char playerLetter;
            if (player.PlayerNumber == 1)
                playerLetter = 'A';
            else
                playerLetter = 'B';

            Console.Write($"Turn {game.TurnNumber} - Player {playerLetter}");
            Console.Write($"Gold: {player.Gold} Income: (+{player.Income}) Cost: (-{player.Cost})     Turn In Dept: {player.TurnInDept}\n\n".PadLeft(70,' '));
            Console.Write("  ");
            for (int x = 0; x < gamesizex; x++)
            {
                if (x >= 10)
                    Console.Write("  " + x + "  ");
                else
                    Console.Write("   " + x + "  ");
            }
            Console.Write("\n");
            for (int y = 0; y < gamesizey; y++)
            {
                Console.Write("  ");
                for (int x = 0; x < gamesizex; x++)
                {
                    Console.Write("+-----");
                }
                Console.Write("+\n");
                Console.Write(y + " ");
                for (int x = 0; x < gamesizex; x++)
                {
                    Console.Write(CreatePosition(x, y, territories[x, y]));
                }
                Console.Write("|\n");
            }
            Console.Write("  ");
            for (int x = 0; x < gamesizex; x++)
            {
                Console.Write("+-----");
            }
            Console.Write("+\n");

            if (player.Gold < 0)
            {
                Console.WriteLine("You are in dept! Pay your debts to avoid losing the game!");
            }
        }

        private static string CreatePosition(int x, int y, Territory territory)
        {
            StringBuilder text = new StringBuilder("|  .  ");

            if (territory.Installation != null)
            {
                if (territory.Installation!.InstallationType == InstallationType.Fortification)
                {
                    text[1] = '[';
                    text[5] = ']';
                }
                else if (territory.Installation!.InstallationType == InstallationType.Camp)
                {
                    text[1] = '(';
                    text[5] = ')';
                }
            }
            if (territory.MilitaryDetachment != null)
            {
                text[3] = ' ';
                string militaryForce = "";
                if (territory.MilitaryDetachment.Player.PlayerNumber == 1)
                    militaryForce += 'A';
                else
                    militaryForce += 'B';
                if (territory.MilitaryDetachment.MilitaryForce < 10)
                    militaryForce = militaryForce.ToLower();
                militaryForce += territory.MilitaryDetachment.MilitaryForce;

                for (int i = 0; i < militaryForce.Length; i++)
                {
                    text[i + 2] = militaryForce[i];
                }
            }

            return text.ToString();
        }
    }
}
