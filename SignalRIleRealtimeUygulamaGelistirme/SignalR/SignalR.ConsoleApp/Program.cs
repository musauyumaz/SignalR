// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.SignalR.Client;
using SignalR.ConsoleApp;

Console.WriteLine("SignalR Console Client");

var connection = new HubConnectionBuilder().WithUrl("https://localhost:7133/exampleTypeSafeHub").Build();

connection.StartAsync().ContinueWith(result => Console.WriteLine(result.IsCompletedSuccessfully ? "Connected" : "Failed to connect"));

connection.On<Product>("ReceiveTypedMessageForAllClient", (product) => Console.WriteLine($"Received Message: {product.Id} - {product.Name} : {product.Price}"));

Console.ReadLine();