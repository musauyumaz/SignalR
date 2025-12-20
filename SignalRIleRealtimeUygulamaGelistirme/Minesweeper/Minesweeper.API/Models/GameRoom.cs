namespace Minesweeper.API.Models;

public class GameRoom
{
    public required string RoomId { get; set; }
    public List<Player> Players { get; set; } = [];
    public List<List<Cell>> Board { get; set; } = [];
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public DateTime CreatedAt { get; set; }
    public int BoardSize { get; set; } = 10;
    public int MineCount { get; set; } = 15;
    public string? WinnerId { get; set; }
}

public enum GameStatus
{
    Waiting,
    Playing,
    Won,
    Lost
}