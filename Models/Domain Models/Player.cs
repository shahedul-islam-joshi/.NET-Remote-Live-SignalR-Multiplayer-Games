namespace NeonGrid.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = "Unknown";
        public string Symbol { get; set; } = string.Empty; // "X" or "O"
    }
}