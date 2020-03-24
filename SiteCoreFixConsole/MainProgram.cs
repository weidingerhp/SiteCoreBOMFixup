using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLineParser.Arguments;
using SiteCoreFileChecker.Data;

namespace SitCoreFixConsole {
    class MainProgram {
        static void Main(string[] args) {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            var arguments = new CommandLineArgs();
            try {
                parser.ExtractArgumentAttributes(arguments);
                parser.ParseCommandLine(args);
            }
            catch (CommandLineParser.Exceptions.CommandLineArgumentException ex) {
                Console.Error.WriteLine(ex.Message);
                parser.ShowUsage();
                return;
            }

            Task.WaitAll(Work(arguments));
        }

        private static async Task Work(CommandLineArgs arguments) {
            SiteCoreFileChecker.SiteCoreFileChecker checker = new SiteCoreFileChecker.SiteCoreFileChecker();

            await Console.Out.WriteLineAsync($"Checking all XML Files under {arguments.StartDirectory.FullName}");
            var files = await checker.ListFiles(arguments.StartDirectory.FullName);
            files = await checker.CheckFiles();
            foreach (var entry in files) {
                await Console.Out.WriteAsync($">> {entry.FileName, -10} - {entry.FlawMessage}");
                if (arguments.FixFiles) {
                    if (entry.FlawType != FileFlawType.NO_FLAW && entry.FlawType != FileFlawType.NOT_CHECKED) {
                        if (await checker.CorrectFile(entry, !arguments.DisableBackup)) {
                            await Console.Out.WriteAsync(" [Fixed]");
                        }
                        else {
                            await Console.Out.WriteAsync(" [Not Fixed]");
                        }
                    }
                }

                await Console.Out.WriteLineAsync();
            }
        }
    }
}