using Scanner;
using CommandLine;

namespace ConsoleScanner
{
    public class Options : IOptions
    {
        [Option ('v', "volume", HelpText = "Drive to scan, example: D, C, F", Required = true)]
        public string DriveToScan { get; set; }
        [Option('f', "folder", HelpText = "Folder to write report, example: D:\\folder", Default = "D:\\", Required = false)]
        public string ResultFileDestinationFolder { get; set; }
        [Option('o', "open", HelpText = "Open report on complete or not", Default = true, Required = false)]
        public bool OpenFileOnComplete { get; set; }
        [Option('d', "duplicates", HelpText = "Search duplicate files, may increase processing time", Default = false, Required = false)]
        public bool FindDuplicates { get; set; }
        [Option('l', "lines", HelpText = "Lines count in result file", Default = 300, Required = false)]
        public int ResLinesCount { get; set; }
    }
}
