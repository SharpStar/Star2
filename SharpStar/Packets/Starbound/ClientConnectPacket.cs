using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Packets;
using StarLib.Packets.Starbound;
using StarLib.Starbound;

namespace SharpStar.Packets.Starbound
{
    public class ClientConnectPacket : IClientConnectPacket
    {
        public byte PacketId => (byte)PacketType.ClientConnect;

        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }


        public byte[] AssetDigest { get; set; }
        public IUuid Uuid { get; set; }
        public string PlayerName { get; set; }
        public string PlayerSpecies { get; set; }
        public Dictionary<byte[], Maybe<byte[]>> ShipChunks { get; set; }
        public IShipUpgrades ShipUpgrades { get; set; }
        public string Account { get; set; }

        public void Read(IStarReader reader)
        {
            AssetDigest = reader.ReadUInt8Array();
            Uuid = reader.Serializer.Deserialize<IUuid>(reader);
            PlayerName = reader.ReadString();
            PlayerSpecies = reader.ReadString();
            ShipChunks = reader.Serializer.Deserialize<Dictionary<byte[], Maybe<byte[]>>>(reader);
            ShipUpgrades = reader.Serializer.Deserialize<IShipUpgrades>(reader);
            Account = reader.ReadString();
        }

        public void Write(IStarWriter writer)
        {
            writer.WriteUInt8Array(AssetDigest);
            writer.Serializer.Serialize(writer, Uuid);
            writer.Write(PlayerName);
            writer.Write(PlayerSpecies);
            writer.Serializer.Serialize(writer, ShipChunks);
            writer.Serializer.Serialize(writer, ShipUpgrades);
            writer.Write(Account);
        }
    }
}
