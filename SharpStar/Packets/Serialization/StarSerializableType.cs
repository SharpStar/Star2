using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace SharpStar.Packets.Serialization
{
    public class StarSerializableType<TOut> : SerializableType<IStarReader, IStarWriter, TOut>
    {
        public StarSerializableType(Action<IStarWriter, TOut> serializer, Func<IStarReader, TOut> deserializer) : base(serializer, deserializer)
        {
        }
    }
}
