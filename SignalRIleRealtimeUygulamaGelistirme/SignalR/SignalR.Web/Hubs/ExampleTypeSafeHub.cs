using Microsoft.AspNetCore.SignalR;

namespace SignalR.Web.Hubs;

public class ExampleTypeSafeHub : Hub<IExampleTypeSafeHub>
{
    private static int _connectedClientCount = 0;
    public async Task BroadCastMessageToAllClient(string message) => await Clients.All.ReceiveMessageForAllClient(message);
    public async Task BroadCastMessageToCallerClient(string message) => await Clients.Caller.ReceiveMessageForCallerClient(message);
    public async Task BroadCastMessageToOthersClient(string message) => await Clients.Others.ReceiveMessageForOthersClient(message);
    public async Task BroadCastMessageToIndividualClient(string connectionId, string message) => await Clients.Client(connectionId).ReceiveMessageForIndividualClient(message);

    public override async Task OnConnectedAsync() 
    {
        _connectedClientCount++;
        await Clients.All.ReceiveConnectedClientCountAllClient(_connectedClientCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedClientCount--;
        await Clients.All.ReceiveConnectedClientCountAllClient(_connectedClientCount);
        await base.OnDisconnectedAsync(exception);
    }
}