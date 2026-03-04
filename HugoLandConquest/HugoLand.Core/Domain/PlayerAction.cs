using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class PlayerAction
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
