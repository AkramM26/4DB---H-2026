using HugoLand.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{


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

        public Action AskAction()
        {

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
        /// Vérifie si le mouvement est possible
        /// </summary>
        /// <returns></returns>
        public bool TryMove()
        {
            return false;
        }





        /// <summary>
        /// Effectue le mouvement 
        /// </summary>
        /// <returns></returns>
        public ArmyService Move()
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
        public ArmyService Fusion()
        {
            return new ArmyService(Context);
        }



    }
}
