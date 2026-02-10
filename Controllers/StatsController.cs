using Microsoft.AspNetCore.Mvc;
using NeonGrid.Services;

namespace NeonGrid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly GameManager _manager;

        public StatsController(GameManager manager)
        {
            _manager = manager;
        }

        // GET: api/stats/leaderboard
        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            var topPlayers = _manager.Stats
                .OrderByDescending(p => p.Wins)
                .Take(10);

            return Ok(topPlayers);
        }

        // GET: api/stats/server-info
        [HttpGet("server-info")]
        public IActionResult GetServerInfo()
        {
            return Ok(new
            {
                ActiveGames = _manager.Games.Count(g => !g.IsGameOver),
                ConnectedPlayers = _manager.Players.Count(),
                Status = "Online",
                Region = "Asia-SE" // Flavor text
            });
        }
    }
}