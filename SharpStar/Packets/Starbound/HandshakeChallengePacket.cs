using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;
using StarLib.Packets.Starbound;

namespace SharpStar.Packets.Starbound
{
    public class HandshakeChallengePacket : IHandshakeChallengePacket
    {
        public byte PacketId => (byte)PacketType.HandshakeChallenge;

        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }

        public byte[] PasswordSalt { get; set; }

        public void Read(IStarReader reader)
        {
            PasswordSalt = reader.ReadUInt8Array();
        }

        public void Write(IStarWriter writer)
        {
            writer.WriteUInt8Array(PasswordSalt);
        }
    }
}
