using Microsoft.AspNetCore.Mvc;
using Minesweeper.API.Services;

namespace Minesweeper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(GameService gameService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllRooms()
    {
        var rooms = gameService.GetAllRooms()
            .Select(r => new
            {
                r.RoomId,
                r.Status,
                PlayerCount = r.Players.Count,
                r.CreatedAt,
                r.BoardSize,
                r.MineCount
            });

        return Ok(rooms);
    }

    [HttpPost]
    public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
    {
        var roomId = string.IsNullOrEmpty(request.RoomId) 
            ? Guid.NewGuid().ToString("N")[..8] 
            : request.RoomId;

        var room = gameService.CreateRoom(roomId, request.BoardSize, request.MineCount);
        
        return Ok(new { room.RoomId });
    }

    [HttpGet("{roomId}")]
    public IActionResult GetRoom(string roomId)
    {
        var room = gameService.GetRoom(roomId);
        
        if (room is null)
            return NotFound();

        return Ok(new
        {
            room.RoomId,
            room.Status,
            room.Players,
            room.BoardSize,
            room.MineCount,
            room.CreatedAt
        });
    }
}
public record CreateRoomRequest(string? RoomId, int BoardSize = 10, int MineCount = 15);