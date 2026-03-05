using HugoLand.Core.Data;
using HugoLand.Core.Domain;
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
