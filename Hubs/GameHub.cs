using Microsoft.AspNetCore.SignalR;
using NeonGrid.Services;
using System.Linq;

namespace NeonGrid.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameManager _gm;
        public GameHub(GameManager gm) { _gm = gm; }

        public async Task Login(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            _gm.AddPlayer(Context.ConnectionId, username);

            // Send leaderboard safely
            var lb = _gm.Stats.OrderByDescending(s => s.Wins).Take(10).ToList();
            await Clients.Caller.SendAsync("LoginSuccess", lb);
            await RefreshLobby();
        }

        public async Task CreateGame()
        {
            var game = _gm.CreateGame(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, game.GameId);
            await Clients.Caller.SendAsync("GameCreated", game.GameId);
            await RefreshLobby();
        }

        public async Task JoinGame(string gameId)
        {
            var game = _gm.JoinGame(gameId, Context.ConnectionId);
            if (game != null && game.PlayerX != null && game.PlayerO != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, game.GameId);
                await Clients.Client(game.PlayerX.ConnectionId).SendAsync("GameStarted", "X", game.PlayerO.Name);
                await Clients.Caller.SendAsync("GameStarted", "O", game.PlayerX.Name);
                await RefreshLobby();
            }
        }

        public async Task MakeMove(string gameId, int index)
        {
            var result = _gm.MakeMove(gameId, Context.ConnectionId, index);
            if (result.isValid)
            {
                string status = result.isWin ? "WIN" : (result.isDraw ? "DRAW" : "NEXT");
                await Clients.Group(gameId).SendAsync("MoveMade", index, status, Context.ConnectionId);

                if (result.isWin || result.isDraw)
                {
                    await Clients.All.SendAsync("UpdateLeaderboard", _gm.Stats.OrderByDescending(s => s.Wins).Take(10).ToList());
                    await RefreshLobby();
                }
            }
        }

        private async Task RefreshLobby()
        {
            // [FIX] Defensive null check on PlayerX
            var openGames = _gm.Games
                .Where(g => g.PlayerO == null && !g.IsGameOver && g.PlayerX != null)
                .Select(g => new { g.GameId, HostName = g.PlayerX!.Name })
                .ToList();

            await Clients.All.SendAsync("UpdateLobby", openGames);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var abandonedGames = _gm.RemovePlayer(Context.ConnectionId);
            foreach (var gid in abandonedGames)
            {
                await Clients.Group(gid).SendAsync("OpponentDisconnected");
            }
            await RefreshLobby();
            await base.OnDisconnectedAsync(exception);
        }
    }
}