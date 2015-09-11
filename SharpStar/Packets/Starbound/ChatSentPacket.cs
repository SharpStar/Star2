using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;
using StarLib.Packets.Starbound;
using StarLib.Starbound;

namespace SharpStar.Packets.Starbound
{
    public class ChatSentPacket : IChatSentPacket
    {
        public byte PacketId => (byte)PacketType.ChatSend;
        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }
        public string Message { get; set; }
        public SendMode Mode { get; set; }

        public void Read(IStarReader reader)
        {
            Message = reader.ReadString();
            Mode = (SendMode)reader.ReadByte();
        }

        public void Write(IStarWriter writer)
        {
            writer.Write(Message);
            writer.Write((byte)Mode);
        }

    }
}
