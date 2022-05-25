using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class Utilities
    {
        public double[] RoundArray(double[] input)
        {
            double[] results = new double[input.Length];
            for(int i = 0; i < input.Length; i++)
            {
                results[i] = Math.Round(input[i], 6);
            }
            return results;
        }


        public double[][] RoundArray2d(double[][] input)
        {
            double[][] results = new double[input.Length][];
            for (int i = 0; i < input.Length; i++)
            {
                results[i] = RoundArray(input[i]);
            }
            return results;
        }

        [TestMethod]
        [DataRow("a", 0.22)]
        [DataRow("b", 0.077)]
        [DataRow("ab", 0.946)]
        [DataRow("def", 0.652)]
        [DataRow("8952klfjas09ujkasdf", 0.549)]
        [DataRow("123", 0.011)]
        [DataRow("___)((*\":&", 0.563)]
        public void GBhash_String_Returnsdouble(string input, double expected)
        {
            double actual = GrowthBook.Utilities.GBhash(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("1", "namespace1", 0, 0.4, false)]
        [DataRow("1", "namespace1", 0.4, 1, true)]
        [DataRow("1", "namespace2", 0, 0.4, false)]
        [DataRow("1", "namespace2", 0.4, 1, true)]
        [DataRow("2", "namespace1", 0, 0.4, false)]
        [DataRow("2", "namespace1", 0.4, 1, true)]
        [DataRow("2", "namespace2", 0, 0.4, false)]
        [DataRow("2", "namespace2", 0.4, 1, true)]
        [DataRow("3", "namespace1", 0, 0.4, false)]
        [DataRow("3", "namespace1", 0.4, 1, true)]
        [DataRow("3", "namespace2", 0, 0.4, true)]
        [DataRow("3", "namespace2", 0.4, 1, false)]
        [DataRow("4", "namespace1", 0, 0.4, false)]
        [DataRow("4", "namespace1", 0.4, 1, true)]
        [DataRow("4", "namespace2", 0, 0.4, true)]
        [DataRow("4", "namespace2", 0.4, 1, false)]
        public void InNamespace_Id_ReturnsMatch(string userId, string id, double start, double end, bool expected)
        {
            bool actual = GrowthBook.Utilities.InNamespace(userId, new GrowthBook.Namespace(id, start, end));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(-1, new double[0])]
        [DataRow(0, new double[0])]
        [DataRow(1, new double[] { 1 })]
        [DataRow(2, new double[] { 0.5, 0.5 })]
        [DataRow(3, new double[] { 0.333333, 0.333333, 0.333333 })]
        [DataRow(4, new double[] { 0.25, 0.25, 0.25, 0.25 })]
        public void GetEqualWeights_ReturnsEqualWeights(int input, double[] expected)
        {
            double[] actual = RoundArray(GrowthBook.Utilities.GetEqualWeights(input));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(2, 1, null)]
        public void GetBucketRanges_Normal_50_50(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            double[][] expected = { new double[] { 0, 0.5 }, new double[] { 0.5, 1 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [DataRow(2, 0.5, null)]
        public void GetBucketRanges_ReducedCoverage(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            double[][] expected = { new double[] { 0, 0.25 }, new double[] { 0.5, 0.75 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [DataRow(2, 0, null)]
        public void GetBucketRanges_ZeroCoverage(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            double[][] expected = { new double[] { 0, 0 }, new double[] { 0.5, 0.5 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [DataRow(4, 1, null)]
        public void GetBucketRanges_FourVariations(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            double[][] expected = { new double[] { 0, 0.25 }, new double[] { 0.25, 0.5 }, new double[] { 0.5, 0.75 }, new double[] { 0.75, 1 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [DataRow(2, 1, new double[] { 0.4, 0.6 })]
        public void GetBucketRanges_UnevenWeights(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            double[][] expected = { new double[] { 0, 0.4 }, new double[] { 0.4, 1 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [DataRow(3, 0.2, new double[] { 0.2, 0.3, 0.5 })]
        public void GetBucketRanges_UnevenWeightsReducedCoverageThreeVariations(int numVariations, double coverage, double[] weights)
        {
            double[][] actual = RoundArray2d(GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights));
            double[][] expected = { new double[] { 0, 0.04 }, new double[] { 0.2, 0.26 }, new double[] { 0.5, 0.6 } };
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        
        //[
        //  "uneven weights, reduced coverage, 3 variations",
        //  [3, 0.2, [0.2, 0.3, 0.5]],
        //  [
        //    [0, 0.04],
        //    [0.2, 0.26],
        //    [0.5, 0.6]
        //  ]
        //],
        //[
        //  "negative coverage",
        //  [2, -0.2, null],
        //  [
        //    [0, 0],
        //    [0.5, 0.5]
        //  ]
        //],
        //[
        //  "coverage above 1",
        //  [2, 1.5, null],
        //  [
        //    [0, 0.5],
        //    [0.5, 1]
        //  ]
        //],
        //[
        //  "weights sum below 1",
        //  [2, 1, [0.4, 0.1]],
        //  [
        //    [0, 0.5],
        //    [0.5, 1]
        //  ]
        //],
        //[
        //  "weights sum above 1",
        //  [2, 1, [0.7, 0.6]],
        //  [
        //    [0, 0.5],
        //    [0.5, 1]
        //  ]
        //],
        //[
        //  "weights.length not equal to num variations",
        //  [4, 1, [0.4, 0.4, 0.2]],
        //  [
        //    [0, 0.25],
        //    [0.25, 0.5],
        //    [0.5, 0.75],
        //    [0.75, 1]
        //  ]
        //],
        //[
        //  "weights sum almost equals 1",
        //  [2, 1, [0.4, 0.5999]],
        //  [
        //    [0, 0.4],
        //    [0.4, 0.9999]
        //  ]
        //]
    }
}
