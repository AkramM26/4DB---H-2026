using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{


    public class Player
    {
        public Guid Id { get; set; }
        public int PlayerNumber { get;protected set; }
        public int Gold { get; set; }
        public int Income { get; set; }
        public int TurnInDept { get; set; }
        public Guid GameId { get;  set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];
        public virtual ICollection<Installation> Installations { get; set; } = [];

        protected Player() { }

        public static Player Create(int gold, Guid GameId, int playerNumber)
        {
            return new Player
            {
                Id = Guid.NewGuid(),
                Gold = gold,
                TurnInDept = 0,
                GameId = GameId,
                PlayerNumber = playerNumber,
                Income = 0
            };
        }
    }
}
