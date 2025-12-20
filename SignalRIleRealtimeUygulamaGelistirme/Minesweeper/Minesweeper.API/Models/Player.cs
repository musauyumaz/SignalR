namespace Minesweeper.API.Models;

public class Player
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public int Score { get; set; }
    public DateTime JoinedAt { get; set; }
}