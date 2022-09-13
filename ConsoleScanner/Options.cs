using ScannerCoreLib;
using CommandLine;

namespace ConsoleScanner
{
    public class Options : IOptions
    {
        [Option ('r', "root", HelpText = "Root directory to start scan", Required = true)]
        public string Root { get; set; }
        [Option('p', "path", HelpText = "Path to write report", Default = "D:\\", Required = false)]
        public string Path { get; set; }
        [Option('o', "open", HelpText = "Open report on complete or not", Default = true, Required = false)]
        public bool Open { get; set; }
    }
}
