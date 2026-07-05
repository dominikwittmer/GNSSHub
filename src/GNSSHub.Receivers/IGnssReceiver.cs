namespace GNSSHub.Receivers;

public interface IGnssReceiver
{
    Task RunAsync(Stream inputStream, CancellationToken cancellationToken);
}