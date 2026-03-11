using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class TurnSnapShot
    {
        public Guid Id { get; set; } 

        public int PlayerNumber { get; set; }
        public int Gold { get; set; }
        public int ArmyCount { get; set; }
        public int TotalMilitaryForce { get; set; }
        public int FortificationCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected TurnSnapShot() { }

        public static TurnSnapShot Create(Guid gameId, int playerNumber, int gold, int armyCount, int totalMilitaryForce, int fortificationCount)
        {
            return new TurnSnapShot
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                PlayerNumber = playerNumber,
                Gold = gold,
                ArmyCount = armyCount,
                TotalMilitaryForce = totalMilitaryForce,
                FortificationCount = fortificationCount,
                CreatedAt = DateTime.UtcNow
            };

        }
    }
}
