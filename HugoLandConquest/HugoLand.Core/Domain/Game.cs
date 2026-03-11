using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class Game
    {
        public Guid Id { get;  set; } 
        public string SaveName { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsPlayer1Winner { get; set; }

        public bool IsFinished { get; set; }
        public int? WinnerPlayerNumber { get; set; }
        public DateTime? EndedAt { get; set; }

        public virtual ICollection<Player> Players { get; set; } = [];
        public virtual ICollection<Territory> Territories { get; set; } = [];
        public virtual ICollection<CombatEvent> CombatEvents { get; set; } = [];
        public virtual ICollection<PlayerAction> PlayerActions { get; set; } = [];
        public virtual ICollection<TurnSnapShot> TurnSnapShots { get; set; } = [];
        public virtual ICollection<Installation> Installations { get; set; } = [];
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];

        protected Game() { }

        public static Game Create()
        {
            return new Game
            {
                Id = Guid.NewGuid(),
                SaveName = DateTime.Now.ToString(),
                IsFinished = false,
                WinnerPlayerNumber = null,
                EndedAt = null
            };
        }

        public Game Clone()
        {
            Game gameClone = Game.Create();

            AssignNewIds(this, gameClone);

            return new Game()
            {
                Id = Guid.NewGuid(),
                SaveName = this.SaveName,

            };
        }

        private void AssignNewIds(Game game, Game gameClone)
        {
            foreach (var item in SaveName)
            {
                
            }
        }
    }
}
