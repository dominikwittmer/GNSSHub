namespace GNSSHub.Protocols;

public sealed record Rtcm3Packet(int MessageType, int Length, byte[] Data);

public sealed class Rtcm3PacketReader
{
    private readonly Stream _stream;

    public Rtcm3PacketReader(Stream stream)
    {
        _stream = stream;
    }

    public async IAsyncEnumerable<Rtcm3Packet> ReadPacketsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int b = await ReadByteAsync(cancellationToken);

            if (b != 0xD3)
                continue;

            int b1 = await ReadByteAsync(cancellationToken);
            int b2 = await ReadByteAsync(cancellationToken);

            int length = ((b1 & 0x03) << 8) | b2;

            byte[] payloadAndCrc = new byte[length + 3];
            await ReadExactlyAsync(payloadAndCrc, cancellationToken);

            if (length < 2)
                continue;

            int messageType =
                (payloadAndCrc[0] << 4) |
                (payloadAndCrc[1] >> 4);

            yield return new Rtcm3Packet(messageType, length, payloadAndCrc);
        }
    }

    private async Task<int> ReadByteAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1];
        await ReadExactlyAsync(buffer, cancellationToken);
        return buffer[0];
    }

    private async Task ReadExactlyAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = await _stream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset),
                cancellationToken);

            if (read == 0)
                throw new EndOfStreamException();

            offset += read;
        }
    }
}