using WebPad.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace WebPad.Tests.Utilities
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void AsTemplated_Should_ReturnOriginalString_Given_AStringWithNoReplacements()
        {
            Assert.AreEqual("My String", "My String".AsTemplated(new {}));
        }

        [TestMethod]
        public void AsTemplated_Should_ReplaceSingleParameter_Given_AStringWithOneTemplate()
        {
            Assert.AreEqual("My own String", "My {{testing}} String".AsTemplated(new { testing = "own" }));
        }

        [TestMethod]
        public void AsTemplated_Should_ReplaceTwoParameters_Given_AStringWithTwoTemplates()
        {
            Assert.AreEqual("My very own String", "My {{anotherTest}} {{testing}} String".AsTemplated(new { anotherTest = "very", testing = "own" }));
        }

        [TestMethod]
        public void AsTemplated_Should_ReplaceADuplicatedParameter_Given_AStringWithTwoTemplates()
        {
            Assert.AreEqual("My own own String", "My {{testing}} {{testing}} String".AsTemplated(new { testing = "own" }));
        }
    }
}
// ReSharper restore InconsistentNaming
