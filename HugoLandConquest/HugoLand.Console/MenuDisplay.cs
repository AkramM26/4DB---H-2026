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
                Console.Write("  Choix :");
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
                Console.Write("  Choix :");
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
                Console.WriteLine("====================");
                Console.WriteLine("  [1] Select an army");
                Console.WriteLine("  [2] Save Game");
                Console.WriteLine("  [3] Go to main menu");
                Console.Write("  Choix :");
                string input = Console.ReadLine();

                validInput = int.TryParse(input, out choice);

                if (choice > 3 || choice < 1)
                    validInput = false;
                if (!validInput)
                    GameDisplay.ShowGame(context);
            }
            return choice;
        }
        public static MilitaryDetachment ShowArmyChoice(ICollection<MilitaryDetachment> militaryDetachments, Game game)
        {
            bool validInputX = false;
            bool validInputY = false;
            int posX = 0;
            int posY = 0;

            while (true)
            {
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
                if (militaryDetachment == null || militaryDetachment.Player.PlayerNumber != game.PlayerTurn)
                    validInputX = false;

                if (validInputX && validInputY)
                    return militaryDetachment;
            }
        }
    }
}
