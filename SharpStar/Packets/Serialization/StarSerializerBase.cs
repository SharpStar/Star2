using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SharpStar.Packets.Serialization
{
    public abstract class StarSerializerBase
    {
        public abstract Expression Read(Expression reader, Type type, Expression dest);

        public abstract Expression Write(Expression writer, Expression value);

        public abstract bool CanRead(Type type);

        public abstract bool CanWrite(Type type);
    }
}
