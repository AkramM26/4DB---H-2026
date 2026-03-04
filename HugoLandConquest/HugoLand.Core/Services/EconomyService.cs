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
    public class EconomyService(HugoLandContext context)
    {
        private readonly HugoLandContext Context;
        public async Task CollectRevenue(Guid playerId)
        {
            var player = await Context.Players
                .FirstAsync(p => p.Id == playerId);
            var income = await Context.Installations
                .Where(i => i.InstallationType == InstallationType.Fortification)
                .Where(i => i.Territory.MilitaryDetachment != null)
                .Where(i => i.Territory.MilitaryDetachment!.PlayerId == playerId)
                .CountAsync() * 5;

            player.Gold += income;
            await Context.SaveChangesAsync();
        }
        public async Task PayMaintenance(Guid playerId)
        {
            var player = await Context.Players
                .FirstAsync(p => p.Id == playerId);
            var cost = await Context.MilitaryDetachments
                .Where(m => m.PlayerId == playerId && m.MilitaryForce >= 10)
                .SumAsync(m => (int)Math.Ceiling(m.MilitaryForce / 10m));

            player.Gold -= cost;

            if (player.Gold < 0)
                player.TurnInDept++;
            else if (player.Gold >= 0)
                player.TurnInDept = 0;

            await Context.SaveChangesAsync();
        }
    }
}
