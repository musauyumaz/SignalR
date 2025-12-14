using SignalR.Web.Models;

namespace SignalR.Web.Hubs;

public interface IExampleTypeSafeHub
{
    Task ReceiveMessageForAllClient(string message);
    Task ReceiveMessageAsStreamForAllClient(string name);
    Task ReceiveTypedMessageForAllClient(Product product);
    Task ReceiveMessageForCallerClient(string message);
    Task ReceiveMessageForOthersClient(string message);
    Task ReceiveMessageForIndividualClient(string message);
    Task ReceiveMessageForGroupClients(string message);
    Task ReceiveConnectedClientCountAllClient(int clientCount);
}