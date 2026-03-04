using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class CombatService(HugoLandContext context)
    {
        private HugoLandContext Context = context;
        public async Task<bool> ResolveCombat(MilitaryDetachment defence, MilitaryDetachment attack)
        {
            Random random = new Random();
            float installationMultiplayer = 1;
            float territoryMultiplayer = 1;
            int defenceForce = defence.MilitaryForce;
            int attackForce = attack.MilitaryForce;
            bool defenceVictory = false;
            TerritoryType territoryType = defence.Territory.TerritoryType;
            InstallationType installationType;

            if (defence.Territory.Installation is null)
                installationType = InstallationType.None;
            else
                installationType = defence.Territory.Installation.InstallationType;

            if (territoryType == TerritoryType.Forest)
                territoryMultiplayer = CombatConstants.ForestMultiplayer;
            if (territoryType == TerritoryType.Mountain)
                territoryMultiplayer = CombatConstants.MountainMultiplayer;

            if (installationType == InstallationType.Fortification)
                installationMultiplayer = CombatConstants.FortificationMultiplayer;
            if (installationType == InstallationType.Camp)
                installationMultiplayer = CombatConstants.CampMultiplayer;

            float effectiveForceDefence = (defenceForce * random.Next(CombatConstants.MinMultiplayerDefence,
                CombatConstants.MaxMultiplayerDefence) / 10f) * territoryMultiplayer * installationMultiplayer;
            float effectiveForceAttack = attackForce * random.Next(CombatConstants.MinMultiplayerAttack,
                CombatConstants.MaxMultiplayerAttack) / 10f;

            if (effectiveForceDefence >= effectiveForceAttack)
            {
                defenceForce = (int)(defenceForce * 0.8);
                attackForce = (int)(attackForce * 0.4);

                defenceVictory = true;
            }
            else
            {
                defenceForce = (int)(defenceForce * 0.4);
                attackForce = (int)(attackForce * 0.8);

                defenceVictory = true;
            }

            var defenceDetachment = await Context.MilitaryDetachments
                .FirstAsync(d => d.Id == defence.Id);
            var attackDetachment = await Context.MilitaryDetachments
                .FirstAsync(d => d.Id == attack.Id);

            defenceDetachment.MilitaryForce = defenceForce;
            attackDetachment.MilitaryForce = attackForce;

            await Context.SaveChangesAsync();
            return defenceVictory;
        }
    }
}
