using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Packets
{
    public interface IPacketProcessor
    {
        bool AddPacketType(Type type);

        bool AddPacketType<T>() where T : IPacket;

        bool RemovePacketType(Type type);

        bool RemovePacketType<T>() where T : IPacket;

        IEnumerable<IPacket> ProcessData(byte[] data, int offset, int count);
    }
}
