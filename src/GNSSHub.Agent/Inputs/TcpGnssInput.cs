using GNSSHub.Agent.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GNSSHub.Agent.Inputs
{
    public sealed class TcpGnssInput : IGnssInput
    {
        private readonly GnssHubInputOptions _options;
        private TcpClient? _client;

        public TcpGnssInput(GnssHubInputOptions options)
        {
            _options = options;
        }

        public async Task<Stream> OpenReadStreamAsync(CancellationToken cancellationToken)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_options.Host, _options.Port, cancellationToken);
            return _client.GetStream();
        }

        public async ValueTask DisposeAsync()
        {
            _client?.Dispose();
            await ValueTask.CompletedTask;
        }
    }
}
