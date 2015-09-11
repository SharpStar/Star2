using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Packets.Serialization
{
    public class CustomSerializer : StarSerializerBase
    {
        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            if (CanRead(type))
            {
                MethodInfo readFrom = typeof(IReadableWritable).GetMethod("ReadFrom", new[] { typeof(IStarReader) });

                MethodInfo resolveMethod = StarSerializer.ResolveMethod.MakeGenericMethod(type);

                var block = Expression.Block(
                    Expression.Assign(dest, Expression.Call(Expression.Call(StarSerializer.StarInstance), resolveMethod)),
                    Expression.Call(Expression.Convert(dest, typeof(IReadableWritable)), readFrom, reader)
                );

                return block;
            }

            return null;
        }

        public override Expression Write(Expression writer, Expression value)
        {
            if (CanWrite(value.Type))
            {
                MethodInfo writeTo = typeof(IReadableWritable).GetMethod("WriteTo", new[] { typeof(IStarWriter) });
                
                return Expression.Call(value, writeTo, writer);
            }

            return null;
        }

        public override bool CanRead(Type type)
        {
            return typeof(IReadableWritable).IsAssignableFrom(type);
        }

        public override bool CanWrite(Type type)
        {
            return typeof(IReadableWritable).IsAssignableFrom(type);
        }
    }
}
