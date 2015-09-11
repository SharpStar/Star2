using System.Collections.Generic;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class ShipUpgrades : IShipUpgrades
    {
        public int ShipLevel { get; set; }
        public int MaxFuel { get; set; }
        public List<string> Capabilities { get; set; }

        public void ReadFrom(IStarReader reader)
        {
            ShipLevel = reader.ReadInt32();
            MaxFuel = reader.ReadInt32();
            Capabilities = reader.Serializer.Deserialize<List<string>>(reader);
        }

        public void WriteTo(IStarWriter writer)
        {
            writer.Write(ShipLevel);
            writer.Write(MaxFuel);
            writer.Serializer.Serialize(writer, Capabilities);
        }
    }
}
