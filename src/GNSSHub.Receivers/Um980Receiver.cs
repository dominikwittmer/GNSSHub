using GNSSHub.Protocols;
using Microsoft.Extensions.Logging;

namespace GNSSHub.Receivers
{
    public sealed class Um980Receiver : IGnssReceiver
    {
        private readonly ILogger<Um980Receiver> _logger;

        public Um980Receiver(ILogger<Um980Receiver> logger)
        {
            _logger = logger;
        }

        public async Task RunAsync(Stream inputStream, CancellationToken cancellationToken)
        {
            var reader = new Rtcm3PacketReader(inputStream);

            long packets = 0;
            var byType = new Dictionary<int, long>();

            await foreach (var packet in reader.ReadPacketsAsync(cancellationToken))
            {
                packets++;

                byType.TryGetValue(packet.MessageType, out var count);
                byType[packet.MessageType] = count + 1;

                if (packets % 100 == 0)
                {
                    var stats = string.Join(", ",
                        byType.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));

                    _logger.LogInformation("UM980 RTCM packets={Packets} [{Stats}]", packets, stats);
                }
            }
        }
    }
}
