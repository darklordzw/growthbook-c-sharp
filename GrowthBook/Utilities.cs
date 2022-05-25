using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook
{
    public static class Utilities
    {
        public static uint FNV32A(string str)
        {
            uint hval = 0x811C9DC5;
            uint prime = 0x01000193;
            byte[] buf = Encoding.Unicode.GetBytes(str);
            byte[] utf8Buf = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, buf);
            foreach (byte b in utf8Buf)
            {
                hval ^= b;
                hval *= prime;
                hval %= uint.MaxValue;
            }
            return hval;
        }

        public static double GBhash(string str)
        {
            uint n = FNV32A(str);
            return (n % 1000) / 1000.0;
        }
    }
}
