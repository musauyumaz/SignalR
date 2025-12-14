using Microsoft.AspNetCore.SignalR;
using SignalR.Web.Models;

namespace SignalR.Web.Hubs;

public class ExampleTypeSafeHub : Hub<IExampleTypeSafeHub>
{
    private static int _connectedClientCount = 0;
    public async Task BroadCastMessageToAllClient(string message) => await Clients.All.ReceiveMessageForAllClient(message);
    public async Task BroadCastTypedMessageToAllClient(Product product) => await Clients.All.ReceiveTypedMessageForAllClient(product);
    public async Task BroadCastMessageToCallerClient(string message) => await Clients.Caller.ReceiveMessageForCallerClient(message);
    public async Task BroadCastMessageToOthersClient(string message) => await Clients.Others.ReceiveMessageForOthersClient(message);
    public async Task BroadCastMessageToIndividualClient(string connectionId, string message) => await Clients.Client(connectionId).ReceiveMessageForIndividualClient(message);

    public async Task BroadcastStreamDataToAllClient(IAsyncEnumerable<string> nameAsChunk)
    {
        await foreach (string name in nameAsChunk) await Clients.All.ReceiveMessageAsStreamForAllClient(name);
    }

    public async Task BroadcastMessageToGroupClients(string groupName, string message)
    {
        await Clients.Group(groupName).ReceiveMessageForGroupClients(message);
    }

    public async Task AddUserToGroupAsync(string groupName)
    { 
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.ReceiveMessageForCallerClient($"{groupName} grubuna dahil oldunuz.");
        //await Clients.Others.ReceiveMessageForOthersClient($"Kullanıcı {Context.ConnectionId} {groupName} grubuna dahil oldu.");
        await BroadcastMessageToGroupClients(groupName, $"Kullanıcı {Context.ConnectionId} {groupName} grubuna dahil oldu.");
    }
    public async Task RemoveUserToGroupAsync(string groupName)
    { 
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.ReceiveMessageForCallerClient($"{groupName} grubundan ayrıldınız.");
        //await Clients.Others.ReceiveMessageForOthersClient($"Kullanıcı {Context.ConnectionId} {groupName} grubundan ayrıldı.");
        await BroadcastMessageToGroupClients(groupName, $"Kullanıcı {Context.ConnectionId} {groupName} grubundan ayrıldı.");
    }

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