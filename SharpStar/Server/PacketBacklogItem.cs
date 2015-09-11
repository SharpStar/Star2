using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets;
using StarLib.Server;

namespace SharpStar.Server
{
    public struct PacketBacklogItem
    {
        public Destination Destination { get; set; }

        public IPacket Packet { get; set; }
    }
}
