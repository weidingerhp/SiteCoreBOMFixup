using System;
using System.Globalization;
using System.Windows.Data;
using SiteCoreFileChecker.Data;

namespace SiteCoreFixup {
    public class ItemImageConverter : IValueConverter{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            
            if (value is FileFlawType val) {
                switch (val) {
                    case FileFlawType.NOT_CHECKED:
                        return "/Icons/folder.png";
                    case FileFlawType.NO_FLAW:
                        return "/Icons/ok.png";
                    default:
                        return "/Icons/critical.png";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}