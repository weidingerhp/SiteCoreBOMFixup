using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SiteCoreFileChecker.Data;

namespace SiteCoreFileChecker {
    public class FilesList : AsyncObservableCollection<FileItemEntry> {
    }


    public class SiteCoreFileChecker {
        public delegate void FileAnalyzedEventHandler(SiteCoreFileChecker source, FileItemEntry item, long itemNo,
            long allItems);

        public delegate void ItemListWorkFinishedEventHandler(SiteCoreFileChecker source, FilesList result);


        public event FileAnalyzedEventHandler FileAnalyzed;
        public event ItemListWorkFinishedEventHandler ListFinished;
        public event ItemListWorkFinishedEventHandler AnalyzeFinished;

        public string BaseDirectory { get; private set; }
        public FilesList ResultList { get; private set; }

        public SiteCoreFileChecker() {
            BaseDirectory = null;
            ResultList = new FilesList();
        }

        public async Task<FilesList> ListFiles(string baseDirectory) {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));

            ResultList.Clear();
            BaseDirectory = baseDirectory;
            var source = this;

            return await Task.Run(() => {
                FillList(BaseDirectory, "");
                ListFinished?.Invoke(source, ResultList);
                return ResultList;
            });
        }

        public async Task<bool> CorrectFile(FileItemEntry entry, bool createBackup) {
            bool wasFixed = false;
            switch (entry.FlawType) {
                case FileFlawType.NO_FLAW:
                case FileFlawType.NOT_CHECKED:
                    // cant do anything here
                    break;
                case FileFlawType.XML_INVALID:
                    // XML-Correction not supported
                    break;
                case FileFlawType.BOM_FOUND:
                    // remove the BOM
                    string fullFileName = Path.Combine(BaseDirectory, entry.FileName);
                    string bakFileName = fullFileName + ".bak";
                    if (File.Exists(bakFileName)) {
                        int i;
                        for (i = 1; File.Exists(bakFileName + i); i++) {
                        }

                        bakFileName += i;
                    }
                    File.Copy(fullFileName, bakFileName);
                    wasFixed = await CharacterCheck.RemoveBOM(fullFileName);
                    break;
            }

            return wasFixed;
        }

        public async Task<FilesList> CheckFiles() {
            if (ResultList == null) throw new InvalidOperationException("You need to call ListFiles first");

            var source = this;
            long allItems = ResultList.Count;
            long currentItem = 0;

            return await Task.Run(async () => {
                foreach (var item in ResultList) {
                    string fileName = BaseDirectory + "/" + item.FileName;
                    var failIndices = await CharacterCheck.CheckForBOM(fileName);
                    if (failIndices.Count > 0) {
                        if (failIndices.Any(index => index.Type == BOM_TYPE.UTF_8)) {
                            item.FlawType = FileFlawType.BOM_FOUND;
                            item.FlawMessage = "Suspicious BOM Found";
                        }
                        else {
                            item.FlawType = FileFlawType.NO_FLAW;
                        }

                        item.Encoding = failIndices[0].Type;
                    }
                    else {
                        item.FlawType = FileFlawType.NO_FLAW;
                        item.Encoding = BOM_TYPE.NO_BOM;
                    }

                    string xmlFlaw = await XMLCheck.CheckXML(fileName, item.Encoding.ToEncoding()); 
                    if (xmlFlaw !=null) {
                        if (!string.IsNullOrEmpty(item.FlawMessage))
                            item.FlawMessage += $"; {xmlFlaw}";
                        else
                            item.FlawMessage = xmlFlaw;
                        
                        if (item.FlawType == FileFlawType.NO_FLAW ||
                            item.FlawType == FileFlawType.NOT_CHECKED) {
                            item.FlawType = FileFlawType.XML_INVALID;
                        } 
                    }

                    FileAnalyzed?.Invoke(source, item, currentItem, allItems);
                    currentItem++;
                }

                AnalyzeFinished?.Invoke(source, ResultList);
                return ResultList;
            });
        }

        private void FillList(string folder, string pathprefix) {
            foreach (var file in Directory.GetFiles(folder, "*.xml")) {
                ResultList.Add(new FileItemEntry(pathprefix + Path.GetFileName(file), FileFlawType.NOT_CHECKED));
            }

            foreach (var subdir in Directory.EnumerateDirectories(folder, "*.*")) {
                FillList(subdir, pathprefix + Path.GetFileName(subdir) + "/");
            }
        }
    }
}