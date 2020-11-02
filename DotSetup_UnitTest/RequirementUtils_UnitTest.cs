using DotSetup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace DotSetup_UnitTest
{
    [TestClass]
    public class RequirementUtils_UnitTest
    {
        ConfigParser configParser;
        XmlDocument textDoc;

        [TestInitialize]
        public void TestInit()
        {
            textDoc = new XmlDocument();
            textDoc.Load(File.OpenRead(@"Resources\\empty_main.xml"));

            XmlDocument responseDoc = new XmlDocument();
            responseDoc.Load(File.OpenRead(@"Resources\\response_example.xml"));

            XmlDocument customDataDoc = new XmlDocument();
            customDataDoc.Load(File.OpenRead(@"Resources\\custom_data_example.xml"));

            configParser = new ConfigParser(textDoc);
            foreach (XmlNode productNode in responseDoc.SelectNodes("//RemoteConfiguration/Products/Product/StaticData"))
            {
                XmlParser.SetNode(responseDoc, productNode.SelectSingleNode("//CustomData"), customDataDoc.SelectSingleNode("//CustomData/CustomVars"));
                XmlParser.SetNode(textDoc, textDoc.SelectSingleNode("//Products/Product"), productNode);
                configParser.EvalCustomVariables(textDoc.SelectSingleNode("//Products/Product/StaticData/CustomData/CustomVars"));
            }
        }

        [TestMethod]
        public void TestProductCustomVariables()
        {
            foreach (XmlNode CustomVar in textDoc.SelectNodes("//Products/Product/CustomData/CustomVars"))
            {
                
                Assert.AreNotEqual(CustomVar.SelectSingleNode("//winVer").InnerText, "", "No custom variable winVer");
                Assert.AreEqual(CustomVar.SelectSingleNode("//winVerExists").InnerText, "true", "No custom variable winVerExists");
                Assert.AreNotEqual(CustomVar.SelectSingleNode("//totalRam").InnerText, "", "No custom variable totalRam");
                Assert.AreEqual(CustomVar.SelectSingleNode("//ChromeNotInstalled").InnerText, "false", "No custom variable ChromeNotInstalled");
                Assert.AreEqual(CustomVar.SelectSingleNode("//AreFilesExists").InnerText, "true", "No custom variable AreFilesExists");
                Assert.AreNotEqual(CustomVar.SelectSingleNode("//CheckConfigValue").InnerText, "", "No custom variable CheckConfigValue");
            }
        }
    }
}
