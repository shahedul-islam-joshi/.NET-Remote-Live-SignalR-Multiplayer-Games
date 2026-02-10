using System.Collections.Concurrent;
using System.Text.Json;
using NeonGrid.Models;

namespace NeonGrid.Services
{
    public class GameManager
    {
        // Thread-safe collections
        private readonly ConcurrentDictionary<string, GameSession> _games = new();
        private readonly ConcurrentDictionary<string, Player> _players = new(); // Key: ConnectionId
        private ConcurrentDictionary<string, UserStat> _stats = new(); // Key: Username

        private readonly string _dbPath = "player_stats.json";
        private readonly object _fileLock = new();

        public GameManager()
        {
            LoadStats();
        }

        // --- Exposure for API ---
        public IEnumerable<GameSession> Games => _games.Values;
        public IEnumerable<Player> Players => _players.Values;
        public IEnumerable<UserStat> Stats => _stats.Values;

        // --- Player Logic ---
        public void AddPlayer(string connectionId, string name)
        {
            var player = new Player { ConnectionId = connectionId, Name = name };
            _players[connectionId] = player;

            // Initialize stats if new user
            if (!_stats.ContainsKey(name))
            {
                _stats[name] = new UserStat { Name = name };
                SaveStats();
            }
        }

        public void RemovePlayer(string connectionId)
        {
            _players.TryRemove(connectionId, out _);
            // Optional: Auto-forfeit games if a player disconnects (omitted for simplicity)
        }

        public Player? GetPlayer(string connectionId) =>
            _players.TryGetValue(connectionId, out var p) ? p : null;

        // --- Game Logic ---
        public GameSession CreateGame(string hostConnectionId)
        {
            var host = GetPlayer(hostConnectionId);
            if (host == null) throw new Exception("Player not found");

            host.Symbol = "X";
            var game = new GameSession
            {
                PlayerX = host,
                CurrentTurnConnectionId = hostConnectionId
            };

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

            // Validation
            if (game.IsGameOver ||
                game.CurrentTurnConnectionId != connectionId ||
                !string.IsNullOrEmpty(game.Board[index]))
            {
                return (false, false, false);
            }

            var player = game.PlayerX!.ConnectionId == connectionId ? game.PlayerX : game.PlayerO;
            game.Board[index] = player!.Symbol;

            bool win = CheckWin(game.Board);
            bool draw = !win && game.Board.All(s => !string.IsNullOrEmpty(s));

            if (win)
            {
                game.IsGameOver = true;
                game.WinnerId = connectionId;
                UpdateStats(game.PlayerX.Name, game.PlayerO!.Name, connectionId == game.PlayerX.ConnectionId ? "X" : "O");
            }
            else if (draw)
            {
                game.IsGameOver = true;
                UpdateStats(game.PlayerX.Name, game.PlayerO!.Name, "DRAW");
            }
            else
            {
                // Switch turn
                game.CurrentTurnConnectionId = connectionId == game.PlayerX.ConnectionId
                    ? game.PlayerO!.ConnectionId
                    : game.PlayerX.ConnectionId;
            }

            // Cleanup if game over (optional, keeps memory low)
            if (game.IsGameOver)
            {
                // We keep it briefly for display, client should handle "Left Game" to remove it fully
                // For now, we leave it in memory so users can see the result
            }

            return (true, win, draw);
        }

        private bool CheckWin(string[] b)
        {
            int[][] lines = {
                new[]{0,1,2}, new[]{3,4,5}, new[]{6,7,8},
                new[]{0,3,6}, new[]{1,4,7}, new[]{2,5,8},
                new[]{0,4,8}, new[]{2,4,6}
            };
            return lines.Any(l => !string.IsNullOrEmpty(b[l[0]]) && b[l[0]] == b[l[1]] && b[l[1]] == b[l[2]]);
        }

        // --- Persistence ---
        private void UpdateStats(string pX, string pO, string result)
        {
            if (result == "X")
            {
                _stats[pX].Wins++;
                _stats[pO].Losses++;
            }
            else if (result == "O")
            {
                _stats[pO].Wins++;
                _stats[pX].Losses++;
            }
            else
            {
                _stats[pX].Draws++;
                _stats[pO].Draws++;
            }
            SaveStats();
        }

        private void SaveStats()
        {
            lock (_fileLock)
            {
                var json = JsonSerializer.Serialize(_stats, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dbPath, json);
            }
        }

        private void LoadStats()
        {
            lock (_fileLock)
            {
                if (File.Exists(_dbPath))
                {
                    try
                    {
                        var json = File.ReadAllText(_dbPath);
                        var data = JsonSerializer.Deserialize<ConcurrentDictionary<string, UserStat>>(json);
                        if (data != null) _stats = data;
                    }
                    catch
                    {
                        _stats = new(); // Fallback if file corrupt
                    }
                }
            }
        }
    }
}