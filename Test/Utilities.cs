using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class Utilities
    {
        [TestMethod]
        [DataRow("a", 0.22f)]
        [DataRow("b", 0.077f)]
        [DataRow("ab", 0.946f)]
        [DataRow("def", 0.652f)]
        [DataRow("8952klfjas09ujkasdf", 0.549f)]
        [DataRow("123", 0.011f)]
        [DataRow("___)((*\":&", 0.563f)]
        public void GBhash_String_ReturnsFloat(string input, float expected)
        {
            float actual = GrowthBook.Utilities.GBhash(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("1", "namespace1", 0, 0.4f, false)]
        [DataRow("1", "namespace1", 0.4f, 1, true)]
        [DataRow("1", "namespace2", 0, 0.4f, false)]
        [DataRow("1", "namespace2", 0.4f, 1, true)]
        [DataRow("2", "namespace1", 0, 0.4f, false)]
        [DataRow("2", "namespace1", 0.4f, 1, true)]
        [DataRow("2", "namespace2", 0, 0.4f, false)]
        [DataRow("2", "namespace2", 0.4f, 1, true)]
        [DataRow("3", "namespace1", 0, 0.4f, false)]
        [DataRow("3", "namespace1", 0.4f, 1, true)]
        [DataRow("3", "namespace2", 0, 0.4f, true)]
        [DataRow("3", "namespace2", 0.4f, 1, false)]
        [DataRow("4", "namespace1", 0, 0.4f, false)]
        [DataRow("4", "namespace1", 0.4f, 1, true)]
        [DataRow("4", "namespace2", 0, 0.4f, true)]
        [DataRow("4", "namespace2", 0.4f, 1, false)]
        public void InNamespace_Id_ReturnsMatch(string userId, string id, float start, float end, bool expected)
        {
            bool actual = GrowthBook.Utilities.InNamespace(userId, new GrowthBook.Namespace(id, start, end));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(-1, new float[0])]
        [DataRow(0, new float[0])]
        [DataRow(1, new float[] { 1 })]
        [DataRow(2, new float[] { 0.5f, 0.5f })]
        [DataRow(3, new float[] { 0.33333333f, 0.33333333f, 0.33333333f })]
        [DataRow(4, new float[] { 0.25f, 0.25f, 0.25f, 0.25f })]
        public void GetEqualWeights_ReturnsEqualWeights(int input, float[] expected)
        {
            float[] actual = GrowthBook.Utilities.GetEqualWeights(input);
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
