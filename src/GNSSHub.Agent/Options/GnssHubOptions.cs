using System;
using System.Collections.Generic;
using System.Text;

namespace GNSSHub.Agent.Options
{
    public sealed class GnssHubOptions
    {
        public GnssHubInputOptions Input { get; set; } = new();
    }

    public sealed class GnssHubInputOptions
    {
        public string Type { get; set; } = "Tcp";
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5015;
        public string PortName { get; set; } = "/dev/ttyUSB0";
        public int BaudRate { get; set; } = 115200;
    }
}
