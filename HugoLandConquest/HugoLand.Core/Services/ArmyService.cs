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

        /// <summary>
        /// Donne le context du jeu à la classe
        /// </summary>
        private readonly HugoLandContext Context = context;


        /// <summary>
        /// Donne la liste des mouvements possibles 
        /// </summary>
        /// <returns></returns>
        public List<Movements> TryMove(Domain.MilitaryDetachment army, int x, int y, Player player)
        {
            List<Movements> mpossibles = new List<Movements>();

            //North movement 
            int Nx = x;
            int Ny = y - 1;


            var ATerritory = Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            if (Ny >= 0)
            {
                var NTerritory = Context.Territories
                    .Include(t => t.MilitaryDetachment)
                    .FirstOrDefault(t => t.PositionX == Nx && t.PositionY == Ny);


                if (NTerritory?.MilitaryDetachment == null)
                    mpossibles.Add(Movements.North);
                else
                {
                    if (NTerritory?.MilitaryDetachment.Player.PlayerNumber == player.PlayerNumber)
                    {
                        Fusion(NTerritory, ATerritory);
                    }
                    else
                    {
                        //Fight
                    }
                }
            }



            //South movement 
            int Sx = x;
            int Sy = y + 1;

            if (Sy < 10)
            {
                var STerritory = Context.Territories
                    .Include(t => t.MilitaryDetachment)
                    .FirstOrDefault(t => t.PositionX == Sx && t.PositionY == Sy);


                if (STerritory?.MilitaryDetachment == null)
                    mpossibles.Add(Movements.South);
                else
                {
                    if (STerritory?.MilitaryDetachment.Player.PlayerNumber == player.PlayerNumber)
                    {
                        Fusion(STerritory, ATerritory);
                    }
                    else
                    {
                        //Fight
                    }
                }

                //East movement 
                int Ex = x + 1;
                int Ey = y;

                if (Ex < 15)
                {
                    var ETerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == Ex && t.PositionY == Ey);

                    if (ETerritory?.MilitaryDetachment == null)
                        mpossibles.Add(Movements.East);
                    else
                    {
                        if (ETerritory?.MilitaryDetachment.Player.PlayerNumber == player.PlayerNumber)
                        {
                            Fusion(ETerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                }

                //West movement 
                int Wx = x - 1;
                int Wy = y;

                if (Wx >= 0)
                {
                    var WTerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == Wx && t.PositionY == Wy);

                    if (WTerritory?.MilitaryDetachment == null)
                        mpossibles.Add(Movements.West);
                    else
                    {
                        if (WTerritory?.MilitaryDetachment.Player.PlayerNumber == player.PlayerNumber)
                        {
                            Fusion(WTerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                }
            }


            return mpossibles;

        }




        /// <summary>
        /// Effectue le mouvement 
        /// </summary>
        /// <returns></returns>
        public void Move(Domain.MilitaryDetachment army, int x, int y, Movements mov)
        {
            switch (mov)
            {
                case Movements.North:
                    army.Territory.PositionX = x;
                    army.Territory.PositionY = y - 1;
                    break;
                case Movements.South:
                    army.Territory.PositionX = x;
                    army.Territory.PositionY = y + 1;
                    break;
                case Movements.East:
                    army.Territory.PositionX = x + 1;
                    army.Territory.PositionY = y;
                    break;
                case Movements.West:
                    army.Territory.PositionX = x - 1;
                    army.Territory.PositionY = y;
                    break;
            }
        }


        /// <summary>
        /// Vérifie si la scission est possible
        /// </summary>
        /// <returns></returns>
        public bool TrySplit()
        {
            return false;
        }




        /// <summary>
        /// Effectue la scission
        /// </summary>
        /// <returns></returns>
        public ArmyService Split()
        {
            return new ArmyService(Context);

        }





        /// <summary>
        /// Vérifie si la fusion est possible 
        /// </summary>
        /// <returns></returns>
        public bool TryFusion()
        {
            return false;
        }



        /// <summary>
        /// Effectue la fusion 
        /// </summary>
        /// <returns></returns>
        public Territory Fusion(Territory nTerritory, Territory? aTerritory)
        {
            Territory FusionResult = Territory.Create(TerritoryType.Plain, nTerritory.PositionX, nTerritory.PositionY, nTerritory.GameId);

            //New Force
            FusionResult.MilitaryDetachment.MilitaryForce = nTerritory.MilitaryDetachment.MilitaryForce + aTerritory.MilitaryDetachment.MilitaryForce;


            //New Enery
            if (nTerritory.MilitaryDetachment.Energy > aTerritory.MilitaryDetachment.Energy)
                FusionResult.MilitaryDetachment.Energy = aTerritory.MilitaryDetachment.Energy;
            else
                FusionResult.MilitaryDetachment.Energy = nTerritory.MilitaryDetachment.Energy;


            //New Territory after fusion 
            return FusionResult;
        }



    }
}
