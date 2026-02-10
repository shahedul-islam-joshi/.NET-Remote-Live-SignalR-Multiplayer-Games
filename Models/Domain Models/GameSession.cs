namespace NeonGrid.Models
{
    public class GameSession
    {
        public string GameId { get; set; } = Guid.NewGuid().ToString("N");
        // FIX: Initialize the array immediately!
        public string[] Board { get; set; } = new string[9];
        public Player? PlayerX { get; set; }
        public Player? PlayerO { get; set; }
        public string? CurrentTurnConnectionId { get; set; }
        public bool IsGameOver { get; set; } = false;
    }

    public class Player
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }

    public class UserStat
    {
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}