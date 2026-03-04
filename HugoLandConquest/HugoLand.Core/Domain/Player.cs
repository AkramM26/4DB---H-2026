using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class Player
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public int Gold { get; set; }
        public int TurnInDept { get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];
    }
}
