using DotSetup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

            CheckSetConfig(configParser, "abc", "123");
            CheckSetConfig(configParser, "Url", @"https://uri.com.br/api/v1/installer?userId=0&programId=49730&status=init");

            configParser.SetStringValue("//Config/" + ConfigConsts.URL_ANALYTICS, "");
            Assert.AreNotEqual(configValidator.Validate(), "", "Config should not be valid");
        }

        private void CheckSetConfig(ConfigParser configParser, string key, string value)
        {
            configParser.SetConfigValue(key, value);
            Assert.AreEqual(configParser.GetConfigValue(key), value, "Config not set correctly");
        }
    }
}
