using Microsoft.AspNetCore.SignalR.Client;
using SignalR.ConsoleApp;

namespace SignalR.WorkerServiceApp;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private HubConnection _hubConnection;
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _hubConnection = new HubConnectionBuilder().WithUrl(configuration["SignalR:Hub"]).Build();
        _hubConnection.StartAsync().ContinueWith(result => logger.LogInformation(result.IsCompletedSuccessfully ? "Connected" : "Failed to connect"));
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _hubConnection.StopAsync(cancellationToken);
        await _hubConnection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _hubConnection.On<Product>("ReceiveTypedMessageForAllClient", (product) => logger.LogInformation($"Received Message: {product.Id} - {product.Name} : {product.Price}"));
        return Task.CompletedTask;
    }
}