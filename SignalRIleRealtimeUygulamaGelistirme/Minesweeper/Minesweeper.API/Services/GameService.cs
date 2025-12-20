using System.Collections.Concurrent;
using Minesweeper.API.Models;

namespace Minesweeper.API.Services;

public class GameService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

    private static readonly string[] Colors =
        ["#3b82f6", "#ef4444", "#10b981", "#f59e0b", "#8b5cf6", "#ec4899", "#06b6d4"];

    public GameRoom CreateRoom(string roomId, int boardSize = 10, int mineCount = 15)
    {
        var room = new GameRoom
        {
            RoomId = roomId,
            BoardSize = boardSize,
            MineCount = mineCount,
            CreatedAt = DateTime.UtcNow,
            Board = CreateBoard(boardSize, mineCount)
        };

        _rooms.TryAdd(roomId, room);
        return room;
    }

    public GameRoom? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public GameRoom GetOrCreateRoom(string roomId)
    {
        return _rooms.GetOrAdd(roomId, _ => new GameRoom
        {
            RoomId = roomId,
            BoardSize = 10,
            MineCount = 15,
            CreatedAt = DateTime.UtcNow,
            Board = CreateBoard(10, 15)
        });
    }

    public Player AddPlayer(string roomId, string connectionId, string playerName)
    {
        var room = GetOrCreateRoom(roomId);

        var player = new Player
        {
            Id = connectionId,
            Name = playerName,
            Color = Colors[room.Players.Count % Colors.Length],
            Score = 0,
            JoinedAt = DateTime.UtcNow
        };

        room.Players.Add(player);

        if (room.Status == GameStatus.Waiting && room.Players.Count >= 1)
        {
            room.Status = GameStatus.Playing;
        }

        return player;
    }

    public void RemovePlayer(string roomId, string connectionId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.Players.RemoveAll(p => p.Id == connectionId);

            if (room.Players.Count == 0)
            {
                _rooms.TryRemove(roomId, out _);
            }
        }
    }

    public RevealResult RevealCell(string roomId, int row, int col, string playerId)
    {
        var room = GetRoom(roomId);
        if (room is null || room.Status != GameStatus.Playing)
            return new RevealResult { Success = false };

        if (row < 0 || row >= room.BoardSize || col < 0 || col >= room.BoardSize)
            return new RevealResult { Success = false };

        var cell = room.Board[row][col];

        if (cell.IsRevealed || cell.IsFlagged)
            return new RevealResult { Success = false };

        if (cell.IsMine)
        {
            cell.IsRevealed = true;
            room.Status = GameStatus.Lost;
            return new RevealResult
            {
                Success = true,
                HitMine = true,
                RevealedCells = [(row, col)]
            };
        }

        var revealedCells = new List<(int, int)>();
        RevealCellRecursive(room, row, col, playerId, revealedCells);

        // Update player score
        var player = room.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is not null)
        {
            player.Score += revealedCells.Count;
        }

        // Check win condition
        var unrevealedSafeCells = room.Board
            .SelectMany(r => r)
            .Count(c => !c.IsRevealed && !c.IsMine);

        if (unrevealedSafeCells == 0)
        {
            room.Status = GameStatus.Won;
            room.WinnerId = room.Players.OrderByDescending(p => p.Score).First().Id;
        }

        return new RevealResult
        {
            Success = true,
            RevealedCells = revealedCells,
            GameWon = room.Status == GameStatus.Won,
            WinnerId = room.WinnerId
        };
    }

    private void RevealCellRecursive(GameRoom room, int row, int col, string playerId, List<(int, int)> revealedCells)
    {
        if (row < 0 || row >= room.BoardSize || col < 0 || col >= room.BoardSize)
            return;

        var cell = room.Board[row][col];
        if (cell.IsRevealed || cell.IsMine || cell.IsFlagged)
            return;

        cell.IsRevealed = true;
        cell.RevealedBy = playerId;
        revealedCells.Add((row, col));

        if (cell.NeighborMines == 0)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr != 0 || dc != 0)
                    {
                        RevealCellRecursive(room, row + dr, col + dc, playerId, revealedCells);
                    }
                }
            }
        }
    }

    public bool ToggleFlag(string roomId, int row, int col)
    {
        var room = GetRoom(roomId);
        if (room is null || room.Status != GameStatus.Playing)
            return false;

        if (row < 0 || row >= room.BoardSize || col < 0 || col >= room.BoardSize)
            return false;

        var cell = room.Board[row][col];
        if (cell.IsRevealed)
            return false;

        cell.IsFlagged = !cell.IsFlagged;
        return true;
    }

    public void ResetRoom(string roomId)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.Board = CreateBoard(room.BoardSize, room.MineCount);
            room.Status = GameStatus.Playing;
            room.WinnerId = null;

            foreach (var player in room.Players)
            {
                player.Score = 0;
            }
        }
    }

    private static List<List<Cell>> CreateBoard(int size, int mineCount)
    {
        var board = new List<List<Cell>>();

        for (int i = 0; i < size; i++)
        {
            var row = new List<Cell>();
            for (int j = 0; j < size; j++)
            {
                row.Add(new Cell());
            }

            board.Add(row);
        }

        // Place mines
        var random = Random.Shared;
        var minesPlaced = 0;

        while (minesPlaced < mineCount)
        {
            var row = random.Next(size);
            var col = random.Next(size);

            if (!board[row][col].IsMine)
            {
                board[row][col].IsMine = true;
                minesPlaced++;
            }
        }

        // Calculate neighbor mines
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (!board[row][col].IsMine)
                {
                    int count = 0;
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            int newRow = row + dr;
                            int newCol = col + dc;
                            if (newRow >= 0 && newRow < size && newCol >= 0 && newCol < size)
                            {
                                if (board[newRow][newCol].IsMine)
                                    count++;
                            }
                        }
                    }

                    board[row][col].NeighborMines = count;
                }
            }
        }

        return board;
    }

    public List<GameRoom> GetAllRooms()
    {
        return [.. _rooms.Values];
    }
}

public class RevealResult
{
    public bool Success { get; set; }
    public bool HitMine { get; set; }
    public List<(int Row, int Col)> RevealedCells { get; set; } = [];
    public bool GameWon { get; set; }
    public string? WinnerId { get; set; }
}