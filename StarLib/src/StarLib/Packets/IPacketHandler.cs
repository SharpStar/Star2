using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Packets
{
    public interface IPacketHandler
    {
        byte PacketId { get; }

        Type Type { get; }

        //Task HandleBeforeAsync(Packet packet, StarConnection connection);

        //Task HandleAfterAsync(Packet packet, StarConnection connection);
    }
}
