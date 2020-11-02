using DotSetup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace DotSetup_UnitTest
{
    [TestClass]
    public class Configuration_UnitTest
    {
        [TestMethod]
        public void TestConfigurationIsValid()
        {
            XmlDocument textDoc = new XmlDocument();
            textDoc.Load(File.OpenRead(@"Resources\\empty_main.xml"));
            ConfigParser configParser = new ConfigParser(textDoc);

            ConfigValidator configValidator = new ConfigValidator();
            Assert.AreNotEqual(configValidator.Validate(), "", "Config should not be valid");

            foreach (string mandatoryConst in ConfigConsts.ReportMandatory)
                configParser.SetStringValue("//Config/" + mandatoryConst, "StringValue");

            Assert.AreEqual(configValidator.Validate(), "", "Config should be valid");

            configParser.SetStringValue("//Config/" + ConfigConsts.URL_ANALYTICS, "");
            Assert.AreNotEqual(configValidator.Validate(), "", "Config should not be valid");
        }

    }
}
