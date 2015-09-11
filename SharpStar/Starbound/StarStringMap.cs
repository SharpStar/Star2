using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class StarStringMap : IStarStringMap
    {
        public Dictionary<IStarString, IStarVariant> Map { get; set; }

        public void ReadFrom(IStarReader reader)
        {
            Map = reader.Serializer.Deserialize<Dictionary<IStarString, IStarVariant>>(reader);
        }

        public void WriteTo(IStarWriter writer)
        {
            writer.Serializer.Serialize(writer, Map);
        }
    }
}
