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
    public class AIService(HugoLandContext context, ArmyService armyService, InstallationService installationService)
    {
        private readonly HugoLandContext _context = context;
        private readonly ArmyService _armyService = armyService;
        private readonly InstallationService _installationService = installationService;

        public async Task PlayIA(Guid playerId)
        {
            var player = await _context.Players
                .Include(p => p.MilitaryDetachments)
                .ThenInclude(m => m.Territory)
                .ThenInclude (t=> t.Installation)
                .FirstAsync(a => a.Id == playerId);

            var army = player.MilitaryDetachments. Where(m=> m.MilitaryForce >= 10);

            foreach (var a in army)
            {
                if (a.Territory.Installation == null)
                {
                    await _installationService.BuildCampAsync(a.Id);
                    return;
                }
                else if (a.Territory.Installation.InstallationType == InstallationType.Camp)
                {
                    await _installationService.UpgradeCampToFortificationAsync(a.Id);
                    return;
                }
                if (a.MilitaryForce < 30)
                {
                    int renforcment = 30 - a.MilitaryForce;
                    await _armyService.Reinforce(a, renforcment, player);
                }
            }

            foreach (var a in army)
            {
                var territories = await _context.Territories
                    .Where(t => t.PositionX == a.Territory.PositionX - 1 || t.PositionX == a.Territory.PositionX + 1)
                    .Where(t => t.PositionY == a.Territory.PositionY - 1 || t.PositionY == a.Territory.PositionY + 1)
                    .ToListAsync();

                foreach (var t in territories)
                {
                    Movements movement;
                    if (t.PositionX < a.Territory.PositionX)
                        movement = Movements.West;
                    else if (t.PositionX > a.Territory.PositionX)
                        movement = Movements.East;
                    else if (t.PositionY < a.Territory.PositionY)
                        movement = Movements.South;
                    else
                        movement = Movements.North;


                    if (t.MilitaryDetachment == null && t.TerritoryType != TerritoryType.Ocean)
                    {
                        await _armyService.Split(a.Id, movement, 15);
                        return;
                    }
                }
            }

        }
    }
}
