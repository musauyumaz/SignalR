namespace Minesweeper.Models;

public class Player
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsAlive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public int AdjacentMines { get; set; }
    public string? RevealedBy { get; set; }
}

public class GameRoom
{
    public string RoomId { get; set; } = string.Empty;
    public List<Player> Players { get; set; } = new();
    public Cell[,] Board { get; set; } = new Cell[0, 0];
    public int Rows { get; set; } = 10;
    public int Cols { get; set; } = 10;
    public int MineCount { get; set; } = 15;
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatMessage
{
    public string PlayerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum GameStatus
{
    Waiting,
    Playing,
    Finished
}

public class RevealResult
{
    public List<Cell> RevealedCells { get; set; } = new();
    public bool HitMine { get; set; }
    public bool GameWon { get; set; }
}