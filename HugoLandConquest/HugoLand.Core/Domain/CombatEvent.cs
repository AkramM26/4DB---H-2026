using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class CombatEvent
    {
        public Guid Id { get; set; }
        public int VictorPlayerNumber { get; set; }
        public float DefenceRandomFactor { get; set; }
        public float AttackRandomFactor { get; set; }
        public int DefenceForce { get; set; }
        public int AttackForce { get; set; }
        public float EffectiveDefenceForce { get; set; }
        public float EffectiveAttackForce { get; set; }
        public int DefenceLoss { get; set; }
        public int AttackLoss { get; set; }
        public int GoldLooted { get; set; }
        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected CombatEvent() { }

        public static CombatEvent Create( Guid gameId, int victorPlayerNumber, float defenceRandomFactor, float attackRandomFactor,
            int defenceForce, int attackForce, float effectiveDefenceForce, float effectiveAttackForce,
            int defenceLoss, int attackLoss, int goldLooted)
        {
            return new CombatEvent
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                VictorPlayerNumber = victorPlayerNumber,
                DefenceRandomFactor = defenceRandomFactor,
                AttackRandomFactor = attackRandomFactor,
                DefenceForce = defenceForce,
                AttackForce = attackForce,
                EffectiveDefenceForce = effectiveDefenceForce,
                EffectiveAttackForce = effectiveAttackForce,
                DefenceLoss = defenceLoss,
                AttackLoss = attackLoss,
                GoldLooted = goldLooted
            };
        }
    }
}
