using Comet.Hubs;
using Comet.Services;
using Microsoft.AspNetCore.Http.Connections;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

builder.Services.AddGrpc();

var app = builder.Build();

app.UseRouting();

app.MapHub<SubscribeHub>("/ws_subscribe", options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
});



// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


// app.Use(async (context, next) =>
// {
//     if (context.Request.Path == "/ws_subscribe")
//     {
//         if (!context.WebSockets.IsWebSocketRequest)
//         {
//             // context.Response.StatusCode = StatusCodes.Status400BadRequest;
//             // return;
//         }
//
//         await next(context);
//     }
// });

app.Run();