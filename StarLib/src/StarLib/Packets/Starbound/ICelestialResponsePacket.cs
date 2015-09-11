using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Starbound;

namespace StarLib.Packets.Starbound
{
    public interface ICelestialResponsePacket : IPacket
    {
        //List<IEither<ICelestialChunk, ICelestialSystemObjects>> Responses { get; set; }
    }
}
