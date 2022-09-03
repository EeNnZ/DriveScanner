using System;
using System.Threading.Tasks;
using CommandLine;
using ShellProgressBar;
using ScannerCoreLib;
using System.Threading;

namespace ConsoleScanner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = false;
                x.EnableDashDash = true;
                x.IgnoreUnknownArguments = true;
            });
            var options = parser.ParseArguments<Options>(args).Value;

            var scanner = new Scanner(options, p =>
            {
                //TODO: Handle progress here
            });
            Console.WriteLine("Processing...");
            await scanner.ScanAsync();
            await scanner.Report();

            Console.WriteLine($"\r\nDone in {scanner.Watch.ElapsedMilliseconds} ms\r\nExit in 5 seconds...");
            Thread.Sleep(5000);
        }

    }
}
