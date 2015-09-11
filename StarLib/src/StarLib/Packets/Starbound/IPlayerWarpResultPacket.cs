using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Starbound;

namespace StarLib.Packets.Starbound
{
    public interface IPlayerWarpResultPacket : IPacket
    {
        bool Success { get; set; }

        IWarpAction WarpAction { get; set; }

        bool WarpActionInvalid { get; set; }
    }
}
