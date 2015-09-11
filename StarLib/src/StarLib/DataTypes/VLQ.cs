using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.DataTypes
{
    /// <summary>
    /// A helper class for VLQs (Variable Length Quantities)
    /// </summary>
    public static class VLQ
    {
        /// <summary>
        /// Convert a VLQ to an array of bytes
        /// </summary>
        /// <param name="value">The VLQ</param>
        /// <returns>The data</returns>
        public static byte[] Create(ulong value)
        {
            var result = new Stack<byte>();

            if (value == 0)
                result.Push(0);

            while (value > 0)
            {
                byte tmp = (byte)(value & 0x7f);

                value >>= 7;

                if (result.Count > 0)
                    tmp |= 0x80;

                result.Push(tmp);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Convert a Signed VLQ to an array of bytes
        /// </summary>
        /// <param name="value">The signed VLQ</param>
        /// <returns>The data</returns>
        public static byte[] CreateSigned(long value)
        {
            long result = Math.Abs(value * 2);

            if (value < 0)
                result -= 1;

            return Create((ulong)result);
        }

        public static ulong FromFunc(Func<int, byte> read, Func<int, bool> condition, out int size, out bool success)
        {
            int ctr = 0;
            ulong value = 0L;
            while (condition(ctr))
            {
                byte tmp = read(ctr);

                value = (value << 7) | (uint)(tmp & 0x7f);

                if ((tmp & 0x80) == 0)
                {
                    size = ctr + 1;
                    success = true;
                    return value;
                }

                ctr++;
            }

            size = 0;
            success = false;

            return 0;
        }

        public static ulong FromEnumerable(IEnumerable<byte> buffer, int offset, int count, out int size, out bool success)
        {
            int ctr = 0;
            ulong value = 0L;
            foreach (byte b in buffer.Skip(offset).Take(count))
            {
                value = (value << 7) | (uint)(b & 0x7f);

                if ((b & 0x80) == 0)
                {
                    size = ctr + 1;
                    success = true;

                    return value;
                }

                ctr++;
            }

            size = 0;
            success = false;

            return 0;
        }

        public static long FromEnumerableSigned(IEnumerable<byte> buffer, int offset, int length, out int size, out bool success)
        {
            ulong value = FromEnumerable(buffer, offset, length, out size, out success);

            if (!success)
                return 0;

            if ((value & 1) == 0x00)
                return (long)value >> 1;

            return -((long)(value >> 1) + 1);
        }

        public static ulong FromBuffer(byte[] buffer, int offset, int length, out int size, out bool success)
        {
            ulong value = FromFunc(ctr => buffer[ctr + offset], ctr => ctr + offset < length, out size, out success);

            if (!success)
                return 0;

            size += offset;

            return value;
        }

        public static long FromBufferSigned(byte[] buffer, int offset, int length, out int size, out bool success)
        {
            ulong value = FromBuffer(buffer, offset, length, out size, out success);

            if (!success)
                return 0;

            if ((value & 1) == 0x00)
                return (long)value >> 1;

            return -((long)(value >> 1) + 1);
        }
    }
}
