using DotSetup;
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

            InstallationPackage pkg = new InstallationPackage("testPkg");
            PackageExtractor extractor = new PackageExtractor(pkg);
            extractor.Extract(zippedFilePath, zippedFileDir);

            foreach (string fileName in zippedContents)
                Assert.IsTrue(File.Exists(fileName), "No file named " + fileName);
        }

    }
}
