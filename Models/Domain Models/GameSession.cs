namespace NeonGrid.Models
{
    public class GameSession
    {
        public string GameId { get; set; } = Guid.NewGuid().ToString();
        public Player? PlayerX { get; set; }
        public Player? PlayerO { get; set; }
        public string CurrentTurnConnectionId { get; set; } = string.Empty;
        public string[] Board { get; set; } = new string[9]; // Index 0-8
        public bool IsGameOver { get; set; }
        public string? WinnerId { get; set; } // Null if draw or ongoing
    }
}