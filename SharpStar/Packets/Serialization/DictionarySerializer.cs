using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using StarLib.Networking;

namespace SharpStar.Packets.Serialization
{
    public class DictionarySerializer : StarSerializerBase
    {
        private static readonly Func<IStarReader, int> _collectionLengthReader = reader => (int)reader.ReadVLQ();
        private static readonly Action<IStarWriter, int> _collectionLengthWriter = (writer, i) => writer.WriteVlq((ulong)i);

        private static readonly Expression _collectionLengthReaderExpr = Expression.Constant(_collectionLengthReader);
        private static readonly Expression _collectionLengthWriterExpr = Expression.Constant(_collectionLengthWriter);

        private readonly StarSerializer _serializer;

        public DictionarySerializer(StarSerializer serializer)
        {
            _serializer = serializer;
        }

        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            if (CanRead(type))
            {
                Type[] gTypes = dest.Type.GetGenericArguments();
                Type keyType = gTypes[0];
                Type valType = gTypes[1];

                var keyVar = Expression.Variable(gTypes[0]);
                var valVar = Expression.Variable(gTypes[1]);

                var length = Expression.Variable(typeof(int), "length");
                var dictAdd = dest.Type.GetMethod("Add", new[] { keyType, valType });

                var exit = Expression.Label();
                var block = Expression.Block(new[] { length, keyVar, valVar },
                    Expression.Assign(dest, Expression.New(dest.Type)),
                    Expression.Assign(length, Expression.Invoke(_collectionLengthReaderExpr, reader)),
                    Expression.Loop(
                        Expression.IfThenElse(Expression.LessThan(
                            Expression.Property(dest, "Count"), length),
                                Expression.Block(
                                    _serializer.BuildDeserializer(reader, gTypes[0], keyVar),
                                    _serializer.BuildDeserializer(reader, gTypes[1], valVar),
                                    Expression.Call(dest, dictAdd, keyVar, valVar)
                                ),
                            Expression.Break(exit)),
                        exit)
                );

                return block;
            }

            return null;
        }

        public override Expression Write(Expression writer, Expression value)
        {
            if (CanWrite(value.Type))
            {
                Type[] gTypes = value.Type.GetGenericArguments();

                var exit = Expression.Label();

                var count = value.Type.GetProperty("Count");
                var kvpType = typeof(KeyValuePair<,>).MakeGenericType(gTypes[0], gTypes[1]);
                var kvp = Expression.Variable(kvpType, "kvp");
                var key = kvpType.GetProperty("Key");
                var val = kvpType.GetProperty("Value");
                var enumerable = typeof(IEnumerable).GetMethod("GetEnumerator");
                var enumerator = typeof(IEnumerator);
                var next = enumerator.GetMethod("MoveNext");
                var current = enumerator.GetProperty("Current");

                var ie = Expression.Variable(enumerator, "ie");
                var block = Expression.Block(new[] { kvp, ie },
                    Expression.Assign(ie, Expression.Call(value, enumerable)),
                    Expression.Invoke(_collectionLengthWriterExpr, writer, Expression.Property(value, count)),
                    Expression.Loop(
                        Expression.IfThenElse(Expression.Equal(Expression.Call(ie, next), Expression.Constant(true)),
                            Expression.Block(
                                Expression.Assign(kvp, Expression.Convert(Expression.Property(ie, current), kvpType)),
                                _serializer.BuildSerializer(writer, Expression.Property(kvp, key)),
                                _serializer.BuildSerializer(writer, Expression.Property(kvp, val))),
                            Expression.Break(exit)),
                        exit));

                return block;
            }

            return null;
        }

        public override bool CanRead(Type type)
        {
            return type.GetTypeInfo().IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public override bool CanWrite(Type type)
        {
            return type.GetTypeInfo().IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
