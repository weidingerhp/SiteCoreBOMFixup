using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SiteCoreFileChecker {
    public class XMLCheck {
        public static async Task<string> CheckXML(string fileName, Encoding encoding) {
            try {
                var settings = new XmlReaderSettings();
                settings.Async = true;
                
                // Using a parser context create problems when having BOM for Files - thats why we use it.
                var readerContext = new XmlParserContext(null, null, null, XmlSpace.Default, encoding);

                using (XmlReader reader = XmlReader.Create(fileName, settings, readerContext)) {
                    while (await reader.ReadAsync()) {
                        // Read all the data
                    }
                }
            }
            catch (Exception ex) {
                return ex.Message;
            }

            return null;
        } 
    }
}