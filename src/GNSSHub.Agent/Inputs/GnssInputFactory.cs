using GNSSHub.Agent.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace GNSSHub.Agent.Inputs
{
    public sealed class GnssInputFactory
    {
        private readonly GnssHubOptions _options;

        public GnssInputFactory(Microsoft.Extensions.Options.IOptions<GnssHubOptions> options)
        {
            _options = options.Value;
        }

        public IGnssInput Create()
        {
            return _options.Input.Type.ToLowerInvariant() switch
            {
                "tcp" => new TcpGnssInput(_options.Input),
                _ => throw new NotSupportedException($"Input type '{_options.Input.Type}' is not supported.")
            };
        }
    }
}
