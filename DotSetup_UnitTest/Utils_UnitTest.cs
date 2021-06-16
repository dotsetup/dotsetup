using DotSetup;
using DotSetup.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DotSetup_UnitTest
{
    [TestClass]
    public class Utils_UnitTest
    {
        [TestMethod]
        public void TestCryptUtils()
        {
            string x = "This is a test 123";
            string s = new StreamReader(CryptUtils.Encode(x, CryptUtils.EncDec.BASE62)).ReadToEnd();
            Assert.AreEqual(s, "HNtbuoHQ6XNtsq1ShOSiM8BZ", "Failed ToBase62");
            string x2 = new StreamReader(CryptUtils.Decode(s, CryptUtils.EncDec.BASE62)).ReadToEnd();
            Assert.AreEqual(x, x2, "Failed FromBase62");
        }
    }
}
