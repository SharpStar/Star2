using System.IO;
using System.Text;
using SharpStar.Packets.Serialization;
using StarLib.DataTypes;
using StarLib.Networking;
using StarLib.Packets.Serialization;
using StarLib.Starbound;

namespace SharpStar.Networking
{
    /// <summary>
    /// A specialized BinaryWriter that is able to write Starbound data types<para/>
    /// For reading, use <seealso cref="StarReader"/>
    /// </summary>
    public class StarWriter : BinaryWriter, IStarWriter
    {
        private static readonly StarSerializer _serializer = new StarSerializer();

        public IStarSerializer Serializer => _serializer;

        public new MemoryStream BaseStream { get; set; }

        public StarWriter()
            : this(new MemoryStream())
        {
        }

        public StarWriter(MemoryStream stream) : base(stream)
        {
            BaseStream = stream;
        }

        public StarWriter(int initialLen) : this(new MemoryStream(initialLen))
        {
        }

        public void WriteUInt8Array(byte[] data)
        {
            WriteVlq((ulong)data.Length);
            Write(data);
        }

        /// <summary>
        /// Writes a string to the stream with a VLQ length
        /// </summary>
        /// <param name="str">The string to write</param>
        public override void Write(string str)
        {
            WriteUInt8Array(Encoding.UTF8.GetBytes(str));
        }

        public override void Write(ulong value)
        {
            WriteVlq(value);
        }

        public override void Write(int value)
        {
            Write((uint)value);
        }

        public void Write(Vec2I vec2)
        {
            Write(vec2.X);
            Write(vec2.Y);
        }

        public void Write(Vec3I vec3)
        {
            Write(vec3.X);
            Write(vec3.Y);
            Write(vec3.Z);
        }

        /// <summary>
        /// Write a VLQ to the stream
        /// </summary>
        /// <param name="vlq">The VLQ to write</param>
        public void WriteVlq(ulong vlq)
        {
            byte[] buffer = VLQ.Create(vlq);

            Write(buffer);
        }

        /// <summary>
        /// Write a Signed VLQ to the stream
        /// </summary>
        /// <param name="vlq">The VLQ to write</param>
        public void WriteSignedVLQ(long vlq)
        {
            byte[] buffer = VLQ.CreateSigned(vlq);

            Write(buffer);
        }

        public byte[] ToArray()
        {
            return BaseStream.ToArray();
        }

        public void WriteVariant(IStarVariant variant)
        {
            _serializer.Serialize(this, variant.Json);
        }
    }
}
