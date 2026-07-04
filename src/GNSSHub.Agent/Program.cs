using GNSSHub.Agent;
using GNSSHub.Agent.Inputs;
using GNSSHub.Agent.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GnssHubOptions>(
    builder.Configuration.GetSection("GnssHub"));

builder.Services.AddSingleton<GnssInputFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
