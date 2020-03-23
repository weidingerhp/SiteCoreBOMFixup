using System;
using System.Globalization;
using System.Windows.Data;
using SiteCoreFileChecker;

namespace SiteCoreFixup {
    public class BomImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            
            if (value is BOM_TYPE val) {
                switch (val) {
                    case BOM_TYPE.NO_BOM:
                        return "/Icons/encoding.png";
                    case BOM_TYPE.UTF_1:
                        return "/Icons/enc-utf-1.png";
                    case BOM_TYPE.UTF_7A:
                    case BOM_TYPE.UTF_7B:
                        return "/Icons/enc-utf-7.png";
                    case BOM_TYPE.UTF_8:
                        return "/Icons/enc-utf-8.png";
                    case BOM_TYPE.UTF_16BE:
                        return "/Icons/enc-utf-16BE.png";
                    case BOM_TYPE.UTF_16LE:
                        return "/Icons/enc-utf-16LE.png";
                    case BOM_TYPE.UTF_32BE:
                        return "/Icons/enc-utf-32BE.png";
                    case BOM_TYPE.UTF_32LE:
                        return "/Icons/enc-utf-32LE.png";
                    case BOM_TYPE.UTF_EBCDIC:
                        return "/Icons/enc-EBCDIC.png";
                    default:
                        return "/Icons/encoding.png";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
        
    }
}