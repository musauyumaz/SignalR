using Minesweeper.Hubs;
using Minesweeper.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseWebSockets();
app.UseCors("AllowAll");

app.MapHub<GameHub>("/gamehub");

Console.WriteLine("✅ Server çalışıyor: http://localhost:5000");
app.Run("http://localhost:5000");