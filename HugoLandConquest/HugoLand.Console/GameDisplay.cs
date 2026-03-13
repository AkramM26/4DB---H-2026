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
            int xLength = 15;
            int yLength = 10;
            Game game = context.Games
                .Include(g => g.Players)
                .Include(g => g.Territories)
                .ThenInclude(t => t.MilitaryDetachment)
                .Include(t => t.Territories)
                .ThenInclude(t => t.Installation)
                .First();

            Player player = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);

            Territory[,] territories = new Territory[xLength, yLength];

            for (int y = 0; y < yLength; y++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    territories[x, y] = game.Territories.First(t => t.PositionX == x && t.PositionY == y);
                }
            }
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            char playerLetter;
            char playerIncome;
            if (player.PlayerNumber == 1)
                playerLetter = 'A';
            else
                playerLetter = 'B';
            if (player.Income >= 0)
                playerIncome = '+';
            else
                playerIncome = '-';
            Console.Write($"Turn {game.TurnNumber} - Player {playerLetter}");
            Console.Write($"Gold: {player.Gold}({playerIncome}{player.Income})     Turn In Dept: {player.TurnInDept}\n\n".PadLeft(70,' '));
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

        //public static void AskAction(int PlayerNumber, HugoLandContext context)
        //{

        //    var player = context.Players
        //        .Include(p => p.MilitaryDetachments)
        //        .FirstOrDefault(p => p.PlayerNumber == PlayerNumber);

        //    var armies = player.MilitaryDetachments
        //        .Where(a => a.MilitaryForce >= 10);

        //    foreach (var army in armies)
        //    {
        //        int startLine = Console.CursorTop;

        //        army.Energy += GameConstants.energyRecuperation;
        //        army.CanAct = true;

        //        bool redo;

        //        Console.WriteLine("Player " + PlayerNumber);
        //        Console.WriteLine("Which action do you want to do?");
        //        Console.WriteLine("1 - Move");
        //        Console.WriteLine("2 - Split");
        //        Console.WriteLine("3 - Reinforce");
        //        Console.WriteLine("4 - Do Nothing");
        //        Console.Write("Enter your answer : ");

        //        do
        //        {
        //            redo = true;

        //            string Answer = Console.ReadLine();

        //            switch (Answer)
        //            {
        //                case "1":
        //                    Move(context, army, player);
        //                    break;

        //                case "2":
        //                    redo = SplitPossible(context, army);
        //                    if (redo)
        //                        Split(context, army, player);
        //                    break;

        //                case "3":
        //                    Reinforce(context, army, player);
        //                    break;

        //                case "4":
        //                    break;
        //            }

        //        } while (!redo);

        //        Console.WriteLine();
        //        Console.WriteLine("APPUYER SUR UNE TOUCHE POUR CONTINUER ");
        //        Console.ReadKey();
        //        int lastBufferLine = Console.BufferHeight - 1;

        //        for (int i = startLine; i <= lastBufferLine; i++)
        //        {
        //            Console.SetCursorPosition(0, i);
        //            Console.Write(new string(' ', Console.WindowWidth));
        //        }

        //        Console.SetCursorPosition(0, startLine);
        //    }


        //}

        private static void Reinforce(HugoLandContext context, MilitaryDetachment army, Player player)
        {
            ArmyService armyService = new ArmyService(context);


            Console.Write("How much soldiers do you want to add to your army ? :");
            string answer = "";
            answer = Console.ReadLine();
            int SoldierForReinfocement = 0;
            bool reiforcement = int.TryParse(answer, out SoldierForReinfocement);

            if (!reiforcement)
                throw new Exception("You did not entered a valid number for the reinforcement !!!");

            Console.WriteLine($"You chose to reinforce your army with {SoldierForReinfocement} soldiers");

            armyService.Reinforce( army, SoldierForReinfocement, player);
        }

        //private static void Move(HugoLandContext context, MilitaryDetachment army, Player player)

        //{
        //    ArmyService armyService = new ArmyService(context);
        //    char S = ' ';
        //    if (player.PlayerNumber == 1)
        //        S = 'A';
        //    else if (player.PlayerNumber == 2)
        //        S = 'B';




        //    //Variables intialisation
        //    int x = army.Territory.PositionX;
        //    int y = army.Territory.PositionY;
        //    int E = army.Energy;
        //    int NecessaryE = 1;
        //    List<Movements> movements = armyService.TryMove(army, x, y);


        //    //Display beginning
        //    Console.WriteLine($"--- Movement from ({x},{y}) — Energy : {E} ---{S}{army.MilitaryForce}");

        //    //North movement 
        //    if (movements.Contains(Movements.North) && E >= NecessaryE)
        //    {
        //        Console.WriteLine($"[N] ({x},{y - 1}) - Empty \t Cost : 1 {NecessaryE} énergie ");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[N] ({x},{y - 1}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
        //    }

        //    //South movement 
        //    if (movements.Contains(Movements.South) && E >= NecessaryE)
        //    {
        //        Console.WriteLine($"[S] ({x},{y + 1}) - Empty \t Cost : 1 {NecessaryE} énergie ");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[S] ({x},{y + 1}) - Opponent \t Cost : 1 {NecessaryE} énergie ");
        //    }

        //    //East movement 
        //    if (movements.Contains(Movements.East) && E >= NecessaryE)
        //    {
        //        Console.WriteLine($"[E] ({x + 1},{y}) - Empty \t Cost : {NecessaryE} énergie ");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[E] ({x + 1},{y}) - Opponent \t Cost : {NecessaryE} énergie ");

        //    }
        //    //West movement 
        //    if (movements.Contains(Movements.West) && E >= NecessaryE)
        //    {
        //        Console.WriteLine($"[W] ({x - 1},{y}) - Empty \t Cost : {NecessaryE} énergie ");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"[W] ({x - 1},{y}) - Opponent \t Cost : {NecessaryE} énergie ");

        //    }



        //    //Choices
        //    string movs = "";
        //    foreach (var movement in movements)
        //    {
        //        string m = movement.ToString();
        //        movs += m.Substring(0, 1);
        //        movs += "/";
        //    }
        //    movs = movs.Substring(0, movs.Length - 1);
        //    Console.WriteLine($"Choices ({movs} or X to cancel");


        //    Console.Write($"Choose your movement:");

        //    string Chosenmov = "";
        //    Chosenmov = Console.ReadLine();

        //    switch (Chosenmov)
        //    {
        //        case "N":
        //        case "n":

        //            if (movements.Contains(Movements.North))
        //            {
        //                armyService.Move(army, x, y, Movements.North);
        //                Console.WriteLine("Movement towards North done");
        //            }
        //            break;
        //        case "S":
        //        case "s":

        //            if (movements.Contains(Movements.South))
        //            {
        //                armyService.Move(army, x, y, Movements.South);
        //                Console.WriteLine("Movement towards South done");
        //            }
        //            break;

        //        case "E":
        //        case "e":

        //            if (movements.Contains(Movements.East))
        //            {
        //                armyService.Move(army, x, y, Movements.East);
        //                Console.WriteLine("Movement towards East done");
        //            }

        //            break;
        //        case "W":
        //        case "w":

        //            if (movements.Contains(Movements.West))
        //            {
        //                armyService.Move(army, x, y, Movements.West);
        //                Console.WriteLine("Movement towards West done");
        //            }
        //            break;
        //        case "X":
        //        case "x":

        //            Console.WriteLine("Movement canceled");
        //            break;
        //    }

        //    //Display ending


        //}
        //private static void Split(HugoLandContext context, MilitaryDetachment army, Player player)
        //{
        //    ArmyService armyService = new ArmyService(context);

        //    int x = army.Territory.PositionX;
        //    int y = army.Territory.PositionY;

        //    Console.WriteLine($"How many soldiers do you want to use for the split(Less than {army.MilitaryForce}) :");
        //    string Answer = "";
        //    Answer = Console.ReadLine();
        //    int SoldierNumberForSplit = 0;
        //    bool splt = int.TryParse(Answer, out SoldierNumberForSplit);

        //    if (!splt)
        //        throw new Exception("You did not entered a valid number for the split !!!");

        //    if (army.MilitaryForce - SoldierNumberForSplit < 1)
        //    {
        //        Console.WriteLine("You selected too much soldiers for the split");
        //    }
        //    else
        //    {

        //        Console.WriteLine($"You selected {SoldierNumberForSplit} soldiers for the split");
        //        List<Movements> movements = armyService.TryMove(army, x, y);


        //        string movs = "";
        //        foreach (var movement in movements)
        //        {
        //            string m = movement.ToString();
        //            movs += m.Substring(0, 1);
        //            movs += "/";
        //        }
        //        movs = movs.Substring(0, movs.Length - 1);
        //        Console.WriteLine($"Territories availables for the split are : {movs}");

        //        Console.Write($"Choose your movement:");

        //        bool redo = true;
        //        do
        //        {
        //            string Chosenmov = "";
        //            Chosenmov = Console.ReadLine();
        //            redo = true;
        //            switch (Chosenmov)
        //            {
        //                case "N":
        //                case "n":

        //                    if (movements.Contains(Movements.North))
        //                    {
        //                        armyService.SplitMove(army, x, y, SoldierNumberForSplit, Movements.North, player);
        //                        Console.WriteLine($"Army splits of {SoldierNumberForSplit} towards North done");
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Movement not possible");
        //                        redo = true;
        //                    }
        //                    break;
        //                case "S":
        //                case "s":

        //                    if (movements.Contains(Movements.South))
        //                    {
        //                        armyService.SplitMove(army, x, y, SoldierNumberForSplit, Movements.South, player);
        //                        Console.WriteLine($"Army splits of {SoldierNumberForSplit} towards South done");

        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Movement not possible");
        //                        redo = true;
        //                    }
        //                    break;

        //                case "E":
        //                case "e":

        //                    if (movements.Contains(Movements.East))
        //                    {
        //                        armyService.SplitMove(army, x, y, SoldierNumberForSplit, Movements.East, player);
        //                        Console.WriteLine($"Army splits of {SoldierNumberForSplit} towards East done");

        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Movement not possible");
        //                        redo = true;
        //                    }

        //                    break;
        //                case "W":
        //                case "w":

        //                    if (movements.Contains(Movements.West))
        //                    {
        //                        armyService.SplitMove(army, x, y, SoldierNumberForSplit, Movements.West, player);
        //                        Console.WriteLine($"Army splits of {SoldierNumberForSplit} towards West done");

        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Movement not possible");
        //                        redo = true;
        //                    }
        //                    break;
        //                default:
        //                    Console.WriteLine("You entered a wrong direction");
        //                    redo = true;
        //                    break;
        //            }
        //        } while (!redo);
        //    }

        //}

        //private static bool SplitPossible(HugoLandContext context, MilitaryDetachment army)
        //{
        //    if (army.MilitaryForce < 11)
        //    {
        //        Console.WriteLine("You don't have enough soldiers to split");
        //        return false;
        //    }
        //    else
        //        return true;

        //}


        //private static void Fusion(HugoLandContext context, IEnumerable<MilitaryDetachment> armies)
        //{
        //    ArmyService armyService = new ArmyService(context);


        //}
    }
}
