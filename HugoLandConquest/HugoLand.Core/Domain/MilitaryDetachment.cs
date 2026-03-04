using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class MilitaryDetachment
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public int Energy { get; set; }

        public int MilitaryForce { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public Guid TerritoryId { get; set; }
        public virtual Territory Territory { get; set; }
    }
}
