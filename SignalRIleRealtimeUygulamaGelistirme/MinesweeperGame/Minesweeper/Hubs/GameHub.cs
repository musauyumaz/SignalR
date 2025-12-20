using Microsoft.AspNetCore.SignalR;
using Minesweeper.Models;
using Minesweeper.Services;

namespace Minesweeper.Hubs;

public class GameHub(GameService _gameService) : Hub
{
    public async Task JoinRoom(string roomId, string playerName)
    {
        try
        {
            var room = _gameService.GetRoom(roomId);
            if (room == null)
            {
                room = _gameService.CreateRoom(roomId);
            }

            var player = _gameService.AddPlayer(roomId, Context.ConnectionId, playerName);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.Group(roomId).SendAsync("PlayerJoined", player);
            await Clients.Group(roomId).SendAsync("PlayersUpdated", room.Players);
            
            // Board henüz oluşturulmamışsa boş board gönder
            var boardDto = room.Status == GameStatus.Playing 
                ? CreateBoardDto(room.Board, room.Rows, room.Cols)
                : new { rows = room.Rows, cols = room.Cols, cells = new List<object>() };
                
            await Clients.Caller.SendAsync("RoomJoined", new
            {
                roomId = room.RoomId,
                players = room.Players,
                status = (int)room.Status,
                board = boardDto
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JoinRoom error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            await Clients.Caller.SendAsync("Error", "Odaya katılırken hata oluştu");
        }
    }

    public async Task StartGame(string roomId)
    {
        try
        {
            _gameService.StartGame(roomId);
            var room = _gameService.GetRoom(roomId);

            if (room != null)
            {
                var boardDto = CreateBoardDto(room.Board, room.Rows, room.Cols);
                await Clients.Group(roomId).SendAsync("GameStarted", boardDto);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StartGame error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Oyun başlatılırken hata oluştu");
        }
    }

    public async Task RevealCell(string roomId, int row, int col)
    {
        try
        {
            var result = _gameService.RevealCell(roomId, row, col, Context.ConnectionId);
            var room = _gameService.GetRoom(roomId);

            if (room != null)
            {
                await Clients.Group(roomId).SendAsync("CellRevealed", result);
                await Clients.Group(roomId).SendAsync("PlayersUpdated", room.Players);

                if (result.HitMine)
                {
                    var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                    if (player != null)
                    {
                        await Clients.Group(roomId).SendAsync("PlayerEliminated", player.Name);
                    }
                }

                if (result.GameWon || room.Status == GameStatus.Finished)
                {
                    await Clients.Group(roomId).SendAsync("GameFinished", room.Players.OrderByDescending(p => p.Score).ToList());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RevealCell error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Hücre açılırken hata oluştu");
        }
    }

    public async Task ToggleFlag(string roomId, int row, int col)
    {
        try
        {
            _gameService.ToggleFlag(roomId, row, col, Context.ConnectionId);
            var room = _gameService.GetRoom(roomId);

            if (room != null && row >= 0 && row < room.Rows && col >= 0 && col < room.Cols)
            {
                var cell = room.Board[row, col];
                await Clients.Group(roomId).SendAsync("FlagToggled", cell);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ToggleFlag error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Bayrak konulurken hata oluştu");
        }
    }

    public async Task SendMessage(string roomId, string message)
    {
        try
        {
            var room = _gameService.GetRoom(roomId);
            if (room == null) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            _gameService.AddMessage(roomId, player.Name, message);

            var chatMessage = new ChatMessage
            {
                PlayerName = player.Name,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(roomId).SendAsync("MessageReceived", chatMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendMessage error: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Mesaj gönderilirken hata oluştu");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var room = _gameService.GetRoomByConnectionId(Context.ConnectionId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    _gameService.RemovePlayer(room.RoomId, Context.ConnectionId);
                    await Clients.Group(room.RoomId).SendAsync("PlayerLeft", player.Name);
                    
                    var updatedRoom = _gameService.GetRoom(room.RoomId);
                    if (updatedRoom != null)
                    {
                        await Clients.Group(room.RoomId).SendAsync("PlayersUpdated", updatedRoom.Players);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnDisconnectedAsync error: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private object CreateBoardDto(Cell[,] board, int rows, int cols)
    {
        var cells = new List<object>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                cells.Add(new
                {
                    row = board[i, j].Row,
                    col = board[i, j].Col,
                    isRevealed = board[i, j].IsRevealed,
                    isFlagged = board[i, j].IsFlagged,
                    isMine = false, // Güvenlik için client'a gönderme
                    adjacentMines = board[i, j].AdjacentMines,
                    revealedBy = board[i, j].RevealedBy
                });
            }
        }

        return new { rows, cols, cells };
    }
}