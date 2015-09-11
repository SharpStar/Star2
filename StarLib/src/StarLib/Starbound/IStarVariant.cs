using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace StarLib.Starbound
{
    public interface IStarVariant
    {
        Any<double, bool, IStarString, IStarVariantArray, IStarStringMap> Json { get; set; }
    }
}
