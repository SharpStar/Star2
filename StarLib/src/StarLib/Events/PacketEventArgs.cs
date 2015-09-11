using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets;
using StarLib.Server;

namespace StarLib.Events
{
    public class PacketEventArgs : EventArgs
    {
        public IStarProxy Proxy { get; set; }

        public IPacket Packet { get; set; }

        public PacketEventArgs(IStarProxy proxy, IPacket packet)
        {
            Proxy = proxy;
            Packet = packet;
        }
    }
}
