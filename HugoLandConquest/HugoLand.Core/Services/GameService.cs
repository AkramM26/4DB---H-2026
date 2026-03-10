using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class GameService (HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;

        public async Task CreateGameAsync()
        {
            await Seed.SeedGameAsync(Context);
            // Créer la partie en se servant des objets créer par SeedGameAsync
            var game = Context.Games.FirstOrDefaultAsync();
            var player1 = Context.Players.FirstOrDefaultAsync(p => p.PlayerNumber == 1);
            var player2 = Context.Players.FirstOrDefaultAsync(p => p.PlayerNumber == 2);
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
