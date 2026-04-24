using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static HugoLand.Core.Services.CombatService;

namespace HugoLand.Core.Services
{
    public enum Movements
    {
        North,
        South,
        East,
        West
    }


    /// <summary>
    /// Auteur: Samuel KPLE-FAGET
    /// Description: Classe chargée de gérer les mouvements, scissions et fusions des armées d'un joueur 
    /// Date: 2026-03-10
    /// </summary>
    /// <param name="context"></param>
    public class ArmyService(HugoLandContext context)
    {
        private readonly HugoLandContext Context = context;
        private CombatService CombatService = new CombatService(context);
        private Random Rnd = new Random();
        public record MoveResult(bool move, bool Fight, bool Fusion, CombatResult? CombatResult);

        public static int GetTerrainEnergyCost(TerritoryType territoryType)
            => territoryType switch
            {
                TerritoryType.Forest => 2,
                TerritoryType.Mountain => 3,
                _ => 1,
            };

        /// <summary>
        /// Donne la liste des mouvements possibles 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Territory>> TryMove(Domain.MilitaryDetachment army, int x, int y)
        {

            List<Territory> mpossibles = new List<Territory>();

            var territories = await Context.Territories
                .Include(t => t.MilitaryDetachment)
                .ToListAsync();

            //North movement 
            int Nx = x;
            int Ny = y - 1;

            if (Ny >= 0)
            {
                var NTerritory = territories.First(t => t.PositionX == Nx && t.PositionY == Ny);
                if (TerrainRules.IsPassable(NTerritory.TerritoryType))
                    mpossibles.Add(NTerritory);
            }
            //South movement
            int Sx = x;
            int Sy = y + 1;

            if (Sy < GameConstants.gameSizeY)
            {
                var STerritory = territories
                    .FirstOrDefault(t => t.PositionX == Sx && t.PositionY == Sy);
                if (STerritory != null && TerrainRules.IsPassable(STerritory.TerritoryType))
                    mpossibles.Add(STerritory);
            }

            //East movement
            int Ex = x + 1;
            int Ey = y;

            if (Ex < GameConstants.gameSizeX)
            {
                var ETerritory = territories
                    .FirstOrDefault(t => t.PositionX == Ex && t.PositionY == Ey);
                if (ETerritory != null && TerrainRules.IsPassable(ETerritory.TerritoryType))
                    mpossibles.Add(ETerritory);
            }

            //West movement
            int Wx = x - 1;
            int Wy = y;

            if (Wx >= 0)
            {
                var WTerritory = territories
                    .FirstOrDefault(t => t.PositionX == Wx && t.PositionY == Wy);
                if (WTerritory != null && TerrainRules.IsPassable(WTerritory.TerritoryType))
                    mpossibles.Add(WTerritory);
            }
            return mpossibles;
        }



        public async Task<MoveResult> Move(Guid militaryDetachementId, Movements movement)
        {
            var militaryDetachement = await Context.MilitaryDetachments
                .Include(m => m.Territory)
                .FirstAsync(m => m.Id == militaryDetachementId);
            CombatResult combatResult = null;
            bool move = false;
            bool fight = false;
            bool fusion = false;
            int oldX = militaryDetachement.Territory.PositionX;
            int oldY = militaryDetachement.Territory.PositionY;
            int newX = oldX;
            int newY = oldY;


            switch (movement)
            {
                case Movements.North:
                    newY--;
                    break;
                case Movements.South:
                    newY++;
                    break;
                case Movements.East:
                    newX++;
                    break;
                case Movements.West:
                    newX--;
                    break;
                default:
                    break;
            }
            if (newX >= GameConstants.gameSizeX || newX < 0 || newY >= GameConstants.gameSizeY || newY < 0 || !militaryDetachement.CanMove)
                return new MoveResult(move, fight, fusion, combatResult);

            var territory = await Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstAsync(t => t.PositionX == newX && t.PositionY == newY);

            if (!TerrainRules.IsPassable(territory.TerritoryType))
                return new MoveResult(move, fight, fusion, combatResult);

            int energyCost = GetTerrainEnergyCost(territory.TerritoryType);
            if (militaryDetachement.Energy - energyCost < 0)
                return new MoveResult(move, fight, fusion, combatResult);

            militaryDetachement.Energy -= energyCost;

            MilitaryDetachment otherMilitaryDetachment = territory.MilitaryDetachment;

            if (otherMilitaryDetachment == null)
            {
                militaryDetachement.TerritoryId = territory.Id;
                move = true;
            }
            else if (otherMilitaryDetachment.PlayerId == militaryDetachement.PlayerId)
            {
                Fusion(militaryDetachement, otherMilitaryDetachment);
                fusion = true;
                move = true;
            }
            else if (otherMilitaryDetachment.PlayerId != militaryDetachement.PlayerId)
            {
                combatResult = await CombatService.ResolveCombatAsync(otherMilitaryDetachment.Id, militaryDetachement.Id);
                if (!combatResult.DefenceVictory)
                {
                    List<Territory> listTerritory = await TryMove(otherMilitaryDetachment, territory.PositionX, territory.PositionY);
                    militaryDetachement.TerritoryId = territory.Id;
                    if (listTerritory.Count == 0)
                        Context.Remove(otherMilitaryDetachment);
                    else
                    {
                        Territory newTerritory = listTerritory[Rnd.Next(listTerritory.Count)];
                        otherMilitaryDetachment.TerritoryId = newTerritory.Id;
                    }
                }
                else
                    move = false;
                fight = true;
            }
            militaryDetachement.CanAct = false;

            if (militaryDetachement.Territory.Installation != null && militaryDetachement.Territory.Installation.InstallationType == InstallationType.Camp)
                Context.Remove(militaryDetachement.Territory.Installation);

            await Context.SaveChangesAsync();
            return new MoveResult(move, fight, fusion, combatResult);
        }

        public async Task<MoveResult> Split(Guid militaryDetachementId, Movements movement, int splitNumber)
        {
            var militaryDetachement = await Context.MilitaryDetachments
                .Include(m => m.Territory)
                .FirstAsync(m => m.Id == militaryDetachementId);
            int remainingArmy = militaryDetachement.MilitaryForce - splitNumber;
            Territory territory = militaryDetachement.Territory;

            if (splitNumber < 10)
                return new MoveResult(false, false, false, null);
            if (remainingArmy <= 0)
                return new MoveResult(false, false, false, null);

            int originalForce = militaryDetachement.MilitaryForce;
            int splitEnergy = militaryDetachement.Energy;
            int destinationX = territory.PositionX;
            int destinationY = territory.PositionY;

            switch (movement)
            {
                case Movements.North:
                    destinationY--;
                    break;
                case Movements.South:
                    destinationY++;
                    break;
                case Movements.East:
                    destinationX++;
                    break;
                case Movements.West:
                    destinationX--;
                    break;
            }

            if (destinationX >= 0 && destinationX < GameConstants.gameSizeX && destinationY >= 0 && destinationY < GameConstants.gameSizeY)
            {
                var destinationTerritory = await Context.Territories
                    .FirstAsync(t => t.PositionX == destinationX && t.PositionY == destinationY);
                splitEnergy = Math.Max(0, militaryDetachement.Energy - GetTerrainEnergyCost(destinationTerritory.TerritoryType));
            }

            militaryDetachement.MilitaryForce = splitNumber;
            await Context.SaveChangesAsync();

            MoveResult moveResult = await Move(militaryDetachementId, movement);

            if (!moveResult.move && !moveResult.Fight)
            {
                militaryDetachement.MilitaryForce = originalForce;
                await Context.SaveChangesAsync();
                return moveResult;
            }
            else if (moveResult.Fight && moveResult.CombatResult!.DefenceVictory)
            {
                var survivingAttacker = await Context.MilitaryDetachments
                    .FirstOrDefaultAsync(m => m.Id == militaryDetachementId);

                if (survivingAttacker != null)
                {
                    survivingAttacker.MilitaryForce += remainingArmy;
                }
                else
                {
                    MilitaryDetachment stationaryMilitaryDetachment = MilitaryDetachment.Create(splitEnergy, remainingArmy,
                        militaryDetachement.PlayerId, territory.Id, militaryDetachement.GameId);
                    await Context.AddAsync(stationaryMilitaryDetachment);
                    stationaryMilitaryDetachment.CanMove = false;
                    stationaryMilitaryDetachment.CanAct = false;
                }
            }
            else
            {
                MilitaryDetachment stationaryMilitaryDetachment = MilitaryDetachment.Create(militaryDetachement.Energy, remainingArmy,
                    militaryDetachement.PlayerId, territory.Id, militaryDetachement.GameId);
                await Context.AddAsync(stationaryMilitaryDetachment);
                stationaryMilitaryDetachment.CanMove = false;
                stationaryMilitaryDetachment.CanAct = false;
            }

            await Context.SaveChangesAsync();
            return moveResult;
        }

        private void Fusion(MilitaryDetachment movingArmy, MilitaryDetachment stationaryArmy)
        {
            stationaryArmy.MilitaryForce += movingArmy.MilitaryForce;

            if (movingArmy.Energy < stationaryArmy.Energy)
                stationaryArmy.Energy = movingArmy.Energy;

            Context.Remove(movingArmy);
        }


        public async Task<ResultService> Reinforce(MilitaryDetachment army, int soldierForReinfocement, Player player)
        {
            if (army.MilitaryForce < 10)
                return ResultService.FailureResult("The militaryForce need to be 10 or more");
            if (army.Territory.Installation == null)
                return ResultService.FailureResult("Reinforcement is impossible if there is no Installation");
            else
            {
                int GoldCost = soldierForReinfocement * 2;

                if (GoldCost < player.Gold)
                {
                    player.Gold -= GoldCost;
                    army.MilitaryForce += soldierForReinfocement;

                    await Context.SaveChangesAsync();
                    return ResultService.SuccessResult("The reinforcement was successful");
                }
                else
                    return ResultService.FailureResult("There is not enough gold");
            }
        }
    }
}
