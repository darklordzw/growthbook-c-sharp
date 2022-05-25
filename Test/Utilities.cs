using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class Utilities
    {
        [TestMethod]
        [DataRow("a", 0.22)]
        [DataRow("b", 0.077)]
        [DataRow("ab", 0.946)]
        [DataRow("def", 0.652)]
        [DataRow("8952klfjas09ujkasdf", 0.549)]
        [DataRow("123", 0.011)]
        [DataRow("___)((*\":&", 0.563)]
        public void GBhash_String_ReturnsDouble(string input, double expected)
        {
            double actual = GrowthBook.Utilities.GBhash(input);
            Assert.AreEqual(expected, actual);
        }
    }
}
