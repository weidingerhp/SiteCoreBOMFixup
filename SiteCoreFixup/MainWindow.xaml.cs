using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using SiteCoreFileChecker;
using SiteCoreFileChecker.Data;

namespace SiteCoreFixup {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private SiteCoreFileChecker.SiteCoreFileChecker _fileChecker;
        List<FileItemEntry> lst = new List<FileItemEntry>();
        
        public MainWindow() {
            _fileChecker = new SiteCoreFileChecker.SiteCoreFileChecker();
            InitializeComponent();
            lst.Add(new FileItemEntry("hallo", FileFlawType.NO_FLAW));
            lstFiles.DataContext = _fileChecker;
            butConvert.DataContext = _fileChecker;
        }

        private void ChooseFolder(object sender, RoutedEventArgs e) {
            using (var dlg = new CommonOpenFileDialog()) {
                dlg.IsFolderPicker = true;
                var result = dlg.ShowDialog();
                if (result == CommonFileDialogResult.Ok) {
                    RefreshList(dlg.FileName);
                }

            }
        }

        private void RefreshList(string chosenFolder) {
            _fileChecker.ListFiles(chosenFolder).ContinueWith((task => { _fileChecker.CheckFiles().Start(); }));
            //_fileChecker.ResultList.Add(new FileCheck("hallo", FileFlawType.NO_FLAW));
        }

        private class PopupMessage {
            string _message;

            public string Message {
                get => _message;
                set => _message = value;
            }
        }
        
        private void OnListBoxItemSelect(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                string message = null;
                foreach (var oItem in e.AddedItems) {
                    if (oItem is FileItemEntry item) {
                        if (!string.IsNullOrEmpty(message)) message += "\n";
                        message = message + item.FlawMessage;
                    }
                }

                if (string.IsNullOrEmpty(message)) {
                    message = "No Errors";
                }
                
                Pops.DataContext = new PopupMessage {Message = message};
                Pops.PlacementTarget = sender as UIElement;
                Pops.IsOpen = true;
            }
            else {
                Pops.IsOpen = false;
            }
        }

        private async void OnConvertButtonClick(object sender, RoutedEventArgs e) {
            bool wasFixed = false;
            foreach (var entry in _fileChecker.ResultList) {
                if (entry.FlawType != FileFlawType.NO_FLAW && entry.FlawType != FileFlawType.NOT_CHECKED) {
                    if (await _fileChecker.CorrectFile(entry, false)) {
                        wasFixed = true;
                    }
                }
            }

            if (wasFixed) {
                await _fileChecker.CheckFiles();
            }
        }
    }
}