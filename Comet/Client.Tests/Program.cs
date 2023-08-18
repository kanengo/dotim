using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

var hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:6666/ws_subscribe").Build();

hubConnection.On<string,string>("ReceiveMessage", (user, message) =>
{

});

await hubConnection.StartAsync();

app.Run();