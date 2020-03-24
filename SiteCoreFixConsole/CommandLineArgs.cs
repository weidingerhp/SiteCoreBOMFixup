using System.IO;
using CommandLineParser.Arguments;

namespace SitCoreFixConsole {
    public class CommandLineArgs {
        [SwitchArgument('f', "fix-files", false, Description = "Overwrite Exisiting File with a fixed Version if possible (Remove UTF-8-BOM")]
        public bool FixFiles { get; set; }

        [SwitchArgument('d', "disable-backup", false, Description = "do not create a .bak file with the contents of the original File")]
        public bool DisableBackup { get; set; }

        [DirectoryArgument(shortName: 's', longName: "source-dir", Description = "Base directory to search for XMLs recursively", Optional = false)]
        public DirectoryInfo StartDirectory { get; set; }
        
    }
}