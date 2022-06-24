using GrowthBook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Tests {
    [TestClass]
    public class Utilities {
        public static JObject testCases;

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context) {
            testCases = JObject.Parse(File.ReadAllText("../../cases.json"));
        }

        public static string GetTestNames(MethodInfo methodInfo, object[] values) {
            return $"{methodInfo.Name} - { values[0] }";
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
        [DynamicData(nameof(HashTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void Hash(string input, double expected) {
            double actual = GrowthBook.Utilities.Hash(input);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> HashTests() {
            foreach (JArray testCase in (JArray)testCases["hash"]) {
                yield return new object[] {
                    testCase[0].ToObject<string>(),
                    testCase[1].ToObject<double>()
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(InNamespaceTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void InNamespace(string testName, string userId, string id, double start, double end, bool expected) {
            bool actual = GrowthBook.Utilities.InNamespace(userId, new GrowthBook.Namespace(id, start, end));
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> InNamespaceTests() {
            foreach (JArray testCase in (JArray)testCases["inNamespace"]) {
                yield return new object[] {
                    testCase[0].ToObject<string>(),
                    testCase[1].ToObject<string>(),
                    testCase[2][0].ToObject<string>(),
                    testCase[2][1].ToObject<double>(),
                    testCase[2][2].ToObject<double>(),
                    testCase[3].ToObject<bool>(),
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetEqualWeightsTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void GetEqualWeights(int input, double[] expected) {
            double[] actual = GrowthBook.Utilities.GetEqualWeights(input);
            CollectionAssert.AreEqual(RoundArray(expected), RoundArray(actual));
        }

        public static IEnumerable<object[]> GetEqualWeightsTests() {
            foreach (JArray testCase in (JArray)testCases["getEqualWeights"]) {
                yield return new object[] {
                    testCase[0].ToObject<int>(),
                    testCase[1].ToObject<double[]>(),
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetBucketRangeTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void GetBucketRanges(string testName, int numVariations, double coverage, double[] weights, List<BucketRange> expected) {
            List<BucketRange> actual = GrowthBook.Utilities.GetBucketRanges(numVariations, coverage, weights);
            CollectionAssert.AreEqual(RoundBucketRanges(expected), RoundBucketRanges(actual));
        }

        public static IEnumerable<object[]> GetBucketRangeTests() {
            foreach (JArray testCase in (JArray)testCases["getBucketRange"]) {
                List<BucketRange> expected = new List<BucketRange>();
                foreach (JArray jArray in testCase[2]) {
                    expected.Add(new BucketRange(jArray[0].ToObject<double>(), jArray[1].ToObject<double>()));
                }
                yield return new object[] {
                    testCase[0].ToObject<string>(),
                    testCase[1][0].ToObject<int>(),
                    testCase[1][1].ToObject<double>(),
                    testCase[1][2].ToObject<double[]>(),
                    expected,
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(ChooseVariationTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void ChooseVariation(string testName, double n, List<BucketRange> ranges, int expected) {
            int actual = GrowthBook.Utilities.ChooseVariation(n, ranges);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> ChooseVariationTests() {
            foreach (JArray testCase in (JArray)testCases["chooseVariation"]) {
                List<BucketRange> ranges = new List<BucketRange>();
                foreach (JArray jArray in testCase[2]) {
                    ranges.Add(new BucketRange(jArray[0].ToObject<double>(), jArray[1].ToObject<double>()));
                }
                yield return new object[] {
                    testCase[0].ToObject<string>(),
                    testCase[1].ToObject<double>(),
                    ranges,
                    testCase[3].ToObject<int>(),
                };
            }
        }
    }
}
