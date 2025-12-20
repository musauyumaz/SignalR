namespace Minesweeper.API.Models;

public class Cell
{
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public int NeighborMines { get; set; }
    public string? RevealedBy { get; set; }
}