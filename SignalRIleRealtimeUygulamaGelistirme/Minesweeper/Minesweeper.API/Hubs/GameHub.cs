using Microsoft.AspNetCore.SignalR;
using Minesweeper.API.Services;

namespace Minesweeper.API.Hubs;

public class GameHub(GameService gameService) : Hub
{
     public async Task JoinRoom(string roomId, string playerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        var player = gameService.AddPlayer(roomId, Context.ConnectionId, playerName);
        var room = gameService.GetRoom(roomId);

        if (room is not null)
        {
            await Clients.Caller.SendAsync("GameState", new
            {
                room.Board,
                room.Players,
                room.Status,
                room.BoardSize,
                room.MineCount,
                YourPlayerId = Context.ConnectionId
            });

            // Notify all players about the new player
            await Clients.Group(roomId).SendAsync("PlayerJoined", player, room.Players);
        }
    }

    public async Task RevealCell(string roomId, int row, int col)
    {
        var result = gameService.RevealCell(roomId, row, col, Context.ConnectionId);
        
        if (result.Success)
        {
            var room = gameService.GetRoom(roomId);
            
            await Clients.Group(roomId).SendAsync("CellsRevealed", new
            {
                result.RevealedCells,
                result.HitMine,
                PlayerId = Context.ConnectionId,
                room?.Players,
                room?.Status,
                result.GameWon,
                result.WinnerId
            });
        }
    }

    public async Task ToggleFlag(string roomId, int row, int col)
    {
        var success = gameService.ToggleFlag(roomId, row, col);
        
        if (success)
        {
            await Clients.Group(roomId).SendAsync("FlagToggled", new
            {
                Row = row,
                Col = col,
                PlayerId = Context.ConnectionId
            });
        }
    }

    public async Task ResetGame(string roomId)
    {
        gameService.ResetRoom(roomId);
        var room = gameService.GetRoom(roomId);

        if (room is not null)
        {
            await Clients.Group(roomId).SendAsync("GameReset", new
            {
                room.Board,
                room.Players,
                room.Status
            });
        }
    }

    public async Task SendMessage(string roomId, string message)
    {
        var room = gameService.GetRoom(roomId);
        var player = room?.Players.FirstOrDefault(p => p.Id == Context.ConnectionId);

        if (player is not null)
        {
            await Clients.Group(roomId).SendAsync("MessageReceived", new
            {
                PlayerName = player.Name,
                PlayerColor = player.Color,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var rooms = gameService.GetAllRooms();
        
        foreach (var room in rooms)
        {
            if (room.Players.Exists(p => p.Id == Context.ConnectionId))
            {
                gameService.RemovePlayer(room.RoomId, Context.ConnectionId);
                var updatedRoom = gameService.GetRoom(room.RoomId);
                
                if (updatedRoom is not null)
                {
                    await Clients.Group(room.RoomId).SendAsync("PlayerLeft", 
                        Context.ConnectionId, 
                        updatedRoom.Players);
                }
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}