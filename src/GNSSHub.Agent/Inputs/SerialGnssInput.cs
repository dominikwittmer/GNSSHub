using System.IO.Ports;
using GNSSHub.Agent.Options;

namespace GNSSHub.Agent.Inputs;

public sealed class SerialGnssInput : IGnssInput
{
    private readonly GnssHubInputOptions _options;
    private SerialPort? _serialPort;

    public SerialGnssInput(GnssHubInputOptions options)
    {
        _options = options;
    }

    public Task<Stream> OpenReadStreamAsync(CancellationToken cancellationToken)
    {
        _serialPort = new SerialPort(
            _options.PortName,
            _options.BaudRate,
            Parity.None,
            8,
            StopBits.One);

        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 5000;
        _serialPort.Open();

        return Task.FromResult(_serialPort.BaseStream);
    }

    public ValueTask DisposeAsync()
    {
        _serialPort?.Dispose();
        return ValueTask.CompletedTask;
    }
}