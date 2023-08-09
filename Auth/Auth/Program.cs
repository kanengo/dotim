using Auth.Infrastructure;
using Auth.Infrastructure.Rpc;
using Auth.Services;
using Sequence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<AccountService>();


builder.Services.AddGrpcClient<Sequencer.SequencerClient>(o =>
{
    o.Address = new Uri("http://localhost:5196");
});

builder.Services.AddSingleton<RpcClientAggregator>();
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthorizerService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();