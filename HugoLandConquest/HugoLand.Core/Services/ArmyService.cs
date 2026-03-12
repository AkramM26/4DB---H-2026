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
        public record MoveResult(bool move, bool Fight, bool Fusion, bool DefenceVicory);

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

                if (NTerritory.MilitaryDetachment == null || NTerritory.MilitaryDetachment.PlayerId == army.PlayerId)
                    mpossibles.Add(NTerritory);
            }



            //South movement 
            int Sx = x;
            int Sy = y + 1;

            if (Sy < 10)
            {
                var STerritory = territories
                    .FirstOrDefault(t => t.PositionX == Sx && t.PositionY == Sy);


                if (STerritory?.MilitaryDetachment == null || STerritory.MilitaryDetachment.PlayerId == army.PlayerId)
                    mpossibles.Add(STerritory);

            }

            //East movement 
            int Ex = x + 1;
            int Ey = y;

            if (Ex < 15)
            {
                var ETerritory = territories
                    .FirstOrDefault(t => t.PositionX == Ex && t.PositionY == Ey);

                if (ETerritory?.MilitaryDetachment == null || ETerritory.MilitaryDetachment.PlayerId == army.PlayerId)
                    mpossibles.Add(ETerritory);

            }

            //West movement 
            int Wx = x - 1;
            int Wy = y;

            if (Wx >= 0)
            {
                var WTerritory = territories
                    .FirstOrDefault(t => t.PositionX == Wx && t.PositionY == Wy);

                if (WTerritory?.MilitaryDetachment == null || WTerritory.MilitaryDetachment.PlayerId == army.PlayerId)
                    mpossibles.Add(WTerritory);

            }
            return mpossibles;
        }



        public async Task<MoveResult> Move(Guid militaryDetachementId, Movements movement)
        {
            var militaryDetachement = await Context.MilitaryDetachments
                .Include(m => m.Territory)
                .FirstAsync(m => m.Id == militaryDetachementId);
            bool defenceVictory = false;
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
            if (newX >= 15 || newX < 0 || newY >= 10 || newY < 0 || !militaryDetachement.CanMove)
                return new MoveResult(move, fight, fusion, defenceVictory);

            var territory = await Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstAsync(t => t.PositionX == newX && t.PositionY == newY);
            MilitaryDetachment otherMilitaryDetachment = territory.MilitaryDetachment;

            if (otherMilitaryDetachment == null)
            {
                militaryDetachement.TerritoryId = territory.Id;
                militaryDetachement.Energy--;
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
                defenceVictory = await CombatService.ResolveCombatAsync(otherMilitaryDetachment.Id, militaryDetachement.Id);
                if (!defenceVictory)
                {
                    militaryDetachement.TerritoryId = territory.Id;
                    List<Territory> listTerritory = await TryMove(otherMilitaryDetachment, territory.PositionX, territory.PositionY);
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
            await Context.SaveChangesAsync();
            return new MoveResult(move, fight, fusion, defenceVictory);
        }

        public async Task<MoveResult> Split(Guid militaryDetachementId, Movements movement, int splitNumber)
        {
            var militaryDetachement = await Context.MilitaryDetachments
                .Include(m => m.Territory)
                .FirstAsync(m => m.Id == militaryDetachementId);
            int remainingArmy = militaryDetachement.MilitaryForce - splitNumber;
            Territory territory = militaryDetachement.Territory;

            if (splitNumber < 10)
                return new MoveResult(false, false, false, false);
            if (remainingArmy <= 0)
                return new MoveResult(false, false, false, false);

            MoveResult moveResult = await Move(militaryDetachementId, movement);

            if (!moveResult.move && !moveResult.Fight)
                return moveResult;
            else if (moveResult.Fight && moveResult.DefenceVicory)
                militaryDetachement.MilitaryForce = remainingArmy;
            else
                await Context.AddAsync(MilitaryDetachment.Create(militaryDetachement.Energy, remainingArmy,
                    militaryDetachement.PlayerId, militaryDetachement.TerritoryId, militaryDetachement.GameId));

            militaryDetachement.CanMove = false;
            await Context.SaveChangesAsync();
            return moveResult;
        }

        private void Fusion(MilitaryDetachment movingArmy, MilitaryDetachment stationaryArmy)
        {
            movingArmy.Energy--;
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
