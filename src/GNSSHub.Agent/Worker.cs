using GNSSHub.Agent.Inputs;
using GNSSHub.Agent.Options;
using GNSSHub.Protocols;
using GNSSHub.Receivers;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace GNSSHub.Agent;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly GnssInputFactory _inputFactory;
    private readonly IGnssReceiver _receiver;

    public Worker(
        ILogger<Worker> logger,
        GnssInputFactory inputFactory,
        IGnssReceiver receiver)
    {
        _logger = logger;
        _inputFactory = inputFactory;
        _receiver = receiver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var input = _inputFactory.Create();

                _logger.LogInformation("Opening GNSS input...");
                await using var stream = await input.OpenReadStreamAsync(stoppingToken);

                _logger.LogInformation("GNSS input opened.");
                await _receiver.RunAsync(stream, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RTCM stream disconnected. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}