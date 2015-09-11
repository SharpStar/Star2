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
    public class ConnectSuccessPacket : IConnectSuccessPacket
    {
        public byte PacketId => (byte)PacketType.ConnectionSuccess;
        public bool Ignore { get; set; }
        public bool IsReceive { get; set; }
        public bool AlwaysCompress { get; set; }
        public Origin Origin { get; set; }

        public ulong ClientId { get; set; }
        public IUuid Uuid { get; set; }
        public CelestialBaseInformation CelestialInformation { get; set; }

        public void Read(IStarReader reader)
        {
            ClientId = reader.ReadVLQ();
            Uuid = reader.Serializer.Deserialize<IUuid>(reader);
            CelestialInformation = new CelestialBaseInformation
            {
                PlanetOrbitalLevels = reader.ReadInt32(),
                SatelliteOrbitalLevels = reader.ReadInt32(),
                ChunkSize = reader.ReadInt32(),
                XyCoordRange = new Vec2I
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32()
                },
                ZCoordRange = new Vec2I
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32()
                }
            };
        }

        public void Write(IStarWriter writer)
        {
            writer.WriteVlq(ClientId);
            writer.Serializer.Serialize(writer, Uuid);
            writer.Write(CelestialInformation.PlanetOrbitalLevels);
            writer.Write(CelestialInformation.SatelliteOrbitalLevels);
            writer.Write(CelestialInformation.ChunkSize);
            writer.Write(CelestialInformation.XyCoordRange);
            writer.Write(CelestialInformation.ZCoordRange);
        }
    }
}
