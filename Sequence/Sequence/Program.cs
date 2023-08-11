using Sequence.Infrastructure;
using Sequence.Infrastructure.Config;
using Sequence.Infrastructure.Data;
using Sequence.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InfrastructureAggregator>();

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<SequencerService>();
// Configure the HTTP request pipeline.
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();