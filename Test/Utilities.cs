using GrowthBook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tests {
    [TestClass]
    public class Utilities {
        public static string GetTestNames(MethodInfo methodInfo, object[] values) {
            return $"{methodInfo.Name} - { values[values.Length - 1] }";
        }

        public double RoundStandard(double input) {
            return Math.Round(input, 6);
        }

        public double[] RoundArray(double[] input) {
            double[] results = new double[input.Length];
            for (int i = 0; i < input.Length; i++) {
                results[i] = RoundStandard(input[i]);
            }
            return results;
        }


        public List<BucketRange> RoundBucketRanges(List<BucketRange> input) {
            List<BucketRange> results = new List<BucketRange>();
            foreach (BucketRange range in input) {
                results.Add(new BucketRange(RoundStandard(range.Start), RoundStandard(range.End)));
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
        public void GBhash_String_Returnsdouble(string input, double expected) {
            double actual = GrowthBook.Utilities.GbHash(input);
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
        public void InNamespace_Id_ReturnsMatch(string userId, string id, double start, double end, bool expected) {
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
        public void GetEqualWeights_ReturnsEqualWeights(int input, double[] expected) {
            double[] actual = RoundArray(GrowthBook.Utilities.GetEqualWeights(input));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DynamicData(nameof(GetBucketRangeTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void GetBucketRanges(int numVariations, double coverage, double[] weights, List<BucketRange> expected, string testName) {
            List<BucketRange> actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            CollectionAssert.AreEqual(RoundBucketRanges(expected), RoundBucketRanges(actual));
        }

        public static IEnumerable<object[]> GetBucketRangeTests() {
            List<BucketRange> test1 = new List<BucketRange> {
                    new BucketRange(0, 0.5),
                    new BucketRange(0.5, 1)
                };
            yield return new object[] { 2, 1, null, test1, "Normal 50/50 distribution" };

            List<BucketRange> test2 = new List<BucketRange> {
                new BucketRange(0, 0.25),
                new BucketRange(0.5, 0.75)
            };
            yield return new object[] { 2, 0.5, null, test2, "Reduced coverage" };

            List<BucketRange> test3 = new List<BucketRange> {
                new BucketRange( 0, 0),
                new BucketRange(0.5, 0.5)
            };
            yield return new object[] { 2, 0, null, test3, "Zero coverage" };

            List<BucketRange> test4 = new List<BucketRange> {
                new BucketRange(0, 0.25),
                new BucketRange(0.25, 0.5),
                new BucketRange(0.5, 0.75),
                new BucketRange(0.75, 1)
            };
            yield return new object[] { 4, 1, null, test4, "4 variations" };

            List<BucketRange> test5 = new List<BucketRange> {
                new BucketRange(0, 0.4),
                new BucketRange(0.4, 1)
            };
            yield return new object[] { 2, 1, new double[] { 0.4, 0.6 }, test5, "Uneven weights" };

            List<BucketRange> test6 = new List<BucketRange> {
                new BucketRange(0, 0.2),
                new BucketRange(0.2, 0.5),
                new BucketRange(0.5, 1)
            };
            yield return new object[] { 3, 1, new double[] { 0.2, 0.3, 0.5 }, test6, "Uneven weights, 3 variations" };

            List<BucketRange> test7 = new List<BucketRange> {
                new BucketRange(0, 0.04),
                new BucketRange(0.2, 0.26),
                new BucketRange(0.5, 0.6)
            };
            yield return new object[] { 3, 0.2, new double[] { 0.2, 0.3, 0.5 }, test7, "Uneven weights, reduced coverage, 3 variations" };

            List<BucketRange> test8 = new List<BucketRange> {
                new BucketRange(0, 0),
                new BucketRange(0.5, 0.5)
            };
            yield return new object[] { 2, -0.2, null, test8, "Negative coverage" };

            List<BucketRange> test9 = new List<BucketRange> {
                new BucketRange(0, 0.5),
                new BucketRange(0.5, 1)
            };
            yield return new object[] { 2, 1.5, null, test9, "Coverage above 1" };

            List<BucketRange> test10 = new List<BucketRange> {
                new BucketRange(0, 0.5),
                new BucketRange(0.5, 1),
            };
            yield return new object[] { 2, 1, new double[] { 0.4, 0.1 }, test10, "weights sum below 1" };

            List<BucketRange> test11 = new List<BucketRange> {
                new BucketRange(0, 0.5),
                new BucketRange(0.5, 1),
            };
            yield return new object[] { 2, 1, new double[] { 0.7, 0.6 }, test11, "weights sum above 1" };

            List<BucketRange> test12 = new List<BucketRange> {
                new BucketRange(0, 0.25),
                new BucketRange(0.25, 0.5),
                new BucketRange(0.5, 0.75),
                new BucketRange(0.75, 1),
            };
            yield return new object[] { 4, 1, new double[] { 0.4, 0.4, 0.2 }, test12, "weights.length not equal to num variations" };

            List<BucketRange> test13 = new List<BucketRange> {
                new BucketRange(0, 0.4),
                new BucketRange(0.4, 0.9999),
            };
            yield return new object[] { 2, 1, new double[] { 0.4, 0.5999 }, test13, "weights sum almost equals 1" };
        }
    }
}
