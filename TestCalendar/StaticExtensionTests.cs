using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicuCalendars.Utilities;

namespace TestCalendar
{
    [TestClass]
    public class StaticExtensionTests
    {
        [TestMethod]
        public void TestToSeperateWords()
        {
            Assert.AreEqual("Not Allowed", "NotAllowed".ToSeperateWords());
        }

        [TestMethod]
        public void TestNthField()
        {
            Assert.AreEqual("abc", "abc/defg/hij/klm".NthField(0,"/"));
            Assert.AreEqual("defg", "abc/defg/hij/klm".NthField(1, "/"));
            Assert.AreEqual("klm", "abc/defg/hij/klm".NthField(3, "/"));
            Assert.AreEqual(null, "abc/defg/hij/klm".NthField(4, "/"));
            Assert.AreEqual(string.Empty, "abc//hij/klm".NthField(1, "/"));
        }
    }
}
