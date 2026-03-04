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

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Installation> Installations { get; set; } = [];
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];
    }
}
