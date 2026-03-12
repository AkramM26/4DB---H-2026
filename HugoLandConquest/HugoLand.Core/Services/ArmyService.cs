using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Donne le context du jeu à la classe
        /// </summary>
        private readonly HugoLandContext Context = context;


        /// <summary>
        /// Donne la liste des mouvements possibles 
        /// </summary>
        /// <returns></returns>
        public List<Movements> TryMove(Domain.MilitaryDetachment army, int x, int y)
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

            }
            return mpossibles;
        }








        /// <summary>
        /// Effectue le mouvement 
        /// </summary>
        /// <returns></returns>
        public async void Move(MilitaryDetachment army, int x, int y, Movements mov)
        {
            if (army.MilitaryForce < 10)
            {
                army.Energy = 5;
                Console.WriteLine("Too few soldiers to move");
                return;
            }


            var ATerritory = Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            switch (mov)
            {
                case Movements.North:

                    var NTerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == x && t.PositionY == (y - 1));

                    if (NTerritory?.MilitaryDetachment == null)
                    {

                        NTerritory.PositionX = x;
                        NTerritory.PositionY = y - 1;
                    }
                    else
                    {
                        if (NTerritory?.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            Fusion(NTerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.South:

                    var STerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == x && t.PositionY == (y + 1));

                    if (STerritory?.MilitaryDetachment == null)
                    {
                        STerritory.PositionX = x;
                        STerritory.PositionY = y + 1;
                    }
                    else
                    {
                        if (STerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            Fusion(STerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.East:

                    var ETerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == (x + 1) && t.PositionY == y);

                    if (ETerritory?.MilitaryDetachment == null)
                    {
                        ETerritory.PositionX = x + 1;
                        ETerritory.PositionY = y;
                    }
                    else
                    {
                        if (ETerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            Fusion(ETerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.West:

                    var WTerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == (x - 1) && t.PositionY == y);

                    if (WTerritory?.MilitaryDetachment == null)
                    {
                        WTerritory.PositionX = x - 1;
                        WTerritory.PositionY = y;
                    }
                    else
                    {
                        if (WTerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            Fusion(WTerritory, ATerritory);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;
            }
            army.Energy = army.Energy - 1;
            await Context.SaveChangesAsync();


        }


        /// <summary>
        /// Vérifie si la scission est possible
        /// </summary>
        /// <returns></returns>
        //public List<Movements> TrySplit(MilitaryDetachment army, int x, int y)
        //{
        //    int playernumber = army.Player.PlayerNumber;

        //    List<Movements> mpossibles = new List<Movements>();

        //    //North movement 
        //    int Nx = x;
        //    int Ny = y - 1;


        //    var ATerritory = Context.Territories
        //        .Include(t => t.MilitaryDetachment)
        //        .FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

        //    if (Ny >= 0)
        //    {
        //        var NTerritory = Context.Territories
        //            .Include(t => t.MilitaryDetachment)
        //            .FirstOrDefault(t => t.PositionX == Nx && t.PositionY == Ny);
        //    }



        //    //South movement 
        //    int Sx = x;
        //    int Sy = y + 1;

        //    if (Sy < 10)
        //    {
        //        var STerritory = Context.Territories
        //            .Include(t => t.MilitaryDetachment)
        //            .FirstOrDefault(t => t.PositionX == Sx && t.PositionY == Sy);
        //    }

        //    //East movement 
        //    int Ex = x + 1;
        //    int Ey = y;

        //    if (Ex < 15)
        //    {
        //        var ETerritory = Context.Territories
        //            .Include(t => t.MilitaryDetachment)
        //            .FirstOrDefault(t => t.PositionX == Ex && t.PositionY == Ey);
        //    }

        //    //West movement 
        //    int Wx = x - 1;
        //    int Wy = y;

        //    if (Wx >= 0)
        //    {
        //        var WTerritory = Context.Territories
        //            .Include(t => t.MilitaryDetachment)
        //            .FirstOrDefault(t => t.PositionX == Wx && t.PositionY == Wy);
        //    }
        //    return mpossibles;
        //}



        /// <summary>
        /// Effectue la scission
        /// </summary>
        /// <returns></returns>
        //public async void Split(MilitaryDetachment army, int x, int y, int soldierNumberForSplit, Movements mov)
        //{
        //    army.MilitaryForce = army.MilitaryForce - soldierNumberForSplit;

        //    MilitaryDetachment NewArmy = MilitaryDetachment.Create(army.Energy, soldierNumberForSplit, army.PlayerId, army.TerritoryId, army.GameId);

        //    switch (mov)
        //    {
        //        case Movements.North:
        //            NewArmy.Territory.PositionX = x;
        //            NewArmy.Territory.PositionY = y - 1;
        //            break;
        //        case Movements.South:
        //            NewArmy.Territory.PositionX = x;
        //            NewArmy.Territory.PositionY = y + 1;
        //            break;
        //        case Movements.East:
        //            NewArmy.Territory.PositionX = x + 1;
        //            NewArmy.Territory.PositionY = y;
        //            break;
        //        case Movements.West:
        //            NewArmy.Territory.PositionX = x - 1;
        //            NewArmy.Territory.PositionY = y;
        //            break;
        //    }
        //    army.Energy = army.Energy - 1;


        //    await Context.SaveChangesAsync();

        //}





        //public Territory SplitFusion(Territory nTerritory, Territory? aTerritory)
        //{
        //    Territory FusionResult = Territory.Create(TerritoryType.Plain, nTerritory.PositionX, nTerritory.PositionY, nTerritory.GameId);

        //    //New Force
        //    FusionResult.MilitaryDetachment.MilitaryForce = nTerritory.MilitaryDetachment.MilitaryForce + aTerritory.MilitaryDetachment.MilitaryForce;


        //    //New Enery
        //    if (nTerritory.MilitaryDetachment.Energy > aTerritory.MilitaryDetachment.Energy)
        //        FusionResult.MilitaryDetachment.Energy = aTerritory.MilitaryDetachment.Energy;
        //    else
        //        FusionResult.MilitaryDetachment.Energy = nTerritory.MilitaryDetachment.Energy;


        //    //New Territory after fusion 
        //    return FusionResult;
        //}

        public async void SplitMove(MilitaryDetachment army, int x, int y, int soldierNumberForSplit, Movements mov, Player player)
        {

            army.MilitaryForce = army.MilitaryForce - soldierNumberForSplit;

            var ATerritory = Context.Territories
                .Include(t => t.MilitaryDetachment)
                .FirstOrDefault(t => t.PositionX == x && t.PositionY == y);

            switch (mov)
            {
                case Movements.North:

                    var NTerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == x && t.PositionY == (y - 1));

                    if (NTerritory?.MilitaryDetachment == null)
                    {
                        MilitaryDetachment NewArmy = MilitaryDetachment.Create(army.Energy, soldierNumberForSplit, army.PlayerId, army.TerritoryId, army.GameId);

                        NewArmy.Territory = Territory.Create(TerritoryType.Plain, x, y - 1, army.GameId);

                        //if (soldierNumberForSplit < 10)
                        //{
                        //    if (player.Gold >= 20)
                        //    {
                        //        player.Gold -= 20;
                        //        NewArmy.Territory.Installation.InstallationType = InstallationType.Camp;
                        //    }
                        //}


                    }
                    else
                    {
                        if (NTerritory?.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            SplitFusion(NTerritory, ATerritory, soldierNumberForSplit);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.South:

                    var STerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == x && t.PositionY == (y + 1));

                    if (STerritory?.MilitaryDetachment == null)
                    {
                        MilitaryDetachment NewArmy = MilitaryDetachment.Create(army.Energy, soldierNumberForSplit, army.PlayerId, army.TerritoryId, army.GameId);
                        NewArmy.Territory = Territory.Create(TerritoryType.Plain, x, y + 1, army.GameId);

                        if (soldierNumberForSplit < 10)
                        {
                            NewArmy.Territory.Installation.InstallationType = InstallationType.Camp;
                        }

                    }
                    else
                    {
                        if (STerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            SplitFusion(STerritory, ATerritory, soldierNumberForSplit);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.East:

                    var ETerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == (x + 1) && t.PositionY == y);

                    if (ETerritory?.MilitaryDetachment == null)
                    {
                        MilitaryDetachment NewArmy = MilitaryDetachment.Create(army.Energy, soldierNumberForSplit, army.PlayerId, army.TerritoryId, army.GameId);
                        NewArmy.Territory = Territory.Create(TerritoryType.Plain, x + 1, y, army.GameId);

                        if (soldierNumberForSplit < 10)
                        {
                            NewArmy.Territory.Installation.InstallationType = InstallationType.Camp;
                        }
                    }
                    else
                    {
                        if (ETerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            SplitFusion(ETerritory, ATerritory, soldierNumberForSplit);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;

                case Movements.West:

                    var WTerritory = Context.Territories
                        .Include(t => t.MilitaryDetachment)
                        .FirstOrDefault(t => t.PositionX == (x - 1) && t.PositionY == y);

                    if (WTerritory?.MilitaryDetachment == null)
                    {
                        MilitaryDetachment NewArmy = MilitaryDetachment.Create(army.Energy, soldierNumberForSplit, army.PlayerId, army.TerritoryId, army.GameId);
                        NewArmy.Territory = Territory.Create(TerritoryType.Plain, x - 1, y, army.GameId);

                        if (soldierNumberForSplit < 10)
                        {
                            NewArmy.Territory.Installation.InstallationType = InstallationType.Camp;
                        }
                    }
                    else
                    {
                        if (WTerritory.MilitaryDetachment.Player.PlayerNumber == army.Player.PlayerNumber)
                        {
                            SplitFusion(WTerritory, ATerritory, soldierNumberForSplit);
                        }
                        else
                        {
                            //Fight
                        }
                    }
                    break;
            }
            army.Energy = army.Energy - 1;
            await Context.SaveChangesAsync();



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
        public async void Fusion(Territory nTerritory, Territory? aTerritory)
        {
            //New Force
            nTerritory.MilitaryDetachment.MilitaryForce += aTerritory.MilitaryDetachment.MilitaryForce;


            //New Enery
            if (nTerritory.MilitaryDetachment.Energy > aTerritory.MilitaryDetachment.Energy)
                nTerritory.MilitaryDetachment.Energy = aTerritory.MilitaryDetachment.Energy;
            else
                nTerritory.MilitaryDetachment.Energy = nTerritory.MilitaryDetachment.Energy;


            await Context.SaveChangesAsync();

        }
        public async void SplitFusion(Territory nTerritory, Territory? aTerritory, int soldierNumberForSplit)
        {

            //New Force
            nTerritory.MilitaryDetachment.MilitaryForce += soldierNumberForSplit;


            //New Enery
            if (nTerritory.MilitaryDetachment.Energy > aTerritory.MilitaryDetachment.Energy)
                nTerritory.MilitaryDetachment.Energy = aTerritory.MilitaryDetachment.Energy;
            else
                nTerritory.MilitaryDetachment.Energy = nTerritory.MilitaryDetachment.Energy;

            //Old territory is destroyed
            Territory.Delete(aTerritory);

            await Context.SaveChangesAsync();

        }

        public void Reinforce(HugoLandContext context, MilitaryDetachment army, int soldierForReinfocement, Player player)
        {
            if (army.MilitaryForce < 10)
                Console.WriteLine("Reinforcement failed !!!Too few soldiers for reinforcement");
            else
            {
                int GoldCost = soldierForReinfocement * 2;

                if (GoldCost < player.Gold)
                {
                    //Player's new Gold amount
                    player.Gold -= GoldCost;

                    //Reinforcement 
                    army.MilitaryForce += soldierForReinfocement;
                    Console.WriteLine("!!!Successful reinforcement!!!");

                }
                else
                    Console.WriteLine("Reinforcement failed !!! Too few gold!!!");


            }
        }
    }
}
