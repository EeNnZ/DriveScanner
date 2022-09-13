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
        static void Main(string[] args)
        {
            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = false;
                x.CaseSensitive = false;
                x.IgnoreUnknownArguments = true;
                x.HelpWriter = Console.Error;
                x.EnableDashDash = true;
            });
            var options = parser.ParseArguments<Options>(args)
                .WithParsed(op =>
                {
                    Go(op);
                });
            Console.ReadLine();
        }
        static void Go(Options options)
        {
            var scanner = new Scanner(options);
            Console.WriteLine("Processing...");
            var timer = new Timer((s) =>
            {
                Console.WriteLine($"Scanned entries: {scanner.EntryCount}");
            }, null, 0, 500);
            Task.Run(() => scanner.Scan())
                .ContinueWith(t => scanner.Report()).Wait();
            timer.Dispose();
            Console.WriteLine($"\r\nDone in {scanner.Watch.ElapsedMilliseconds} ms\r\nPress any to exit...");
        }

    }
}
