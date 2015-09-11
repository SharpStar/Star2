using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Packets.Starbound
{
    public interface IProtocolVersionPacket : IPacket
    {
        int ProtocolVersion { get; set; }
    }
}
