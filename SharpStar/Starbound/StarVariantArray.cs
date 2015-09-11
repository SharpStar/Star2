using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class StarVariantArray : IStarVariantArray
    {
        public List<IStarVariant> Array { get; set; }

        public void ReadFrom(IStarReader reader)
        {
            Array = reader.Serializer.Deserialize<List<IStarVariant>>(reader);
        }

        public void WriteTo(IStarWriter writer)
        {
            writer.Serializer.Serialize(writer, Array);
        }
    }
}
