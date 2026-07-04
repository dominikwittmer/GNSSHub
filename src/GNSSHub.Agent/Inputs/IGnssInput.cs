using System;
using System.Collections.Generic;
using System.Text;

namespace GNSSHub.Agent.Inputs
{
    public interface IGnssInput : IAsyncDisposable
    {
        Task<Stream> OpenReadStreamAsync(CancellationToken cancellationToken);
    }
}
