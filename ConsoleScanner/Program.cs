using System;
using System.Threading.Tasks;
using CommandLine;
using System.Threading;
using System.Diagnostics;
using Scanner;
using Scanner.Parts;
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
#if DEBUG
            var options = new Options
            {
                DriveToScan = "D",
                ResLinesCount = 300,
                FindDuplicates = false,
                OpenFileOnComplete = true,
                ResultFileDestinationFolder = "D:\\"
            };
#else
            var options = parser.ParseArguments<Options>(args).Value;
#endif
            if (options == null) { Environment.Exit(-1); }
            var scanner = new DriveScanner(options);
            Console.WriteLine("Processing...");
            var stopwatch = Stopwatch.StartNew();
            try
            {

                var task = Task.Run(() => scanner.Scan());
                //var stopwatch = Stopwatch.StartNew;

                while (!task.IsCompleted)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Console.Write('.');
                        Thread.Sleep(100);
                    }
                    Console.Clear();
                    Console.WriteLine("Processing...");
                }
            }
            catch (Exception)
            {

                throw;
            }
            Console.WriteLine("Done");
            stopwatch.Stop();

            var r = new Reporter(scanner, options.ResLinesCount, stopwatch.Elapsed.Seconds, options.FindDuplicates);
            string report = r.Report().ToString();
            File.WriteAllText(r.ResPath, report);

            //if (options.OpenFileOnComplete) Notepad.SendText(report);

            Console.WriteLine("Press any to exit...");
            Console.ReadKey();
        }
        //static void LaunchFile(string url)
        //{
        //    Process.Start("notepad.exe", url);
        //}

    }
}
