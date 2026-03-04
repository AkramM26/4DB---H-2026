using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public enum InstallationType
    {
        None,
        Camp,
        Fortification
    }
    public class Installation
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public InstallationType InstallationType { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public Guid TerritoryId { get; set; }
        public virtual Territory Territory { get; set; }
    }
}
