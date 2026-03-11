using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class MilitaryDetachment
    {
        private int _energy = 0;
        public Guid Id { get; protected set; }

        public int Energy
        {
            get => _energy;
            set
            {
                _energy = value > 5 ? 5 : value;
            }
        }

        public int MilitaryForce { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public Guid TerritoryId { get; set; }
        public virtual Territory Territory { get; set; }
        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected MilitaryDetachment() { }

        public static MilitaryDetachment Create(int energy, int militaryForce, Guid playerId, Guid TerritoryId, Guid gameId)
        {
            return new MilitaryDetachment
            {
                Energy = energy,
                Id = Guid.NewGuid(),
                MilitaryForce = militaryForce,
                TerritoryId = TerritoryId,
                PlayerId = playerId,
                GameId = gameId
            };
        }
    }
}
