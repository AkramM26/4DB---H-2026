using HugoLand.Core.Domain;
using static HugoLand.Core.Services.ArmyService;

namespace HugoLand.Core
{
    public interface IGameView
    {
        // ── Menu principal ────────────────────────────────────────────────────
        /// <summary>Affiche le menu principal. Retourne : 1=Nouvelle partie, 2=Charger, 3=Quitter.</summary>
        Task<int> ShowMainMenuAsync();

        /// <summary>Demande le nom et la description pour une nouvelle partie.</summary>
        Task<(string name, string description)> ShowNewGameInputAsync();

        /// <summary>Affiche la liste des sauvegardes. Retourne l'index (0-based) ou -1 pour retourner.</summary>
        Task<int> ShowLoadingMenuAsync(List<Game> games);

        // ── Affichage en jeu ──────────────────────────────────────────────────
        /// <summary>
        /// Rafraîchit l'affichage de l'état courant du jeu (grille, or, tour...).
        /// En WPF avec data binding, cette méthode peut être vide (no-op).
        /// </summary>
        void RefreshDisplay();

        // ── Actions du joueur ─────────────────────────────────────────────────
        /// <summary>Menu d'actions principal. Retourne : 1=Sélectionner armée, 2=Sauvegarder, 3=Menu principal, 4=Fin de tour.</summary>
        Task<int> ShowActionChoiceAsync();

        /// <summary>Le joueur sélectionne une de ses armées. Retourne l'armée choisie.</summary>
        Task<MilitaryDetachment> ShowArmyChoiceAsync(ICollection<MilitaryDetachment> militaryDetachments, Game game);

        /// <summary>Menu d'actions pour une armée. Retourne : 1=Déplacer, 2=Camp, 3=Fortification, 4=Renforcer, 5=Scinder, 6=Passer.</summary>
        Task<int> ShowArmyActionAsync(MilitaryDetachment militaryDetachment);

        /// <summary>Menu de déplacement. Retourne : 'N','S','E','W' pour une direction, 'X' pour annuler.</summary>
        Task<char> ShowMoveMenuAsync(List<Territory> territories, MilitaryDetachment militaryDetachment);

        /// <summary>Affiche le résultat d'un déplacement/combat et attend l'accusé du joueur.</summary>
        Task ShowMoveResultAsync(MoveResult moveResult);

        /// <summary>Demande le nombre de soldats à acheter pour renforcer l'armée.</summary>
        Task<int> ShowReinforceNumberAsync(MilitaryDetachment militaryDetachment);

        /// <summary>Demande le nombre de soldats à détacher pour la scission (0 = annuler).</summary>
        Task<int> ShowSplitNumberAsync(MilitaryDetachment militaryDetachment);

        /// <summary>Affiche l'écran de victoire et attend l'accusé du joueur.</summary>
        Task ShowVictoryScreenAsync(Game game);
    }
}
