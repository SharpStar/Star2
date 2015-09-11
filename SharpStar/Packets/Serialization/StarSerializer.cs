using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SharpStar.Networking;
using StarLib.Networking;
using System.Reflection;
using SharpStar.Starbound;
using StarLib;
using StarLib.Packets.Serialization;
using StarLib.Starbound;

namespace SharpStar.Packets.Serialization
{
    public class StarSerializer : StarSerializerBase, IStarSerializer
    {
        public static readonly List<SerializableType> PrimitiveSerializables = new List<SerializableType>
        {
            new StarSerializableType<int>((writer, val) => writer.Write(val), reader => reader.ReadInt32()),
            new StarSerializableType<uint>((writer, val) => writer.Write(val), reader => reader.ReadUInt32()),
            new StarSerializableType<short>((writer, val) => writer.Write(val), reader => reader.ReadInt16()),
            new StarSerializableType<ushort>((writer, val) => writer.Write(val), reader => reader.ReadUInt16()),
            new StarSerializableType<byte>((writer, val) => writer.Write(val), reader => reader.ReadByte()),
            new StarSerializableType<bool>((writer, val) => writer.Write(val), reader => reader.ReadBoolean()),
            new StarSerializableType<float>((writer, val) => writer.Write(val), reader => reader.ReadSingle()),
            new StarSerializableType<double>((writer, val) => writer.Write(val), reader => reader.ReadDouble()),
            new StarSerializableType<long>((writer, val) => writer.Write(val), reader => reader.ReadInt64()),
            new StarSerializableType<ulong>((writer, val) => writer.Write(val), reader => reader.ReadUInt64()),
            new StarSerializableType<string>((writer, val) => writer.Write(val), reader => reader.ReadString()),
            new StarSerializableType<byte[]>((writer, val) => writer.WriteUInt8Array(val), reader => reader.ReadUInt8Array()),
            new StarSerializableType<IStarString>((writer, val) => writer.Write(val.Value), reader => new StarString(reader.ReadString())),
            new StarSerializableType<IStarVariant>((writer, val) => writer.WriteVariant(val), reader => reader.ReadVariant())
        };

        public static readonly Dictionary<Type, SerializableType> Serializables = PrimitiveSerializables.ToDictionary(p => p.Type);

        public readonly List<StarSerializerBase> DefaultSerializers;

        private static readonly Dictionary<Type, Delegate> _serializers = new Dictionary<Type, Delegate>();
        private static readonly Dictionary<Type, Delegate> _deserializers = new Dictionary<Type, Delegate>();

        public static readonly MethodInfo ResolveMethod = typeof(Star).GetMethod("Resolve", Type.EmptyTypes);
        public static readonly MethodInfo StarInstance = typeof(Star).GetProperty("Instance").GetGetMethod();

        public StarSerializer()
        {
            DefaultSerializers = new List<StarSerializerBase>()
            {
                new DictionarySerializer(this),
                new ListSerializer(this),
                new AnySerializer(this),
                new MaybeSerializer(this),
                new CustomSerializer()
            };
        }

        public void Serialize<T>(IStarWriter writer, T value)
        {
            Action<IStarWriter, T> lambda = Serialize<T>();

            Type sType = typeof(T);

            if (!_serializers.ContainsKey(sType))
                _serializers[sType] = lambda;

            lambda(writer, value);
        }

        public Action<IStarWriter, T> Serialize<T>()
        {
            Type sType = typeof(T);

            if (_serializers.ContainsKey(sType))
                return (Action<IStarWriter, T>)_serializers[sType];

            ParameterExpression writerParam = Expression.Parameter(typeof(IStarWriter), "writer");
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");

            Expression build = BuildSerializer(writerParam, valueParam);

            var action = Expression.Lambda<Action<IStarWriter, T>>(build, writerParam, valueParam).Compile();

            return action;
        }

        public T Deserialize<T>(IStarReader reader)
        {
            Func<IStarReader, T> lambda = Deserialize<T>();

            Type dType = typeof(T);

            if (!_deserializers.ContainsKey(dType))
                _deserializers[dType] = lambda;

            return lambda(reader);
        }

        public Func<IStarReader, T> Deserialize<T>()
        {
            Type dType = typeof(T);

            if (_deserializers.ContainsKey(dType))
                return (Func<IStarReader, T>)_deserializers[dType];

            ParameterExpression readerParam = Expression.Parameter(typeof(IStarReader), "writer");
            ParameterExpression returnVar = Expression.Variable(dType);

            Expression build = Expression.Block(new[] { returnVar },
                BuildDeserializer(readerParam, typeof(T), returnVar),
                returnVar
            );

            var func = Expression.Lambda<Func<IStarReader, T>>(build, readerParam).Compile();

            return func;
        }

        public virtual Expression BuildSerializer(Expression writer, Expression instance)
        {
            return Write(writer, instance) ?? DefaultSerializers.Single(p => p.CanWrite(instance.Type))?.Write(writer, instance);
        }

        public virtual Expression BuildDeserializer(Expression reader, Type type, Expression dest)
        {
            if (!CanRead(type))
            {
                return DefaultSerializers.SingleOrDefault(p => p.CanRead(type))?.Read(reader, type, dest);
            }

            return Read(reader, type, dest);
        }

        public override Expression Write(Expression writer, Expression value)
        {
            SerializableType sType = null;
            if (Serializables.ContainsKey(value.Type))
                sType = Serializables[value.Type];

            if (sType != null)
            {
                return GetWriter(sType, writer, value);
            }

            return null;
        }

        public override Expression Read(Expression reader, Type type, Expression dest)
        {
            SerializableType sType = null;
            if (Serializables.ContainsKey(type))
                sType = Serializables[type];

            if (sType != null)
            {
                return GetReader(sType, reader, dest);
            }

            return null;
        }

        protected Expression GetReader(SerializableType sType, Expression reader, Expression dest)
        {
            Type valType = sType.GetType().GetGenericArguments().First();
            Type sFuncType = typeof(Func<,>).MakeGenericType(typeof(IStarReader), valType);

            Expression toFunc = Expression.Convert(Expression.Constant(sType.Deserializer), sFuncType);
            Expression read = Expression.Invoke(toFunc, reader);

            return Expression.Assign(dest, valType == dest.Type ? read : Expression.Convert(read, dest.Type));
        }

        protected Expression GetWriter(SerializableType sType, Expression writer, Expression value)
        {
            Type valType = sType.GetType().GetGenericArguments().First();
            Type sFuncType = typeof(Action<,>).MakeGenericType(typeof(IStarWriter), valType);

            var toFunc = Expression.Convert(Expression.Constant(sType.Serializer), sFuncType);
            var write = Expression.Invoke(toFunc, writer, Expression.Convert(value, valType));

            return write;
        }

        public override bool CanRead(Type type)
        {
            return Serializables.ContainsKey(type);
        }

        public override bool CanWrite(Type type)
        {
            return Serializables.ContainsKey(type);
        }
    }
}
