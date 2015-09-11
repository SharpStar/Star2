using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SharpStar.Zlib
{
    public class ZlibUtils
    {
        private static readonly byte[] Header = { 0x78, 0x9C };

        public static byte[] Compress(byte[] buffer)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                outStream.Write(Header, 0, Header.Length);

                using (DeflateStream ds = new DeflateStream(outStream, CompressionLevel.Optimal, true))
                {
                    ds.Write(buffer, 0, buffer.Length);
                }

                byte[] checksum = GetChecksum(buffer, 0, buffer.Length);

                outStream.Write(checksum, 0, checksum.Length);

                return outStream.ToArray();
            }

        }

        public static byte[] GetChecksum(byte[] buffer, int offset, int count)
        {
            uint value = Adler.Adler32(1, buffer, offset, count);

            byte[] checksum = new byte[4];
            checksum[0] = (byte)((value & 0xFF000000) >> 24);
            checksum[1] = (byte)((value & 0x00FF0000) >> 16);
            checksum[2] = (byte)((value & 0x0000FF00) >> 8);
            checksum[3] = (byte)(value & 0x000000FF);

            return checksum;
        }
    }
}
