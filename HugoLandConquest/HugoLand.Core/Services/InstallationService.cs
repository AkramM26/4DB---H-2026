using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace HugoLand.Core.Services
{
    public class InstallationService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;

        public async Task<bool> BuildCampAsync(Guid militaryDetachmentId)
        {
            var militaryDetachment = await Context.MilitaryDetachments
                .Include(m => m.Player)
                .Include(m => m.Territory)
                .ThenInclude(t => t.Installation)
                .FirstOrDefaultAsync(m => m.Id == militaryDetachmentId);

            if (militaryDetachment == null)
                return false;

            if (militaryDetachment.MilitaryForce < GameConstants.MinimumArmyForceForActions)
                return false;

            if (!militaryDetachment.CanAct)
                return false;

            if (militaryDetachment.Territory.Installation != null)
                return false;

            if (militaryDetachment.Player.Gold < GameConstants.CampConstructionCost)
                return false;

            var camp = Installation.Create(
                InstallationType.Camp,
                militaryDetachment.TerritoryId,
                militaryDetachment.GameId);

            militaryDetachment.Player.Gold -= GameConstants.CampConstructionCost;
            militaryDetachment.CanAct = false;
            militaryDetachment.Territory.Installation = camp;

            await Context.Installations.AddAsync(camp);
            await Context.AddAsync(PlayerAction.Create(militaryDetachment.GameId));
            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpgradeCampToFortificationAsync(Guid militaryDetachmentId)
        {
            var militaryDetachment = await Context.MilitaryDetachments
                .Include(m => m.Player)
                .Include(m => m.Territory)
                .ThenInclude(t => t.Installation)
                .FirstOrDefaultAsync(m => m.Id == militaryDetachmentId);

            if (militaryDetachment == null)
                return false;

            if (militaryDetachment.MilitaryForce < GameConstants.MinimumArmyForceForActions)
                return false;

            if (!militaryDetachment.CanAct)
                return false;

            if (militaryDetachment.Territory.Installation == null)
                return false;

            if (militaryDetachment.Territory.Installation.InstallationType != InstallationType.Camp)
                return false;

            if (militaryDetachment.Player.Gold < GameConstants.FortificationUpgradeCost)
                return false;

            militaryDetachment.Player.Gold -= GameConstants.FortificationUpgradeCost;
            militaryDetachment.CanAct = false;
            militaryDetachment.Territory.Installation.InstallationType = InstallationType.Fortification;

            await Context.AddAsync(PlayerAction.Create(militaryDetachment.GameId));
            await Context.SaveChangesAsync();

            return true;
        }
    }
}
