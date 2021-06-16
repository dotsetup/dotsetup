using DotSetup;
using DotSetup.Installation.Configuration;
using DotSetup.Installation.Packages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DotSetup_UnitTest
{
    [TestClass]
    public class Packages_UnitTest
    {
        [TestMethod]
        public void TestExtractor()
        {
            string zippedFilePath = Path.GetFullPath(@"Resources\\Halts.zip");
            string zippedFileDir = Path.GetDirectoryName(zippedFilePath);
            string[] zippedContents = { zippedFileDir + "\\Halts_with_0.exe", zippedFileDir + "\\Halts_with_1.exe" };
            foreach (string fileName in zippedContents)
                if (File.Exists(fileName))
                    File.Delete(fileName);
            ProductSettings settings = new ProductSettings { Name = "testPkg" };
            InstallationPackage pkg = new InstallationPackage(settings);
            PackageExtractor extractor = new PackageExtractor(pkg);
            extractor.Extract(zippedFilePath, zippedFileDir);

            foreach (string fileName in zippedContents)
                Assert.IsTrue(File.Exists(fileName), "No file named " + fileName);
        }

    }
}
