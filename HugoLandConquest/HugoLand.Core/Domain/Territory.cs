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
        public Guid Id { get; set; }
        public TerritoryType TerritoryType{ get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }
        public virtual MilitaryDetachment? MilitaryDetachment { get; set; }
        public virtual Installation? Installation { get; set; }

        protected Territory() { }

        public static Territory Create(TerritoryType territoryType,int positionX,int positionY, Guid GameId)
        {
            return new Territory
            {
                Id = Guid.NewGuid(),
                TerritoryType = territoryType,
                GameId = GameId,
                PositionY = positionY,
                PositionX = positionX,
            };
        }
    }
}
