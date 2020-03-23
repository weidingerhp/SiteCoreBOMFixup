using System;
using System.Globalization;
using System.Windows.Data;
using SiteCoreFileChecker;

namespace SiteCoreFixup {
    public class BowNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            
            if (value is BOM_TYPE val) {
                switch (val) {
                    case BOM_TYPE.NO_BOM:
                        return "";
                    case BOM_TYPE.UTF_1:
                        return "UTF-1";
                    case BOM_TYPE.UTF_7A:
                    case BOM_TYPE.UTF_7B:
                        return "UTF-7";
                    case BOM_TYPE.UTF_8:
                        return "UTF-8";
                    case BOM_TYPE.UTF_16BE:
                        return "UTF-16 BE";
                    case BOM_TYPE.UTF_16LE:
                        return "UTF-16 LE";
                    case BOM_TYPE.UTF_32BE:
                        return "UTF-32 BE";
                    case BOM_TYPE.UTF_32LE:
                        return "UTF-32 LE";
                    case BOM_TYPE.UTF_EBCDIC:
                        return "EBCDIC";
                    default:
                        return "?";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
        
    }
}