using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class EconomyService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        public async Task CollectRevenue(Player player)
        {
            var income = await Context.Installations
                .Where(i => i.InstallationType == InstallationType.Fortification)
                .Where(i => i.Territory.MilitaryDetachment != null)
                .Where(i => i.Territory.MilitaryDetachment!.PlayerId == player.Id)
                .CountAsync() * 5;

            player.Gold += income;
            player.Income = income;
        }
        public async Task PayMaintenance(Player player)
        {
            double cost = await Context.MilitaryDetachments
                .Where(m => m.PlayerId == player.Id && m.MilitaryForce >= 10)
                .SumAsync(m => (m.MilitaryForce / 10d));
            cost = Math.Ceiling(cost);

            player.Gold -= (int)cost;
            player.Income -= (int)cost;

            if (player.Gold < 0)
                player.TurnInDept++;
            else if (player.Gold >= 0)
                player.TurnInDept = 0;
        }
    }
}
