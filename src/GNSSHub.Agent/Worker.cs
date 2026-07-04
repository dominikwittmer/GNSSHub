using System.Net.Sockets;
using GNSSHub.Protocols;

namespace GNSSHub.Agent;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var client = new TcpClient();

                _logger.LogInformation("Connecting to RTKBase RTCM stream at 127.0.0.1:5015...");
                await client.ConnectAsync("127.0.0.1", 5015, stoppingToken);

                _logger.LogInformation("Connected.");

                await using var stream = client.GetStream();

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