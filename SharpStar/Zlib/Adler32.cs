public static class Adler
{

    public const int Base = 65521;
    public const int Max = 5552;

    public static uint Adler32(uint adler, byte[] buf, int index, int len)
    {
        if (buf == null)
            return 1;

        uint s1 = (uint)(adler & 0xffff);
        uint s2 = (uint)((adler >> 16) & 0xffff);

        while (len > 0)
        {
            int k = len < Max ? len : Max;
            len -= k;
            while (k >= 16)
            {
                //s1 += (buf[index++] & 0xff); s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                s1 += buf[index++]; s2 += s1;
                k -= 16;
            }
            if (k != 0)
            {
                do
                {
                    s1 += buf[index++];
                    s2 += s1;
                }
                while (--k != 0);
            }
            s1 %= Base;
            s2 %= Base;
        }
        return (uint)((s2 << 16) | s1);
    }
}