using Minesweeper.Models;

namespace Minesweeper.Services;

public class GameService
{
    private readonly Dictionary<string, GameRoom> _rooms = new();
    private readonly Random _random = new();
    private readonly object _lock = new();

    public GameRoom CreateRoom(string roomId)
    {
        lock (_lock)
        {
            var room = new GameRoom { RoomId = roomId };
            _rooms[roomId] = room;
            return room;
        }
    }

    public GameRoom? GetRoom(string roomId)
    {
        lock (_lock)
        {
            return _rooms.GetValueOrDefault(roomId);
        }
    }

    public List<GameRoom> GetAllRooms()
    {
        lock (_lock)
        {
            return _rooms.Values.ToList();
        }
    }

    public Player AddPlayer(string roomId, string connectionId, string playerName)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            if (room == null) return null!;

            var player = new Player
            {
                ConnectionId = connectionId,
                Name = playerName,
                Score = 0,
                IsAlive = true
            };

            room.Players.Add(player);
            return player;
        }
    }

    public void RemovePlayer(string roomId, string connectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            if (room == null) return;

            room.Players.RemoveAll(p => p.ConnectionId == connectionId);

            if (room.Players.Count == 0)
            {
                _rooms.Remove(roomId);
            }
        }
    }

    public void StartGame(string roomId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            if (room == null || room.Status != GameStatus.Waiting) return;

            room.Status = GameStatus.Playing;
            InitializeBoard(room);
        }
    }

    private void InitializeBoard(GameRoom room)
    {
        room.Board = new Cell[room.Rows, room.Cols];

        for (int i = 0; i < room.Rows; i++)
        {
            for (int j = 0; j < room.Cols; j++)
            {
                room.Board[i, j] = new Cell { Row = i, Col = j };
            }
        }

        PlaceMines(room);
        CalculateAdjacentMines(room);
    }

    private void PlaceMines(GameRoom room)
    {
        int placed = 0;
        while (placed < room.MineCount)
        {
            int row = _random.Next(room.Rows);
            int col = _random.Next(room.Cols);

            if (!room.Board[row, col].IsMine)
            {
                room.Board[row, col].IsMine = true;
                placed++;
            }
        }
    }

    private void CalculateAdjacentMines(GameRoom room)
    {
        for (int i = 0; i < room.Rows; i++)
        {
            for (int j = 0; j < room.Cols; j++)
            {
                if (room.Board[i, j].IsMine) continue;

                int count = 0;
                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = -1; dj <= 1; dj++)
                    {
                        if (di == 0 && dj == 0) continue;
                        int ni = i + di;
                        int nj = j + dj;

                        if (ni >= 0 && ni < room.Rows && nj >= 0 && nj < room.Cols)
                        {
                            if (room.Board[ni, nj].IsMine) count++;
                        }
                    }
                }
                room.Board[i, j].AdjacentMines = count;
            }
        }
    }

    public RevealResult RevealCell(string roomId, int row, int col, string playerConnectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            var result = new RevealResult();

            if (room == null || room.Status != GameStatus.Playing) return result;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == playerConnectionId);
            if (player == null || !player.IsAlive) return result;

            var cell = room.Board[row, col];
            if (cell.IsRevealed || cell.IsFlagged) return result;

            if (cell.IsMine)
            {
                cell.IsRevealed = true;
                cell.RevealedBy = player.Name;
                result.HitMine = true;
                player.IsAlive = false;
                result.RevealedCells.Add(cell);

                if (room.Players.All(p => !p.IsAlive))
                {
                    room.Status = GameStatus.Finished;
                    RevealAllMines(room, result);
                }
                return result;
            }

            FloodFill(room, row, col, player.Name, result);
            player.Score += result.RevealedCells.Count;

            var nonMineCells = room.Rows * room.Cols - room.MineCount;
            var revealedCount = 0;
            for (int i = 0; i < room.Rows; i++)
            {
                for (int j = 0; j < room.Cols; j++)
                {
                    if (room.Board[i, j].IsRevealed && !room.Board[i, j].IsMine)
                        revealedCount++;
                }
            }

            if (revealedCount == nonMineCells)
            {
                result.GameWon = true;
                room.Status = GameStatus.Finished;
            }

            return result;
        }
    }

    private void FloodFill(GameRoom room, int row, int col, string playerName, RevealResult result)
    {
        if (row < 0 || row >= room.Rows || col < 0 || col >= room.Cols)
            return;

        var cell = room.Board[row, col];
        if (cell.IsRevealed || cell.IsMine || cell.IsFlagged)
            return;

        cell.IsRevealed = true;
        cell.RevealedBy = playerName;
        result.RevealedCells.Add(cell);

        if (cell.AdjacentMines == 0)
        {
            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    if (di == 0 && dj == 0) continue;
                    FloodFill(room, row + di, col + dj, playerName, result);
                }
            }
        }
    }

    private void RevealAllMines(GameRoom room, RevealResult result)
    {
        for (int i = 0; i < room.Rows; i++)
        {
            for (int j = 0; j < room.Cols; j++)
            {
                if (room.Board[i, j].IsMine && !room.Board[i, j].IsRevealed)
                {
                    room.Board[i, j].IsRevealed = true;
                    result.RevealedCells.Add(room.Board[i, j]);
                }
            }
        }
    }

    public void ToggleFlag(string roomId, int row, int col, string playerConnectionId)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            if (room == null || room.Status != GameStatus.Playing) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == playerConnectionId);
            if (player == null || !player.IsAlive) return;

            if (row < 0 || row >= room.Rows || col < 0 || col >= room.Cols) return;

            var cell = room.Board[row, col];
            if (!cell.IsRevealed)
            {
                cell.IsFlagged = !cell.IsFlagged;
            }
        }
    }

    public void AddMessage(string roomId, string playerName, string message)
    {
        lock (_lock)
        {
            var room = GetRoom(roomId);
            if (room == null) return;

            room.Messages.Add(new ChatMessage
            {
                PlayerName = playerName,
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            if (room.Messages.Count > 100)
            {
                room.Messages.RemoveAt(0);
            }
        }
    }

    public Player? GetPlayerByConnectionId(string connectionId)
    {
        lock (_lock)
        {
            foreach (var room in _rooms.Values)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    return player;
                }
            }
            return null;
        }
    }

    public GameRoom? GetRoomByConnectionId(string connectionId)
    {
        lock (_lock)
        {
            foreach (var room in _rooms.Values)
            {
                if (room.Players.Any(p => p.ConnectionId == connectionId))
                {
                    return room;
                }
            }
            return null;
        }
    }
}