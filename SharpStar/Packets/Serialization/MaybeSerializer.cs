using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StarLib.Starbound;
using System.Reflection;

namespace SharpStar.Packets.Serialization
{
    public class MaybeSerializer : StarSerializerBase
    {
        private readonly StarSerializer _serializer;

        public MaybeSerializer(StarSerializer serializer)
        {
            _serializer = serializer;
        }

        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            if (CanRead(type))
            {
                var maybeType = type.GetGenericArguments().First();

                var hasAny = Expression.Variable(typeof(byte), "hasAny");
                var valueProp = type.GetProperty("Value");

                var newExpr = Expression.Block(new[] { hasAny },
                    Expression.Assign(dest, Expression.New(type)),
                    _serializer.BuildDeserializer(reader, typeof(byte), hasAny),
                    Expression.IfThen(Expression.Equal(hasAny, Expression.Constant((byte)1)),
                        _serializer.BuildDeserializer(reader, maybeType, Expression.Property(dest, valueProp))
                    ),
                    dest
                );

                return newExpr;
            }

            return null;
        }

        public override Expression Write(Expression writer, Expression value)
        {
            if (CanWrite(value.Type))
            {
                var maybeValueProp = value.Type.GetProperty("Value");
                var valueType = value.Type.GetGenericArguments().First();

                var block = Expression.Block(
                    Expression.IfThenElse(Expression.NotEqual(Expression.Property(value, maybeValueProp), Expression.Constant(null)),
                    Expression.Block(
                        _serializer.BuildSerializer(writer, Expression.Constant((byte)1)),
                        _serializer.BuildSerializer(writer, Expression.Property(value, maybeValueProp))
                        ),
                    _serializer.BuildSerializer(writer, Expression.Constant((byte)0))
                    )
                );

                return block;
            }

            return null;
        }

        public override bool CanRead(Type type)
        {
            return typeof(Maybe).IsAssignableFrom(type);
        }

        public override bool CanWrite(Type type)
        {
            return typeof(Maybe).IsAssignableFrom(type);
        }
    }
}
