using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using StarLib.Networking;
using System.Reflection;

namespace SharpStar.Packets.Serialization
{
    public class ListSerializer : StarSerializerBase
    {
        private static readonly Func<IStarReader, int> _collectionLengthReader = reader => (int)reader.ReadVLQ();
        private static readonly Action<IStarWriter, int> _collectionLengthWriter = (writer, i) => writer.WriteVlq((ulong)i);

        private static readonly Expression _collectionLengthReaderExpr = Expression.Constant(_collectionLengthReader);
        private static readonly Expression _collectionLengthWriterExpr = Expression.Constant(_collectionLengthWriter);

        private readonly StarSerializer _serializer;

        public ListSerializer(StarSerializer serializer)
        {
            _serializer = serializer;
        }

        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            if (CanRead(type))
            {
                Type listType = dest.Type.GetGenericArguments().First();

                var list = Expression.Variable(listType, "list");
                var length = Expression.Variable(typeof(int), "length");
                var exit = Expression.Label();
                var count = Expression.Property(dest, typeof(ICollection<>).MakeGenericType(listType), "Count");
                var typeReader = _serializer.BuildDeserializer(reader, listType, list);

                var block = Expression.Block(new[] { list, length },
                    Expression.Assign(dest, Expression.New(dest.Type)),
                    Expression.Assign(length, Expression.Invoke(_collectionLengthReaderExpr, reader)),
                    Expression.Loop(
                        Expression.IfThenElse(Expression.LessThan(count, length),
                            Expression.Call(dest, "Add", null, typeReader),
                            Expression.Break(exit)),
                        exit),
                    dest);

                return block;
            }

            return null;
        }

        public override Expression Write(Expression writer, Expression value)
        {
            if (CanWrite(value.Type))
            {
                var counter = Expression.Variable(typeof(int), "counter");
                var exit = Expression.Label();
                var listLength = Expression.Property(value, typeof(ICollection<>).MakeGenericType(value.Type.GetGenericArguments()), "Count");
                var currentItem = Expression.Property(value, "Item", counter);
                var block = Expression.Block(new[] { counter },
                    Expression.Invoke(_collectionLengthWriterExpr, writer, listLength),
                    Expression.Loop(
                        Expression.IfThenElse(Expression.LessThan(counter, listLength),
                            Expression.Block(
                                _serializer.BuildSerializer(writer, currentItem),
                                Expression.Assign(counter, Expression.Increment(counter))),
                            Expression.Break(exit)),
                        exit));
                return block;
            }

            return null;
        }

        public override bool CanRead(Type type)
        {
            return type.GetTypeInfo().IsGenericType && typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public override bool CanWrite(Type type)
        {
            return type.GetTypeInfo().IsGenericType && typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
