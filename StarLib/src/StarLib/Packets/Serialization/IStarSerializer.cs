using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace StarLib.Packets.Serialization
{
    public interface IStarSerializer
    {
        void Serialize<T>(IStarWriter writer, T value);

        T Deserialize<T>(IStarReader reader);
    }
}
