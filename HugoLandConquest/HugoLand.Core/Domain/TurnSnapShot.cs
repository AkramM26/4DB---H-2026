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

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected TurnSnapShot() { }

        public static TurnSnapShot Create(Guid id, Guid gameId)
        {
            return new TurnSnapShot
            {
                Id = Guid.NewGuid(),
                GameId = gameId
            };

        }
    }
}
