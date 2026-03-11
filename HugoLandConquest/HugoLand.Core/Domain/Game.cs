using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class Game
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string SaveName { get; set; }

        public virtual ICollection<Player> Players { get; set; } = [];
        public virtual ICollection<Territory> Territories { get; set; } = [];
        public virtual ICollection<CombatEvent> CombatEvents { get; set; } = [];
        public virtual ICollection<PlayerAction> PlayerActions { get; set; } = [];
        public virtual ICollection<TurnSnapShot> TurnSnapShots { get; set; } = [];
        public virtual ICollection<Installation> Installations { get; set; } = [];
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];

        protected Game() { }

        public static Game Create()
        {
            return new Game
            {
                SaveName = Constants.GameConstants.CurrentGame
            };
        }
    }
}
