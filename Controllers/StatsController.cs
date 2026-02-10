using Microsoft.AspNetCore.Mvc;
using NeonGrid.Services;
using System.Linq; // Added

namespace NeonGrid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly GameManager _manager;
        public StatsController(GameManager manager) { _manager = manager; }

        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard() => Ok(_manager.Stats.OrderByDescending(p => p.Wins).Take(10));

        [HttpGet("server-info")]
        public IActionResult GetServerInfo() => Ok(new
        {
            ActiveGames = _manager.Games.Count(g => !g.IsGameOver),
            ConnectedPlayers = _manager.Players.Count(),
            Status = "Online"
        });
    }
}