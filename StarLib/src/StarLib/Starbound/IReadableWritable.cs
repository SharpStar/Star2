using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace StarLib.Starbound
{
    public interface IReadableWritable
    {
        void ReadFrom(IStarReader reader);

        void WriteTo(IStarWriter writer);
    }
}
