using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets.Serialization;
using StarLib.Starbound;

namespace StarLib.Networking
{
    public interface IStarReader : IDisposable
    {
        IStarSerializer Serializer { get; }

        MemoryStream BaseStream { get; set; }

        byte ReadByte();

        byte[] ReadBytes(int count);

        byte[] ReadUInt8Array();

        uint ReadUInt32();

        int ReadInt32();

        long ReadInt64();

        ulong ReadUInt64();

        byte[] ReadToEnd();

        string ReadString();

        short ReadInt16();

        ushort ReadUInt16();

        bool ReadBoolean();

        float ReadSingle();

        double ReadDouble();

        ulong ReadVLQ();

        ulong ReadVLQ(out int length, out bool success);

        long ReadSignedVLQ();

        long ReadSignedVLQ(out int length, out bool success);

        Vec2I ReadVec2I();

        Vec3I ReadVec3I();

        IStarVariant ReadVariant();
    }
}
