using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SiteCoreFileChecker.Data {
    public enum FileFlawType {
        NOT_CHECKED,
        NO_FLAW,
        BOM_FOUND,
        XML_INVALID
        
    }

    public class FileItemEntry : INotifyPropertyChanged {
        private readonly string _fileName;
        private FileFlawType _flawType;
        private BOM_TYPE _encoding;
        private string _flawMessage;

        public FileItemEntry(string fileName, FileFlawType flawType) {
            _fileName = fileName;
            _flawType = flawType;
            _encoding = BOM_TYPE.NO_BOM;
        }

        public string FileName => _fileName;
        public FileFlawType FlawType {
            get => _flawType;
            set {
                _flawType = value;
                OnPropertyChanged(nameof(FlawType));
            }
        }

        public BOM_TYPE Encoding {
            get => _encoding;
            set {
                _encoding = value;
                OnPropertyChanged(nameof(Encoding));
            }
        }

        public string FlawMessage {
            get => _flawMessage;
            set {
                _flawMessage = value;
                OnPropertyChanged(nameof(FlawMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((string) propertyName));
        }
    }
}