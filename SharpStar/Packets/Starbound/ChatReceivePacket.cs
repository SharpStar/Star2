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
    public class ChatReceivePacket : IChatReceivePacket
    {
        public byte PacketId => (byte)PacketType.ChatReceived;

        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }

        public ChatReceivedMessage ReceivedMessage { get; set; }

        public void Read(IStarReader reader)
        {
            ReceivedMessage = new ChatReceivedMessage
            {
                Context = new MessageContext
                {
                    Mode = (Mode)reader.ReadByte(),
                    ChannelName = reader.ReadString()
                },
                ClientId = reader.ReadInt32(),
                FromNick = reader.ReadString(),
                Text = reader.ReadString()
            };
        }

        public void Write(IStarWriter writer)
        {
            writer.Write((byte)ReceivedMessage.Context.Mode);
            writer.Write(ReceivedMessage.Context.ChannelName);
            writer.Write(ReceivedMessage.ClientId);
            writer.Write(ReceivedMessage.FromNick);
            writer.Write(ReceivedMessage.Text);
        }
    }
}
