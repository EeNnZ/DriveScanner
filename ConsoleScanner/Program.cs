using System;
using System.Threading.Tasks;
using CommandLine;
using ShellProgressBar;
using ScannerCoreLib;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.IO;

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
            var options = parser.ParseArguments<Options>(args).Value;

            var scanner = new Scanner(options);
            Console.WriteLine("Processing...");

            var task = new Task(() => scanner.Scan());
            var stopwatch = Stopwatch.StartNew();
            task.Start();

            while (!task.IsCompleted)
            {
                Console.Write('.');
                Thread.Sleep(100);
            }
            Console.WriteLine("Done");
            stopwatch.Stop();

            var r = new Reporter(scanner, options.ResLinesCount, stopwatch.Elapsed.Seconds);
            r.Report();

            if (options.OpenFileOnComplete) LaunchFile(r.ResPath);

            Console.WriteLine("Press any to exit...");
            Console.ReadKey();
        }
        static void LaunchFile(string url)
        {
            Process.Start("notepad.exe", url);
        }

    }
}
