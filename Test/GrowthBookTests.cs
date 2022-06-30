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
                    testCase[0].ToString(),
                    testCase[1].ToObject<Context>(),
                    testCase[2].ToObject<Experiment>(),
                    testCase[3],
                    testCase[4].ToObject<bool>(),
                    testCase[5].ToObject<bool>(),
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(EvalFeatureTests), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestNames))]
        public void EvalFeature(string testname, Context context, string key, FeatureResult expected) {
            GrowthBook.GrowthBook gb = new GrowthBook.GrowthBook(context);
            FeatureResult actual = gb.EvalFeature(key);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<object[]> EvalFeatureTests() {
            foreach (JArray testCase in (JArray)testCases["feature"]) {
                yield return new object[] {
                    testCase[0].ToString(),
                    testCase[1].ToObject<Context>(),
                    testCase[2].ToString(),
                    testCase[3].ToObject<FeatureResult>(),
                };
            }
        }
    }
}
