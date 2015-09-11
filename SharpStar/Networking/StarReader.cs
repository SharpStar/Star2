using System;
using System.IO;
using System.Text;
using SharpStar.Packets.Serialization;
using SharpStar.Starbound;
using StarLib.DataTypes;
using StarLib.Networking;
using StarLib.Packets.Serialization;
using StarLib.Starbound;

namespace SharpStar.Networking
{
    /// <summary>
    /// A specialized BinaryReader that is able to read Starbound data types<para/>
    /// For writing, see <seealso cref="StarWriter"/>
    /// </summary>
    public class StarReader : BinaryReader, IStarReader
    {
        private static readonly IStarSerializer _serializer = new StarSerializer();

        public IStarSerializer Serializer => _serializer;

        public new MemoryStream BaseStream { get; set; }

        public StarReader(byte[] data) : base(new MemoryStream(data))
        {
            BaseStream = (MemoryStream)base.BaseStream;
        }

        public long DataLeft
        {
            get
            {
                return BaseStream.Length - BaseStream.Position;
            }
        }

        /// <summary>
        /// Read a VLQ-specified amount of bytes from the stream
        /// </summary>
        /// <returns>The data</returns>
        public byte[] ReadUInt8Array()
        {
            int length = (int)ReadVLQ();

            return ReadBytes(length);
        }

        public override uint ReadUInt32()
        {
            return (uint)(
               (ReadByte() << 24) |
               (ReadByte() << 16) |
               (ReadByte() << 8) |
                ReadByte());
        }

        public override long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public override ulong ReadUInt64()
        {
            return unchecked(
                ((ulong)ReadByte() << 56) |
                ((ulong)ReadByte() << 48) |
                ((ulong)ReadByte() << 40) |
                ((ulong)ReadByte() << 32) |
                ((ulong)ReadByte() << 24) |
                ((ulong)ReadByte() << 16) |
                ((ulong)ReadByte() << 8) |
                (ulong)ReadByte());
        }

        public byte[] ReadToEnd()
        {
            return ReadBytes((int)(BaseStream.Length - BaseStream.Position));
        }

        /// <summary>
        /// Reads a string with a VLQ-specified length from the stream
        /// </summary>
        /// <returns>The string</returns>
        public override string ReadString()
        {
            return Encoding.UTF8.GetString(ReadUInt8Array());
        }

        public ulong ReadVLQ()
        {
            int length;
            bool success;
            ulong result = ReadVLQ(out length, out success);

            if (!success)
                throw new Exception("Error reading VLQ!");

            return result;
        }

        /// <summary>
        /// Reads a VLQ from the stream
        /// </summary>
        /// <returns>A VLQ</returns>
        public ulong ReadVLQ(out int length, out bool success)
        {
            ulong result = VLQ.FromFunc(_ => ReadByte(), _ => true, out length, out success);

            if (!success)
                return 0;

            return result;
        }

        public long ReadSignedVLQ()
        {
            int length;
            bool success;
            long result = ReadSignedVLQ(out length, out success);

            if (!success)
                throw new Exception("Error reading sVLQ!");

            return result;
        }

        /// <summary>
        /// Reads a Signed VLQ from the stream
        /// </summary>
        /// <returns>A signed VLQ</returns>
        public long ReadSignedVLQ(out int length, out bool success)
        {
            ulong value = ReadVLQ(out length, out success);

            if ((value & 1) == 0x00)
                return (long)value >> 1;

            return -((long)(value >> 1) + 1);
        }

        public Vec2I ReadVec2I()
        {
            return new Vec2I
            {
                X = ReadInt32(),
                Y = ReadInt32()
            };
        }

        public Vec3I ReadVec3I()
        {
            return new Vec3I
            {
                X = ReadInt32(),
                Y = ReadInt32(),
                Z = ReadInt32()
            };
        }

        public IStarVariant ReadVariant()
        {
            var any = Serializer.Deserialize<Any<double, bool, IStarString, IStarVariantArray, IStarStringMap>>(this);

            return new StarVariant { Json = any };
        }
    }
}
