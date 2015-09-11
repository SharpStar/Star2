using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StarLib.Networking;
using System.Reflection;
using SharpStar.Starbound;
using StarLib.Starbound;

namespace SharpStar.Packets.Serialization
{
    public class AnySerializer : StarSerializerBase
    {
        private readonly StarSerializer _serializer;

        public AnySerializer(StarSerializer serializer)
        {
            _serializer = serializer;
        }

        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            if (CanRead(dest.Type))
            {
                Type[] gTypes = dest.Type.GetGenericArguments();

                var idx = Expression.Variable(typeof(byte), "idx");
                //var index = Expression.Property(dest, "Index");
                var value = Expression.Property(dest, "Value");
                
                var exprs = new List<Expression>();
                for (int i = 0; i < gTypes.Length; i++)
                {
                    exprs.Add(Expression.Block(
                        Expression.IfThen(Expression.Equal(idx, Expression.Constant((byte)(i + 1))),
                        Expression.Block(
                            Expression.Assign(dest, Expression.New(type)),
                            _serializer.BuildDeserializer(reader, gTypes[i], value)
                        )
                        )
                    ));

                }

                var block = Expression.Block(new[] { idx },
                    _serializer.BuildDeserializer(reader, typeof(byte), idx),
                    Expression.IfThen(Expression.GreaterThan(idx, Expression.Constant((byte)0)),
                    Expression.Block(exprs)
                    )
                );

                return block;
            }

            return null;
        }

        public override Expression Write(Expression writer, Expression value)
        {
            var anyIndexProp = value.Type.GetProperty("Index");
            var anyValueProp = value.Type.GetProperty("Value");

            var anyIndex = Expression.Property(value, anyIndexProp);
            var anyVal = Expression.Property(value, anyValueProp);

            Type[] gTypes = value.Type.GetGenericArguments();

            var exprs = new List<Expression>();
            for (int i = 0; i < gTypes.Length; i++)
            {
                exprs.Add(Expression.Block(
                    Expression.IfThen(Expression.Equal(anyIndex, Expression.Constant((byte)(i + 1))),
                    Expression.IfThenElse(Expression.NotEqual(anyVal, Expression.Constant(null)),
                        Expression.Block(
                            _serializer.BuildSerializer(writer, anyIndex),
                            _serializer.BuildSerializer(writer, Expression.Convert(anyVal, gTypes[i]))
                        ),
                        _serializer.BuildSerializer(writer, Expression.Constant((byte)0))
                    ))
                ));
            }

            return Expression.Block(exprs);
        }

        public override bool CanRead(Type type)
        {
            return typeof(Any).IsAssignableFrom(type);
        }

        public override bool CanWrite(Type type)
        {
            return typeof(Any).IsAssignableFrom(type);
        }
    }
}
