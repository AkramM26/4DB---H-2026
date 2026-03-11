using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Domain
{
    public class PlayerAction
    {
        public Guid Id { get; protected set; }

        public PlayerActionType ActionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }

        public Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        protected PlayerAction() { }

        public static PlayerAction Create(Guid gameId, PlayerActionType actionType, string desc)
        {
            return new PlayerAction
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                ActionType = actionType,
                Description = desc,
                CreateAt = DateTime.UtcNow
            };
        }
    }
}
