using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

var hubConnection = new HubConnectionBuilder().WithUrl(new Uri("http://localhost:5241/ws_subscribe")).Build();

await hubConnection.StartAsync();

app.Run();