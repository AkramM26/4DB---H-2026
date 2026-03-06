using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class CombatService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private record CombatResult(bool DefenceVictory, int DefenceForce, int AttackForce,
            int GoldGain, float DefenceRandomFactor, float AttackRandomFactor,
            float EffectiveDefenceForce, float EffectiveAttackForce);

        public async Task<bool> ResolveCombatAsync(Guid defenceId, Guid attackId)
        {
            var defence = await Context.MilitaryDetachments
                .Include(d => d.Territory.Installation)
                .Include(d => d.Player)
                .FirstAsync(d => d.Id == defenceId);
            var attack = await Context.MilitaryDetachments
                .Include(a => a.Territory.Installation)
                .Include(d => d.Player)
                .FirstAsync(a => a.Id == attackId);

            int defenceForce = defence.MilitaryForce;
            int attackForce = attack.MilitaryForce;
            TerritoryType territoryType = defence.Territory.TerritoryType;
            InstallationType installationType;
            if (defence.Territory.Installation is null)
                installationType = InstallationType.None;
            else
                installationType = defence.Territory.Installation.InstallationType;

            CombatResult combatResult = InternalResolveCombat(defenceForce, attackForce, territoryType, installationType);

            defence.MilitaryForce = combatResult.DefenceForce;
            attack.MilitaryForce = combatResult.AttackForce;
            defence.Energy = 0;
            attack.Energy = 0;

            int victorPlayerNumber;
            if (combatResult.DefenceVictory)
            {
                victorPlayerNumber = defence.Player.PlayerNumber;
                defence.Player.Gold += combatResult.GoldGain;
                if (combatResult.AttackForce < 10)
                    Context.Remove(attack);
            }
            else
            {
                victorPlayerNumber = attack.Player.PlayerNumber;
                attack.Player.Gold += combatResult.GoldGain;
                if (combatResult.DefenceForce < 10)
                    Context.Remove(defence);
            }

            CombatEvent combatEvent = CombatEvent.Create(defence.Player.GameId, victorPlayerNumber,
                combatResult.DefenceRandomFactor, combatResult.AttackRandomFactor, defenceForce, attackForce,
                combatResult.EffectiveDefenceForce, combatResult.EffectiveAttackForce,
                defenceForce - combatResult.DefenceForce,attackForce-combatResult.AttackForce,combatResult.GoldGain);
                
            Context.Add(combatEvent);

            await Context.SaveChangesAsync();

            return combatResult.DefenceVictory;
        }
        private CombatResult InternalResolveCombat(int defenceForce, int attackForce, TerritoryType territoryType,
            InstallationType installationType)
        {
            Random random = new Random();
            float installationMultiplayer = 1;
            float territoryMultiplayer = 1;
            float attackRandomFactor = attackForce * random.Next(CombatConstants.MinMultiplayerAttack,
                CombatConstants.MaxMultiplayerAttack) / 10f;
            float defenceRandomFactor= defenceForce * random.Next(CombatConstants.MinMultiplayerDefence,
                CombatConstants.MaxMultiplayerDefence) / 10f;


            if (territoryType == TerritoryType.Forest)
                territoryMultiplayer = CombatConstants.ForestMultiplayer;
            if (territoryType == TerritoryType.Mountain)
                territoryMultiplayer = CombatConstants.MountainMultiplayer;

            if (installationType == InstallationType.Fortification)
                installationMultiplayer = CombatConstants.FortificationMultiplayer;
            if (installationType == InstallationType.Camp)
                installationMultiplayer = CombatConstants.CampMultiplayer;

            float effectiveForceDefence = defenceForce * defenceRandomFactor * territoryMultiplayer * installationMultiplayer;
            float effectiveForceAttack =attackForce * attackRandomFactor;

            int loserForceBeforeCombat;
            int loserForceloss;
            bool defenceVictory;
            if (effectiveForceDefence >= effectiveForceAttack)
            {
                loserForceBeforeCombat = attackForce;
                defenceForce = (int)(defenceForce * 0.8);
                attackForce = (int)(attackForce * 0.4);
                defenceVictory = true;
                loserForceloss = loserForceBeforeCombat - attackForce;
            }
            else
            {
                loserForceBeforeCombat = defenceForce;
                defenceForce = (int)(defenceForce * 0.4);
                attackForce = (int)(attackForce * 0.8);
                defenceVictory = false;
                loserForceloss = loserForceBeforeCombat - defenceForce;
            }

            CombatResult combatResult = new CombatResult(defenceVictory, defenceForce, attackForce, (loserForceloss / 10) * 5,
                defenceRandomFactor,attackRandomFactor, effectiveForceDefence, effectiveForceAttack);
            return combatResult;
        }
    }
}
