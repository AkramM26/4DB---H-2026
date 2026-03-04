using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class TurnSnapShot
    {
        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
