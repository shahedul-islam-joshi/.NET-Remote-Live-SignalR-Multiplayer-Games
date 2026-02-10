using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq; // CRITICAL: Added for .Any(), .All(), .Where()
using NeonGrid.Models;

namespace NeonGrid.Services
{
    public class GameManager
    {
        private readonly ConcurrentDictionary<string, GameSession> _games = new();
        private readonly ConcurrentDictionary<string, Player> _players = new();
        private ConcurrentDictionary<string, UserStat> _stats = new();

        private readonly string _dbPath = "player_stats.json";
        private readonly object _fileLock = new();

        public GameManager() { LoadStats(); }

        public IEnumerable<GameSession> Games => _games.Values;
        public IEnumerable<Player> Players => _players.Values;
        public IEnumerable<UserStat> Stats => _stats.Values;

        public void AddPlayer(string connectionId, string name)
        {
            var player = new Player { ConnectionId = connectionId, Name = name };
            _players[connectionId] = player;

            if (!_stats.ContainsKey(name))
            {
                _stats[name] = new UserStat { Name = name };
                SaveStats();
            }
        }

        public List<string> RemovePlayer(string connectionId)
        {
            _players.TryRemove(connectionId, out _);

            // End any active games this player was in
            var affectedGames = _games.Values
                .Where(g => !g.IsGameOver && (g.PlayerX?.ConnectionId == connectionId || g.PlayerO?.ConnectionId == connectionId))
                .ToList();

            foreach (var game in affectedGames)
            {
                game.IsGameOver = true;
            }

            return affectedGames.Select(g => g.GameId).ToList();
        }

        public Player? GetPlayer(string connectionId) => _players.GetValueOrDefault(connectionId);

        public GameSession CreateGame(string hostConnectionId)
        {
            var host = GetPlayer(hostConnectionId) ?? throw new Exception("Player not found");
            host.Symbol = "X";
            var game = new GameSession { PlayerX = host, CurrentTurnConnectionId = hostConnectionId };
            _games[game.GameId] = game;
            return game;
        }

        public GameSession? JoinGame(string gameId, string joinerConnectionId)
        {
            if (_games.TryGetValue(gameId, out var game) && game.PlayerO == null)
            {
                var joiner = GetPlayer(joinerConnectionId);
                if (joiner == null) return null;
                joiner.Symbol = "O";
                game.PlayerO = joiner;
                return game;
            }
            return null;
        }

        public (bool isValid, bool isWin, bool isDraw) MakeMove(string gameId, string connectionId, int index)
        {
            if (!_games.TryGetValue(gameId, out var game)) return (false, false, false);

            if (game.IsGameOver || game.CurrentTurnConnectionId != connectionId || !string.IsNullOrEmpty(game.Board[index]))
                return (false, false, false);

            var player = game.PlayerX!.ConnectionId == connectionId ? game.PlayerX : game.PlayerO;
            game.Board[index] = player!.Symbol;

            bool win = CheckWin(game.Board);
            bool draw = !win && game.Board.All(s => !string.IsNullOrEmpty(s));

            if (win)
            {
                game.IsGameOver = true;
                UpdateStats(game.PlayerX.Name, game.PlayerO!.Name, player.Symbol);
            }
            else if (draw)
            {
                game.IsGameOver = true;
                UpdateStats(game.PlayerX.Name, game.PlayerO!.Name, "DRAW");
            }
            else
            {
                game.CurrentTurnConnectionId = connectionId == game.PlayerX.ConnectionId ? game.PlayerO!.ConnectionId : game.PlayerX.ConnectionId;
            }

            return (true, win, draw);
        }

        private bool CheckWin(string[] b)
        {
            int[][] lines = { new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, new[] { 0, 4, 8 }, new[] { 2, 4, 6 } };
            return lines.Any(l => !string.IsNullOrEmpty(b[l[0]]) && b[l[0]] == b[l[1]] && b[l[1]] == b[l[2]]);
        }

        private void UpdateStats(string pX, string pO, string result)
        {
            // Ensure keys exist (Safety Check)
            if (!_stats.ContainsKey(pX)) _stats[pX] = new UserStat { Name = pX };
            if (!_stats.ContainsKey(pO)) _stats[pO] = new UserStat { Name = pO };

            if (result == "X") { _stats[pX].Wins++; _stats[pO].Losses++; }
            else if (result == "O") { _stats[pO].Wins++; _stats[pX].Losses++; }
            else { _stats[pX].Draws++; _stats[pO].Draws++; }
            SaveStats();
        }

        private void SaveStats() { lock (_fileLock) { File.WriteAllText(_dbPath, JsonSerializer.Serialize(_stats, new JsonSerializerOptions { WriteIndented = true })); } }
        private void LoadStats() { lock (_fileLock) { if (File.Exists(_dbPath)) { try { _stats = JsonSerializer.Deserialize<ConcurrentDictionary<string, UserStat>>(File.ReadAllText(_dbPath)) ?? new(); } catch { _stats = new(); } } } }
    }
}