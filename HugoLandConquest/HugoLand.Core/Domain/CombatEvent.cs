using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class CombatEvent
    {
        public Guid Id { get; protected set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected CombatEvent() { }

        public static CombatEvent Create( Guid gameId)
        {
            return new CombatEvent
            {
                Id = Guid.NewGuid(),
                GameId = gameId
            };
        }
    }
}
