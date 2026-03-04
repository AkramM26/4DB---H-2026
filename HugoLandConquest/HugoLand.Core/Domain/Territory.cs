using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public enum TerritoryType
    {
        Plain,
        Forest,
        Mountain
    }
    public class Territory
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public TerritoryType TerritoryType{ get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
        public Guid? MilitaryDetachmentId { get; set; }
        public virtual MilitaryDetachment? MilitaryDetachment { get; set; }
        public Guid? InstallationId { get; set; }
        public virtual Installation? Installation { get; set; }
    }
}
