using GrowthBook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tests {
    [TestClass]
    public class GrowthBookTests {
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

        public IList<double> RoundArray(IList<double> input) {
            List<double> results = new List<double>();
            for (int i = 0; i < input.Count; i++) {
                results.Add(RoundStandard(input[i]));
            }
            return results;
        }

        public IList<BucketRange> RoundBucketRanges(IList<BucketRange> input) {
            List<BucketRange> results = new List<BucketRange>();
            foreach (BucketRange range in input) {
                results.Add(new BucketRange(RoundStandard(range.Start), RoundStandard(range.End)));
            }
            return results;
        }

        [TestMethod]
        [DynamicData(nameof(RunTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void Run(string testname, Context context, Experiment experiment, JToken expectedValue, bool inExperiment, bool hashUsed) {
            GrowthBook.GrowthBook gb = new GrowthBook.GrowthBook(context);
            ExperimentResult actual = gb.Run(experiment);
            Assert.AreEqual(inExperiment, actual.InExperiment);
            Assert.AreEqual(hashUsed, actual.HashUsed);
            Assert.IsTrue(JToken.DeepEquals(actual.Value, expectedValue));
        }

        public static IEnumerable<object[]> RunTests() {
            foreach (JArray testCase in (JArray)testCases["run"]) {
                yield return new object[] {
                    testCase[0].ToObject<string>(),
                    testCase[1].ToObject<Context>(),
                    testCase[2].ToObject<Experiment>(),
                    testCase[3],
                    testCase[4].ToObject<bool>(),
                    testCase[5].ToObject<bool>(),
                };
            }
        }
    }
}
