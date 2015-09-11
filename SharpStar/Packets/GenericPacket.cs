using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace SharpStar.Packets
{
    public class GenericPacket : Packet
    {
        public byte[] Data { get; set; }

        public GenericPacket(byte packetId)
        {
            PacketId = packetId;
        }

        public override void Read(IStarReader reader)
        {
            Data = reader.ReadToEnd();
        }

        public override void Write(IStarWriter writer)
        {
            writer.Write(Data);
        }
    }
}
