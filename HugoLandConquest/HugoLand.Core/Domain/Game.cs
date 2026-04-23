using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class Game
    {
        private string? _gameDescription = string.Empty;
        private string _gameName = string.Empty;

        public Guid Id { get;  set; } 
        public string SaveName { get; set; }
        public string GameName {
            get => _gameName;
            set
            {
                if (value.Length <= 50)
                    _gameName = value;
                else
                    _gameName = value.Substring(0, 50);
            }
        }
        public string? GameDescription
        {
            get => _gameDescription;
            set
            {
                if (value!.Length <= 500)
                    _gameDescription = value;
                else
                    _gameDescription = value.Substring(0, 500);
            }
        }
        public int TurnNumber { get; set; }
        public int PlayerTurn { get; set; }
        public bool IsFinished { get; set; }
        public bool IsTemplate { get; set; }
        public bool MilitaryVictory { get; set; }
        public int? WinnerPlayerNumber { get; set; }
        public DateTime? EndedAt { get; set; }
        public int GameSizeX { get; set; }
        public int GameSizeY { get; set; }

        public virtual ICollection<Player> Players { get; set; } = [];
        public virtual ICollection<Territory> Territories { get; set; } = [];
        public virtual ICollection<CombatEvent> CombatEvents { get; set; } = [];
        public virtual ICollection<PlayerAction> PlayerActions { get; set; } = [];
        public virtual ICollection<TurnSnapShot> TurnSnapShots { get; set; } = [];
        public virtual ICollection<Installation> Installations { get; set; } = [];
        public virtual ICollection<MilitaryDetachment> MilitaryDetachments { get; set; } = [];

        protected Game() { }

        public static Game Create(string gameName, int gameSizeX, int gameSizeY)
        {
            return new Game
            {
                Id = Guid.NewGuid(),
                SaveName = DateTime.Now.ToString(),
                GameSizeX = gameSizeX,
                GameSizeY = gameSizeY,
                IsFinished = false,
                WinnerPlayerNumber = null,
                EndedAt = null,
                TurnNumber = 1,
                PlayerTurn = 1,
                MilitaryVictory = false,
                GameName = gameName,
                IsTemplate = true
            };
        }
    }
}
