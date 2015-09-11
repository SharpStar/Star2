using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Starbound;

namespace StarLib.Packets.Starbound
{
    public interface IClientConnectPacket : IPacket
    {
        byte[] AssetDigest { get; set; }
        
        IUuid Uuid { get; set; }

        string PlayerName { get; set; }

        string PlayerSpecies { get; set; }

        Dictionary<byte[], Maybe<byte[]>> ShipChunks { get; set; }

        IShipUpgrades ShipUpgrades { get; set; }

        string Account { get; set; }
    }
}
