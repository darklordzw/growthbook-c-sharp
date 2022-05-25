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

        public static float GBhash(string str)
        {
            uint n = FNV32A(str);
            return (n % 1000) / 1000.0f;
        }

        public static bool InNamespace(string userId, Namespace nSpace)
        {
            float n = GBhash(userId + "__" + nSpace.Id);
            return n >= nSpace.Start && n < nSpace.End;
        }

        public static float[] GetEqualWeights(int numVariations) {
            if (numVariations < 1)
                return new float[0];

            float[] weights = new float[numVariations];
            for (int i = 0; i < numVariations; i++)
            {
                weights[i] = 1.0f / numVariations;
            }
            return weights;
        }
    }
}
