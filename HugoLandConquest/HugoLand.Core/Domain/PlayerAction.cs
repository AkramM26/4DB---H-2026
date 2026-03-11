using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{


    public class PlayerAction
    {
        public Guid Id { get; protected set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
        protected PlayerAction() { }

        public static PlayerAction Create(Guid gameId)
        {
            return new PlayerAction
            {
                Id = Guid.NewGuid(),
                GameId = gameId
               
            };
        }
    }
}
