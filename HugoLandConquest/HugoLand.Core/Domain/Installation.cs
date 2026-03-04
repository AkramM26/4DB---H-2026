using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class Installation
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
