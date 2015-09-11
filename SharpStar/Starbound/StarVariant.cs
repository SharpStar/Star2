using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class StarVariant : IStarVariant
    {
        public Any<double, bool, IStarString, IStarVariantArray, IStarStringMap> Json { get; set; }
    }
}
