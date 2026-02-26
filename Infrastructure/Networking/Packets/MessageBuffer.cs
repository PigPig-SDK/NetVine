using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Networking.Packets;

public class MessageBuffer
{
    private readonly List<byte> _buffer = new();

    private Lock _lock = new Lock();

    /// <summary>
    /// Append data to the buffer. Use TryReadPacket(...) for the managing the buffered data.
    /// </summary>
    /// <param name="data">The byte data to append to the buffer</param>
    /// <param name="offset">The offset to use for each 'packet segment'</param>
    /// <param name="size">The size from each 'offset'</param>
    /// <remarks>Data may contain multiple packets for optimization.</remarks>
    public void Append(byte[] data, long offset, long size)
    {
        if (size <= 0) return;
        lock (_lock)
        {
            _buffer.AddRange(new ArraySegment<byte>(data, (int)offset, (int)size));
        }
    }
    /// <summary>
    /// Attempts to read a valid packet, If no packet is visualized, you will get nothing out.
    /// </summary>
    /// <param name="packetBytes">The fully visualized packet</param>
    /// <returns>True if a packet is ready to be pumped... out... man..</returns>
    public bool TryReadPacket(out byte[]? packetBytes)
    {
        packetBytes = null;
        lock (_lock)
        {
            if (_buffer.Count < 4)//4 bytes for reading message length
                return false;

            int messageLength = BitConverter.ToInt32(new[] { _buffer[0], _buffer[1], _buffer[2], _buffer[3] }, 0);//Use first 4 bytes to get message length

            if (_buffer.Count < 4 + messageLength)//The full message is there.
                return false;

            //Pop the message bytes
            packetBytes = _buffer.Take(4 + messageLength).ToArray();
            _buffer.RemoveRange(0, 4 + messageLength);
            return true;
        }
    }

    public void Clear() => _buffer.Clear();
}
