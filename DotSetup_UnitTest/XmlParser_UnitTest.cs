using DotSetup;
using DotSetup.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DotSetup_UnitTest
{
    [TestClass]
    public class XmlParser_UnitTest
    {
        private XmlDocument textDoc;

        [TestInitialize]
        public void TestInit()
        {
            textDoc = new XmlDocument();
            textDoc.Load(File.OpenRead(@"Resources\\XmlParser_Tests.xml"));
        }

        [TestMethod]
        public void TestGetXmlStringValue()
        {
            string res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a1/a1b1"));
            Assert.AreEqual(res, "str1", "GetStringValue check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a2"));
            Assert.AreEqual(res, "str1", "Flat XPath check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a3"));
            Assert.AreEqual(res, "", "Bad XPath check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a4"));
            Assert.AreEqual(res, "str2", "Recursive XPath check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a5"));
            Assert.AreEqual(res, "str3", "XPath backtracking check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a6"));
            Assert.AreEqual(res, "str4", "XPath default check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a7"));
            Assert.AreEqual(res, "", "Empty Known path failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a8"));
            Assert.AreEqual(res, KnownFolders.GetPath(KnownFolder.Documents), "Known path check failed");

            res = XmlParser.GetStringValue(textDoc.SelectSingleNode("//a9"));
            Assert.AreEqual(res, KnownFolders.GetPath(KnownFolder.DocumentsLibrary), "Using XPath in known path failed");
        }

        [TestMethod]
        public void TestGetXmlAttributeValue()
        {
            Dictionary<string, string> attributes = XmlParser.GetXpathRefAttributes(textDoc.SelectSingleNode("//a1/a1b2"));
            Assert.AreEqual(attributes["attr0"], "str10", "XPath Xml attribute Value failed");

            attributes = XmlParser.GetXpathRefAttributes(textDoc.SelectSingleNode("//a4"));
            Assert.AreEqual(attributes["attr1"], "str11", "XPath Xml attribute Value failed");

            attributes = XmlParser.GetXpathRefAttributes(textDoc.SelectSingleNode("//a10"));
            Assert.AreEqual(attributes["attr0"], "str12", "XPath Xml attribute double Value failed");
        }
    }
}
