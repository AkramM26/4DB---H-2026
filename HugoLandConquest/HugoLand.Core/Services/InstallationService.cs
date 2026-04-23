using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace HugoLand.Core.Services
{
    public class InstallationService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;

        public async Task<ResultService> BuildCampAsync(Guid militaryDetachmentId)
        {
            var militaryDetachment = await Context.MilitaryDetachments
                .Include(m => m.Player)
                .Include(m => m.Territory)
                .ThenInclude(t => t.Installation)
                .FirstOrDefaultAsync(m => m.Id == militaryDetachmentId);

            if (militaryDetachment == null)
                return ResultService.FailureResult("Military detachment not found.");

            if (militaryDetachment.MilitaryForce < GameConstants.MinimumArmyForceForActions)
                return ResultService.FailureResult("Army needs a minimum of 10 soldiers.");

            if (!militaryDetachment.CanAct)
                return ResultService.FailureResult("Army detachment cannot act this turn.");

            if (militaryDetachment.Territory.Installation != null)
                return ResultService.FailureResult("There is already an installation on this territory.");

            if (militaryDetachment.Player.Gold < GameConstants.CampConstructionCost)
                return ResultService.FailureResult("Not enough gold (20 needed)");

            var camp = Installation.Create(
                InstallationType.Camp,
                militaryDetachment.Territory.Id,
                militaryDetachment.Player.Id,
                militaryDetachment.Game.Id);

            militaryDetachment.Player.Gold -= GameConstants.CampConstructionCost;
            militaryDetachment.CanAct = false;
            militaryDetachment.CanMove = false;
            militaryDetachment.Territory.Installation = camp;

            await Context.Installations.AddAsync(camp);
            await Context.AddAsync(PlayerAction.Create(militaryDetachment.GameId, PlayerActionType.BuildCamp,
                $"Camp built at ({militaryDetachment.Territory.PositionX}, {militaryDetachment.Territory.PositionY})"));
            await Context.SaveChangesAsync();

            return ResultService.SuccessResult("Camp built successfully.");
        }

        public async Task<ResultService> UpgradeCampToFortificationAsync(Guid militaryDetachmentId)
        {
            var militaryDetachment = await Context.MilitaryDetachments
                .Include(m => m.Player)
                .Include(m => m.Territory)
                .ThenInclude(t => t.Installation)
                .FirstOrDefaultAsync(m => m.Id == militaryDetachmentId);

            if (militaryDetachment == null)
                return ResultService.FailureResult("Army not found.");

            if (militaryDetachment.MilitaryForce < GameConstants.MinimumArmyForceForActions)
                return ResultService.FailureResult("Army needs a minimum of 10 soldiers.");

            if (!militaryDetachment.CanAct)
                return ResultService.FailureResult("Army cannot act this turn.");

            if (militaryDetachment.Territory.Installation == null)
                return ResultService.FailureResult("There is no installation on this territory.");

            if (militaryDetachment.Territory.Installation.InstallationType != InstallationType.Camp)
                return ResultService.FailureResult("Only camps can be upgraded to fortifications.");

            if (militaryDetachment.Territory.Installation.PlayerId != militaryDetachment.PlayerId)
                return ResultService.FailureResult("You can only upgrade your own camps.");

            if (militaryDetachment.Player.Gold < GameConstants.FortificationUpgradeCost)
                return ResultService.FailureResult("Not enough gold (50 needed)");

            militaryDetachment.Player.Gold -= GameConstants.FortificationUpgradeCost;
            militaryDetachment.CanAct = false;
            militaryDetachment.CanMove = false;
            militaryDetachment.Territory.Installation.InstallationType = InstallationType.Fortification;

            await Context.AddAsync(PlayerAction.Create(militaryDetachment.GameId, PlayerActionType.UpgradeCampToFortification,
                $"Camp upgraded to fortification at ({militaryDetachment.Territory.PositionX}, {militaryDetachment.Territory.PositionY})"));
            await Context.SaveChangesAsync();

            return ResultService.SuccessResult("Camp upgraded to fortification successfully.");
        }
    }
}
