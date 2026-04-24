using HugoLand.Core.Constants;
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
        Mountain,
        Ocean
    }

    public static class TerrainRules
    {
        public static int GetEnergyCost(TerritoryType territoryType)
            => territoryType switch
            {
                TerritoryType.Forest => 2,
                TerritoryType.Mountain => 3,
                _ => 1,
            };

        public static float GetDefenseMultiplier(TerritoryType territoryType)
            => territoryType switch
            {
                TerritoryType.Forest => CombatConstants.ForestMultiplayer,
                TerritoryType.Mountain => CombatConstants.MountainMultiplayer,
                _ => CombatConstants.PlainMultiplayer,
            };

        public static bool IsPassable(TerritoryType territoryType)
            => territoryType != TerritoryType.Ocean;

        public static bool IsBuildable(TerritoryType territoryType)
            => territoryType != TerritoryType.Ocean;
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

        internal static void Delete(Territory aTerritory)
        {
            aTerritory = null;
        }
    }
}
