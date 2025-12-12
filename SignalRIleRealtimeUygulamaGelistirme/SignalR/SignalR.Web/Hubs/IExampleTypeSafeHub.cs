namespace SignalR.Web.Hubs;

public interface IExampleTypeSafeHub
{
    Task ReceiveMessageForAllClient(string message);
    Task ReceiveMessageForCallerClient(string message);
    Task ReceiveConnectedClientCountAllClient(int clientCount);
}