using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;
using StarLib.Packets.Starbound;

namespace SharpStar.Packets.Starbound
{
    public class HandshakeResponsePacket : IHandshakeResponsePacket
    {
        public byte PacketId => (byte)PacketType.HandshakeResponse;
        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }
        public byte[] PasswordHash { get; set; }

        public void Read(IStarReader reader)
        {
            PasswordHash = reader.ReadUInt8Array();
        }

        public void Write(IStarWriter writer)
        {
            writer.Write(PasswordHash);
        }
    }
}
