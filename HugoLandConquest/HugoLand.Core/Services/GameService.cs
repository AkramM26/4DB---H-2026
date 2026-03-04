using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class GameService
    {
        private readonly HugoLandContext _context;

        public GameService(HugoLandContext context)
        {
            _context = context;
        }

        public void CreateGame()
        {
            Game game = new();
            Player player1 = new(), player2 = new();
            game.Players.Add(player1);
            game.Players.Add(player2);

            foreach (var p in game.Players)
            {
                // Attribuer les éléments de départ à chaque joueur
            }
        }

        public void LoadGame()
        {
        }

        public void SaveGame()
        {
        }

        public void EndTurn()
        {

        }
    }
}
