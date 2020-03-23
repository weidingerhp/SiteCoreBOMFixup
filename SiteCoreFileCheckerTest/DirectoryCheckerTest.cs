using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SiteCoreFileChecker;
using SiteCoreFileChecker.Data;

namespace SiteCoreFileCheckerTest {
    public class DirectoryCheckerTest {
        private string _testDataFolder;

        [SetUp]
        public void SetUp() {
            _testDataFolder =
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestData"));

            Console.Out.WriteLine(_testDataFolder);
        }

        [Test]
        public async Task TestReadDataDirectory() {
            SiteCoreFileChecker.SiteCoreFileChecker checker = new SiteCoreFileChecker.SiteCoreFileChecker();
            var filesList = await checker.ListFiles(Path.Combine(_testDataFolder, "Simple"));
            Assert.AreEqual(6, filesList.Count, "Invalid Number of expected Files");
            foreach (var item in filesList) {
                Assert.AreEqual(BOM_TYPE.NO_BOM, item.Encoding);
                Assert.AreEqual(FileFlawType.NOT_CHECKED, item.FlawType);
            }
        }

        [Test]
        public async Task TestCheckBOMCorrectlyRecognized() {
            SiteCoreFileChecker.SiteCoreFileChecker checker = new SiteCoreFileChecker.SiteCoreFileChecker();
            var filesList = await checker.ListFiles(Path.Combine(_testDataFolder, "Simple"));
            Assert.AreEqual(6, filesList.Count, "Invalid Number of expected Files");
            filesList = await checker.CheckFiles();
            foreach (var item in filesList) {
                if (item.FileName.StartsWith("EBCDIC")) {
                    Assert.AreEqual(BOM_TYPE.UTF_EBCDIC, item.Encoding);
                    // no further checks with EBCDIC - Cannot read on this system
                    continue;
                }
                else {
                    if (item.FileName.EndsWith("With_BOM.xml")) {
                        Assert.AreNotEqual(BOM_TYPE.NO_BOM, item.Encoding);
                    }
                    else {
                        Assert.AreEqual(BOM_TYPE.NO_BOM, item.Encoding);
                    }
                }
                
                if (item.FileName.Equals("UTF-With_BOM.xml")) {
                    Assert.AreEqual(FileFlawType.BOM_FOUND, item.FlawType);
                }
                else {
                    Assert.AreEqual(FileFlawType.NO_FLAW, item.FlawType);
                }
            }
        }

        [Test]
        public async Task TestCheckXMLFlaws() {
            SiteCoreFileChecker.SiteCoreFileChecker checker = new SiteCoreFileChecker.SiteCoreFileChecker();
            var filesList = await checker.ListFiles(Path.Combine(_testDataFolder, "XMLFlaws"));
            Assert.AreEqual(3, filesList.Count, "Invalid Number of expected Files");
            filesList = await checker.CheckFiles();
            foreach (var item in filesList) {
                if (!item.FileName.StartsWith("No_")) {
                    Assert.AreEqual(FileFlawType.XML_INVALID, item.FlawType);
                    await Console.Out.WriteLineAsync($"{item.FileName}: {item.FlawMessage}");
                }
            }
        }
    }
}