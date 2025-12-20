using Microsoft.AspNetCore.SignalR;

namespace SignalRServerExample.Hubs;

public class MyHub : Hub
{
    private static List<string> clients = new();
    public async Task SendMessageAsync(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public override async Task OnConnectedAsync()
    {
        clients.Add(Context.ConnectionId);
        await Clients.All.SendAsync("clients", clients);
        await Clients.All.SendAsync("UserJoined", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        clients.Remove(Context.ConnectionId);
        await Clients.All.SendAsync("clients", clients);
        await Clients.All.SendAsync("UserLeft", Context.ConnectionId);
    }
}
#region BaglantiOlayları
//Bağlantı olayları signalR uygulamalarında loglama için oldukça elverişli fonksiyonlardır.
//ConnectionId : Hub'a bağlantı gerçekleştiren client'lara sistem tarafından verilen unique/tekil bir değerdir. Amacı client'ları birbirlerinden ayırmaktır.
#endregion