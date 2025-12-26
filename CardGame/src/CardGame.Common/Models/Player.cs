using System;

namespace CardGame.Common.Models
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int Rating { get; set; } = 1000;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int Silver { get; set; } = 1000;
        public int Gold { get; set; } = 50;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        // Настройки арены
        public string DefenseFormationJson { get; set; } = "{}";

    }
}