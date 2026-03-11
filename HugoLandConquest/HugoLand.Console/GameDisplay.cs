using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HugoLand
{
    public class GameDisplay
    {

        public static void ShowGame(HugoLandContext context)
        {
            int xLength = 15;
            int yLength = 10;
            Game game = context.Games
                .Include(g => g.Players)
                .Include(g => g.Territories)
                .ThenInclude(t => t.MilitaryDetachment)
                .Include(t => t.Territories)
                .ThenInclude(t => t.Installation)
                .First();

            Territory[,] territories = new Territory[xLength, yLength];

            for (int y = 0; y < yLength; y++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    territories[x, y] = game.Territories.First(t => t.PositionX == x && t.PositionY == y);
                }
            }
            Console.Write("  ");
            for (int x = 0; x < xLength; x++)
            {
                if (x >= 10)
                    Console.Write("  " + x + "  ");
                else
                    Console.Write("   " + x + "  ");
            }
            Console.Write("\n");
            for (int y = 0; y < yLength; y++)
            {
                Console.Write("  ");
                for (int x = 0; x < xLength; x++)
                {
                    Console.Write("+-----");
                }
                Console.Write("+\n");
                Console.Write(y + " ");
                for (int x = 0; x < xLength; x++)
                {
                    Console.Write(CreatePosition(x, y, territories[x, y]));
                }
                Console.Write("|\n");
            }
            Console.Write("  ");
            for (int x = 0; x < xLength; x++)
            {
                Console.Write("+-----");
            }
            Console.Write("+\n");

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

        public static void AskAction(int PlayerNumber, HugoLandContext context)
        {

            var player = context.Players
                .Include(p => p.MilitaryDetachments)
                .FirstOrDefault(p => p.PlayerNumber == PlayerNumber);

            var armies = player.MilitaryDetachments
                .Where(a => a.MilitaryForce >= 10);


            Console.WriteLine("Player " + PlayerNumber);

            Console.WriteLine("Which action do you want to do?");
            Console.WriteLine("1 - Move");
            Console.WriteLine("2 - Split");
            Console.WriteLine("3 - Fusion");

            Console.Write("Enter your answer : ");

            string Answer = Console.ReadLine();

            switch (Answer)
            {
                case "1":
                    Move(context, armies);
                    break;
                case "2":
                    Split(context, armies);
                    break;
                case "3":
                    Fusion(context, armies);
                    break;
            }
        }

        private static void Move(HugoLandContext context, IEnumerable<MilitaryDetachment> armies)
        {
            ArmyService armyService = new ArmyService(context);

            foreach (var army in armies)
            {
                //Energy upgrades 
                army.Energy = army.Energy + 2;

                //Variables intialisation
                int x = army.Territory.PositionX;
                int y = army.Territory.PositionY;
                int E = army.Energy;
                int NecessaryE = 1;
                List<Movements> movements = armyService.TryMove(army, x, y);

                //Display beginning
                Console.WriteLine($"--- Movement from ({x},{y}) — Energy : {E} ---");

                //North movement 
                if (movements.Contains(Movements.North) && E >= NecessaryE)
                {
                    Console.WriteLine($"[N] ({x},{y - 1}) - Empty \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }
                else
                {
                    Console.WriteLine($"[N] ({x},{y - 1}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }

                //South movement 
                if (movements.Contains(Movements.South) && E >= NecessaryE)
                {
                    Console.WriteLine($"[S] ({x},{y + 1}) - Empty \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }
                else
                {
                    Console.WriteLine($"[S] ({x},{y + 1}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }

                //West movement 
                if (movements.Contains(Movements.West) && E >= NecessaryE)
                {
                    Console.WriteLine($"[W] ({x - 1},{y}) - Empty \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }
                else
                {
                    Console.WriteLine($"[W] ({x - 1},{y}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;

                }

                //East movement 
                if (movements.Contains(Movements.East) && E >= NecessaryE)
                {
                    Console.WriteLine($"[E] ({x + 1},{y}) - Empty \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;
                }
                else
                {
                    Console.WriteLine($"[E] ({x + 1},{y}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
                    army.Energy = E - 1;

                }


                //Choices
                string movs = "";
                foreach (var movement in movements)
                {
                    string m = movement.ToString();
                    movs+= m.Substring(1, m.Length - 2);
                    movs += "/";
                }
                movs.Substring(movs.Length - 1, 1);
                Console.WriteLine($"Choices ({movs} or X to cancel");


                Console.Write("Choose your movement :");

                string Chosenmov = "";
                Chosenmov = Console.ReadLine();

                switch (Chosenmov)
                {
                    case "N":
                        if (movements.Contains(Movements.North))
                        {
                            armyService.Move(army, x, y, Movements.North);
                            Console.WriteLine("Movement towars North done");
                        }
                        break;
                    case "S":
                        if (movements.Contains(Movements.South))
                        {
                            armyService.Move(army, x, y, Movements.South);
                            Console.WriteLine("Movement towars South done");

                        }
                        break;

                    case "E":
                        if (movements.Contains(Movements.East))
                        {
                            armyService.Move(army, x, y, Movements.East);
                            Console.WriteLine("Movement towars East done");
                        }

                        break;
                    case "W":
                        if (movements.Contains(Movements.West))
                        {
                            armyService.Move(army, x, y, Movements.West);
                            Console.WriteLine("Movement towars West done");
                        }
                        break;
                }

                //Display ending
            }


        }
        private static void Split(HugoLandContext context, IEnumerable<MilitaryDetachment> armies)
        {
            ArmyService armyService = new ArmyService(context);

        }

        private static void Fusion(HugoLandContext context, IEnumerable<MilitaryDetachment> armies)
        {
            ArmyService armyService = new ArmyService(context);

        }
    }
}
