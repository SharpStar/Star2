using System;

namespace SharpStar.Packets.Serialization
{
    public abstract class SerializableType
    {
        public Type Type { get; set; }

        public Delegate Serializer { get; set; }

        public Delegate Deserializer { get; set; }
    }

    public class SerializableType<TReader, TWriter, TOut> : SerializableType
    {

        public SerializableType(Action<TWriter, TOut> serializer, Func<TReader, TOut> deserializer)
        {
            Type = typeof(TOut);
            Serializer = serializer;
            Deserializer = deserializer;
        }

        public void Serialize(TWriter writer, TOut obj)
        {
            var s = (Action<TWriter, TOut>)Serializer;

            s(writer, obj);
        }

        public TOut Deserialize(TReader reader)
        {
            var d = (Func<TReader, TOut>)Deserializer;

            return d(reader);
        }

    }
}
