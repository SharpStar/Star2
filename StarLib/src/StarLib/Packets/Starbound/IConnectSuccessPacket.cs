using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Starbound;

namespace StarLib.Packets.Starbound
{
    public interface IConnectSuccessPacket : IPacket
    {
        ulong ClientId { get; set; }

        IUuid Uuid { get; set; }

        CelestialBaseInformation CelestialInformation { get; set; }
    }
}
