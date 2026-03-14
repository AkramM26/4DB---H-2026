using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand
{
    public class MenuDisplay
    {
        public static int ShowMainMenu()
        {
            bool validInput = false;
            int choice = 0;
            while (!validInput)
            {
                Console.Clear();
                Console.WriteLine("══════════════════════════════════════");
                Console.WriteLine("      HugoLand: Conquest — Phase 1");
                Console.WriteLine("══════════════════════════════════════\n");
                Console.WriteLine("  [1] New Game");
                Console.WriteLine("  [2] Load Game");
                Console.WriteLine("  [3] Quit\n");
                Console.Write("  Choice :");
                string input = Console.ReadLine();
                validInput = int.TryParse(input, out choice);
                if (choice > 3 || choice < 1)
                    validInput = false;
            }
            return choice;
        }
        public static int ShowLoadingMenu(List<Game> games)
        {
            bool validInput = false;
            int choice = 0;
            while (!validInput)
            {
                Console.Clear();
                Console.WriteLine("--- Game Save ---");

                for (int i = 1; i <= games.Count; i++)
                {
                    Console.WriteLine($"  [{i}] " + games[i - 1].SaveName);
                }
                Console.WriteLine($"  [0] Quit\n");
                Console.Write("  Choice :");
                string input = Console.ReadLine();
                validInput = int.TryParse(input, out choice);
                choice--;
                if (choice > games.Count || choice < -1)
                    validInput = false;
            }
            return choice;
        }
        public static int ShowActionChoice(HugoLandContext context)
        {
            bool validInput = false;
            int choice = 0;
            while (!validInput)
            {
                GameDisplay.ShowGame(context);

                Console.WriteLine("====================");
                Console.WriteLine("  [1] Select an army");
                Console.WriteLine("  [2] Save Game");
                Console.WriteLine("  [3] Go to main menu");
                Console.WriteLine("  [4] Skip turn");
                Console.Write("  Choice :");
                string input = Console.ReadLine();

                validInput = int.TryParse(input, out choice);

                if (choice > 4 || choice < 1)
                    validInput = false;
                //if (!validInput)
                //    GameDisplay.ShowGame(context);
            }
            return choice;
        }
        public static MilitaryDetachment ShowArmyChoice(ICollection<MilitaryDetachment> militaryDetachments, Game game, HugoLandContext context)
        {
            bool validInputX = false;
            bool validInputY = false;
            int posX = 0;
            int posY = 0;

            while (true)
            {
                GameDisplay.ShowGame(context);

                Console.WriteLine("====================");
                Console.WriteLine("Insert the position of the military detachment you want to use");
                Console.Write("Position in x: ");
                string inputX = Console.ReadLine();
                Console.Write("Position in y: ");
                string inputY = Console.ReadLine();

                validInputX = int.TryParse(inputX, out posX);
                validInputY = int.TryParse(inputY, out posY);
                if (posX > 14 || posX < 0)
                    validInputX = false;
                if (posY > 9 || posY < 0)
                    validInputY = false;

                MilitaryDetachment militaryDetachment = militaryDetachments.FirstOrDefault(m => m.Territory.PositionX == posX && m.Territory.PositionY == posY);
                if (militaryDetachment == null || militaryDetachment.Player.PlayerNumber != game.PlayerTurn || militaryDetachment.MilitaryForce < 10)
                    validInputX = false;

                if (validInputX && validInputY)
                    return militaryDetachment;
            }
        }
        public static int ShowArmyAction(MilitaryDetachment militaryDetachment, HugoLandContext context)
        {
            bool validInput = false;
            int choice = 0;

            while (!validInput)
            {
                GameDisplay.ShowGame(context);

                bool installationPresent = militaryDetachment.Territory.Installation != null;
                bool forticicationPresent = false;
                if (installationPresent)
                    forticicationPresent = militaryDetachment.Territory.Installation!.InstallationType == InstallationType.Fortification;

                Console.WriteLine($"\n  --- Army in ({militaryDetachment.Territory.PositionX},{militaryDetachment.Territory.PositionY}) " +
                    $"- {militaryDetachment.MilitaryForce} soldier, {militaryDetachment.Energy} energy ---");

                string installation = "";
                if (militaryDetachment.Territory.Installation == null)
                    installation = "None";
                else
                    installation = militaryDetachment.Territory.Installation.InstallationType.ToString();

                Console.WriteLine($"    Installation: {installation}\n");
                Console.WriteLine($"    Available actions :");
                if (militaryDetachment.Energy == 0)
                    Console.WriteLine($"       [1] Move                                     - Impossible : The army have 0 energy");
                else if (!militaryDetachment.CanMove)
                    Console.WriteLine($"       [1] Move                                     - Impossible : The army can't move");
                else
                    Console.WriteLine($"       [1] Move");

                if (!militaryDetachment.CanAct)
                    Console.WriteLine($"       [2] Build a camp                 (20 gold)   - Impossible : The army can't act");
                else if (installationPresent)
                    Console.WriteLine($"       [2] Build a camp                 (20 gold)   - Impossible : installation already present");
                else
                    Console.WriteLine($"       [2] Build a camp                 (20 gold)");
                if (forticicationPresent)
                    Console.WriteLine($"       [3] Convert into fortification   (50 gold)   - Impossible : fortification already present");
                else if (!militaryDetachment.CanAct)
                    Console.WriteLine($"       [3] Convert into fortification   (20 gold)   - Impossible : The army can't act");
                else
                    Console.WriteLine($"       [3] Convert into fortification   (50 gold)");
                if (installationPresent)
                    Console.WriteLine($"       [4] Strenghten the army          (2 gold/soldier)");
                else
                    Console.WriteLine($"       [4] Strenghten the army          (2 gold/soldier)   - Impossible : There is no installation");
                if (militaryDetachment.MilitaryForce < 11)
                    Console.WriteLine($"       [5] Split the army   - Impossible : The army is not big enough (11 or more)");
                else
                    Console.WriteLine($"       [5] Split the army");

                Console.WriteLine($"       [6] Pass (do nothing)");

                Console.Write("\n    Choice :");

                string input = Console.ReadLine();
                validInput = int.TryParse(input, out choice);

                if (choice > 6 || choice < 1)
                    validInput = false;
                else if ((choice == 1 && militaryDetachment.Energy == 0) || (choice == 1 && !militaryDetachment.CanMove))
                    validInput = false;
                else if ((choice == 2 && installationPresent) || (choice == 2 && !militaryDetachment.CanAct))
                    validInput = false;
                else if ((choice == 3 && forticicationPresent) || (choice == 2 && !militaryDetachment.CanAct))
                    validInput = false;
                else if (choice == 4 && !installationPresent)
                    validInput = false;
                else if (choice == 5 && militaryDetachment.MilitaryForce < 11)
                    validInput = false;
            }
            return choice;
        }
        public static char ShowMoveMenu(ICollection<Territory> territories, MilitaryDetachment militaryDetachment, HugoLandContext context)
        {
            bool validInput = false;
            char choice = ' ';
            int posX = militaryDetachment.Territory.PositionX;
            int posY = militaryDetachment.Territory.PositionY;

            Territory nTerritory = territories.FirstOrDefault(t => t.PositionY == posY - 1 && t.PositionX == posX)!;
            Territory sTerritory = territories.FirstOrDefault(t => t.PositionY == posY + 1 && t.PositionX == posX)!;
            Territory eTerritory = territories.FirstOrDefault(t => t.PositionY == posY && t.PositionX == posX + 1)!;
            Territory wTerritory = territories.FirstOrDefault(t => t.PositionY == posY && t.PositionX == posX - 1)!;

            bool enemyNorth = (nTerritory != null) && (nTerritory.MilitaryDetachment != null) && (nTerritory.MilitaryDetachment.PlayerId != militaryDetachment.PlayerId);
            bool enemySouth = (sTerritory != null) && (sTerritory.MilitaryDetachment != null) && (sTerritory.MilitaryDetachment.PlayerId != militaryDetachment.PlayerId);
            bool enemyEst = (eTerritory != null) && (eTerritory.MilitaryDetachment != null) && (eTerritory.MilitaryDetachment.PlayerId != militaryDetachment.PlayerId);
            bool enemyWest = (wTerritory != null) && (wTerritory.MilitaryDetachment != null) && (wTerritory.MilitaryDetachment.PlayerId != militaryDetachment.PlayerId);
            bool allyNorth = (nTerritory != null) && (nTerritory.MilitaryDetachment != null) && (nTerritory.MilitaryDetachment.PlayerId == militaryDetachment.PlayerId);
            bool allySouth = (sTerritory != null) && (sTerritory.MilitaryDetachment != null) && (sTerritory.MilitaryDetachment.PlayerId == militaryDetachment.PlayerId);
            bool allyEast = (eTerritory != null) && (eTerritory.MilitaryDetachment != null) && (eTerritory.MilitaryDetachment.PlayerId == militaryDetachment.PlayerId);
            bool allyWest = (wTerritory != null) && (wTerritory.MilitaryDetachment != null) && (wTerritory.MilitaryDetachment.PlayerId == militaryDetachment.PlayerId);

            string nArmy = "Empty";
            string sArmy = "Empty";
            string eArmy = "Empty";
            string wArmy = "Empty";
            if (nTerritory != null && enemyNorth)
                nArmy = $"Enemy : military force({nTerritory.MilitaryDetachment!.MilitaryForce})";
            else if (nTerritory != null && allyNorth)
                nArmy = $"ally : military force({nTerritory.MilitaryDetachment!.MilitaryForce})";
            if (sTerritory != null && enemySouth)
                sArmy = $"Enemy : military force({sTerritory.MilitaryDetachment!.MilitaryForce})";
            else if (sTerritory != null && allySouth)
                sArmy = $"ally : military force({sTerritory.MilitaryDetachment!.MilitaryForce})";
            if (eTerritory != null && enemyEst)
                eArmy = $"Enemy : military force({eTerritory.MilitaryDetachment!.MilitaryForce})";
            else if (eTerritory != null && allyEast)
                eArmy = $"ally : military force({eTerritory.MilitaryDetachment!.MilitaryForce})";
            if (wTerritory != null && enemyWest)
                wArmy = $"Enemy : military force({wTerritory.MilitaryDetachment!.MilitaryForce})";
            else if (wTerritory != null && allyWest)
                wArmy = $"ally : military force({wTerritory.MilitaryDetachment!.MilitaryForce})";

            while (!validInput)
            {
                GameDisplay.ShowGame(context);

                Console.WriteLine($"\n  --- Army at ({posX},{posY}) - Energy : {militaryDetachment.Energy} ---\n");
                Console.WriteLine("    Adjacent territories :");
                if (nTerritory != null)
                {
                    Console.Write($"      [N] ({nTerritory.PositionX},{nTerritory.PositionY}) - {nArmy} " +
                        $"         Cost : 1 Energy      ");
                    if (enemyNorth)
                        Console.Write("  Fight\n");
                    else if (allyNorth)
                        Console.Write("  Fusion\n");
                    else
                        Console.Write("  Move\n");
                }
                if (sTerritory != null)
                {
                    Console.Write($"      [S] ({sTerritory.PositionX},{sTerritory.PositionY}) - {sArmy} " +
                        $"         Cost : 1 Energy      ");
                    if (enemySouth)
                        Console.Write("  Fight\n");
                    else if (allySouth)
                        Console.Write("  Fusion\n");
                    else
                        Console.Write("  Move\n");
                }
                if (eTerritory != null)
                {
                    Console.Write($"      [E] ({eTerritory.PositionX},{eTerritory.PositionY}) - {eArmy} " +
                        $"         Cost : 1 Energy      ");
                    if (enemyEst)
                        Console.Write("  Fight\n");
                    else if (allyEast)
                        Console.Write("  Fusion\n");
                    else
                        Console.Write("  Move\n");
                }
                if (wTerritory != null)
                {
                    Console.Write($"      [W] ({wTerritory.PositionX},{wTerritory.PositionY}) - {wArmy} " +
                        $"         Cost : 1 Energy      ");
                    if (enemyWest)
                        Console.Write("  Fight\n");
                    else if (allyWest)
                        Console.Write("  Fusion\n");
                    else
                        Console.Write("  Move\n");
                }
                Console.WriteLine("      [X] Cancel");
                Console.Write("  Choice :");
                string input = Console.ReadLine();

                validInput = char.TryParse(input, out choice);
                choice = Char.ToUpper(choice);

                if (choice != 'N' && choice != 'S' && choice != 'E' && choice != 'W' && choice != 'X')
                    validInput = false;
                else if (choice == 'N' && nTerritory == null)
                    validInput = false;
                else if (choice == 'S' && sTerritory == null)
                    validInput = false;
                else if (choice == 'W' && wTerritory == null)
                    validInput = false;
                else if (choice == 'E' && eTerritory == null)
                    validInput = false;
            }
            return choice;
        }

        public static int ShowSplitNumber(MilitaryDetachment militaryDetachment, HugoLandContext context)
        {
            bool validInput = false;
            int splitNumber = -1;

            while (!validInput)
            {
                GameDisplay.ShowGame(context);

                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine("Enter the number of soldiers to split (minimum 10, or 0 to cancel): ");
                string input = Console.ReadLine();

                validInput = int.TryParse(input, out splitNumber);

                if (splitNumber < 10 && splitNumber != 0)
                    validInput = false;
            }
            return splitNumber;
        }
        public static int ShowReinforceNumber(MilitaryDetachment militaryDetachment, HugoLandContext context)
        {
            bool validInput = false;
            int reinforceNumber = -1;

            while (!validInput)
            {
                GameDisplay.ShowGame(context);

                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine("Enter the number of soldiers to buy: ");
                string input = Console.ReadLine();

                validInput = int.TryParse(input, out reinforceNumber);

                if (reinforceNumber < 0)
                    validInput = false;
            }
            return reinforceNumber;
        }
        public static void ShowVictoryScreen(Game game, HugoLandContext context)
        {
            char player;
            if (game.WinnerPlayerNumber == 1)
                player = 'A';
            else
                player = 'B';
            GameDisplay.ShowGame(context);
            Console.WriteLine($"=====================");
            Console.WriteLine($"The player {player} has won the game.");
            Console.WriteLine($"=====================");
        }
    }
}
