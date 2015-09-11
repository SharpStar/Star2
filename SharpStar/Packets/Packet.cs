using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;

namespace SharpStar.Packets
{
    public abstract class Packet : IPacket
    {
        public byte PacketId { get; protected set; }

        public PacketType PacketType
        {
            get
            {
                return (PacketType)PacketId;
            }
        }

        public bool Ignore { get; set; }

        public bool IsReceive { get; set; }

        public virtual bool AlwaysCompress { get; set; }

        public Origin Origin { get; set; }

        public abstract void Read(IStarReader reader);

        public abstract void Write(IStarWriter writer);
    }
}
