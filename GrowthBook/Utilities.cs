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
            return (n % 1000) / 1000.0f;
        }

        public static bool InNamespace(string userId, Namespace nSpace)
        {
            double n = GBhash(userId + "__" + nSpace.Id);
            return n >= nSpace.Start && n < nSpace.End;
        }

        public static double[] GetEqualWeights(int numVariations) {
            if (numVariations < 1)
                return new double[0];

            double[] weights = new double[numVariations];
            for (int i = 0; i < numVariations; i++)
            {
                weights[i] = 1.0f / numVariations;
            }
            return weights;
        }

        public static double[][] GetBucketRanges(int numVariations, double coverage = 1, double[] weights = null)
        {
            if (coverage < 0)
                coverage = 0;
            if (coverage > 1)
                coverage = 1;
            if (weights == null)
                weights = GetEqualWeights(numVariations);
            if (weights.Length != numVariations)
                weights = GetEqualWeights(numVariations);

            double totalWeight = 0;
            foreach(double w in weights)
            {
                totalWeight += w;
            }
            if (totalWeight < 0.99 || totalWeight > 1.01f)
                weights = GetEqualWeights(numVariations);

            double cumulative = 0;
            double[][] ranges = new double[weights.Length][];
            for (int i = 0; i < weights.Length; i++)
            {
                double start = cumulative;
                cumulative += weights[i];
                ranges[i] = new double[] { start, start + coverage * weights[i] };
            }

            return ranges;
        }

        //def getBucketRanges(
        //    numVariations: int, coverage: double = 1, weights: "list[double]" = None
        //) -> "list[tuple[double,double]]":
        //    if coverage< 0:
        //        coverage = 0
        //    if coverage> 1:
        //        coverage = 1
        //    if weights is None:
        //        weights = getEqualWeights(numVariations)
        //    if len(weights) != numVariations:
        //        weights = getEqualWeights(numVariations)
        //    if sum(weights) < 0.99 or sum(weights) > 1.01:
        //        weights = getEqualWeights(numVariations)

        //    cumulative = 0
        //    ranges = []
        //    for w in weights:
        //        start = cumulative
        //        cumulative += w
        //        ranges.append((start, start + coverage* w))

        //    return ranges
    }
}
