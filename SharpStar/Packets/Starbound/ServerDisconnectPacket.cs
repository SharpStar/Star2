using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;
using StarLib.Packets.Starbound;

namespace SharpStar.Packets.Starbound
{
    public class ServerDisconnectPacket : IServerDisconnectPacket
    {
        public byte PacketId => (byte)PacketType.ServerDisconnect;
        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }
        public string Reason { get; set; }

        public void Read(IStarReader reader)
        {
            Reason = reader.ReadString();
        }

        public void Write(IStarWriter writer)
        {
            writer.Write(Reason);
        }
    }
}
