using GNSSHub.Agent.Inputs;
using GNSSHub.Agent.Options;
using GNSSHub.Protocols;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace GNSSHub.Agent;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly GnssInputFactory _inputFactory;

    public Worker(
        ILogger<Worker> logger,
        GnssInputFactory inputFactory)
    {
        _logger = logger;
        _inputFactory = inputFactory;
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
                var reader = new Rtcm3PacketReader(stream);

                long packets = 0;
                long bytes = 0;
                DateTime lastReport = DateTime.UtcNow;

                await foreach (var packet in reader.ReadPacketsAsync(stoppingToken))
                {
                    packets++;
                    bytes += packet.Length + 6;

                    var now = DateTime.UtcNow;
                    if ((now - lastReport).TotalSeconds >= 5)
                    {
                        _logger.LogInformation(
                            "RTCM packets={Packets}, bytes={Bytes}, lastType={Type}, lastLength={Length}",
                            packets,
                            bytes,
                            packet.MessageType,
                            packet.Length);

                        lastReport = now;
                    }
                }
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