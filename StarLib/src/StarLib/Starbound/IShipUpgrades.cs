using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public interface IShipUpgrades : IReadableWritable
    {
        int ShipLevel { get; set; }

        int MaxFuel { get; set; }

        List<string> Capabilities { get; set; }
    }
}
