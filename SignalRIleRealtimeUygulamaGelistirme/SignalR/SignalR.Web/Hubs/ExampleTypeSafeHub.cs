using Microsoft.AspNetCore.SignalR;

namespace SignalR.Web.Hubs;

public class ExampleTypeSafeHub : Hub<IExampleTypeSafeHub>
{
    private static int connectedClientCount = 0;
    public async Task BroadCastMessageToAllClient(string message) => await Clients.All.ReceiveMessageForAllClient(message);

    public override async Task OnConnectedAsync()
    {
        connectedClientCount++;
        await Clients.All.ReceiveConnectedClientCountAllClient(connectedClientCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        connectedClientCount--;
        await Clients.All.ReceiveConnectedClientCountAllClient(connectedClientCount);
        await base.OnDisconnectedAsync(exception);
    }
}