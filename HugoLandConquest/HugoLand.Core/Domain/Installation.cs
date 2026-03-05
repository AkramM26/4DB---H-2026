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
        public Guid Id { get; protected set; }

        public InstallationType InstallationType { get; set; }

        public Guid TerritoryId { get; set; }
        public virtual Territory Territory { get; set; }

        protected Installation() { }

        public static Installation Create(InstallationType installationType, Guid territoryId)
        {
            return new Installation
            {
                Id = Guid.NewGuid(),
                InstallationType = installationType,
                TerritoryId = territoryId
            };
        }
    }
}
