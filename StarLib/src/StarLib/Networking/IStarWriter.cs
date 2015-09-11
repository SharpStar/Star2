using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets.Serialization;
using StarLib.Starbound;

namespace StarLib.Networking
{
    public interface IStarWriter : IDisposable
    {
        IStarSerializer Serializer { get; }

        MemoryStream BaseStream { get; set; }

        void WriteUInt8Array(byte[] data);

        void Write(string str);

        void Write(byte value);

        void Write(byte[] data);

        void Write(byte[] data, int offset, int count);

        void Write(ulong value);

        void Write(int value);

        void Write(short value);

        void Write(ushort value);

        void Write(float value);

        void Write(double value);

        void Write(bool value);

        void Write(Vec2I vec2);

        void Write(Vec3I vec3);

        void WriteVlq(ulong vlq);

        void WriteSignedVLQ(long vlq);

        byte[] ToArray();

        void WriteVariant(IStarVariant variant);
    }
}
