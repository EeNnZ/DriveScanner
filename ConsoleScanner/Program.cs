using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ScannerCoreLib;

namespace ConsoleScanner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string requested = string.Empty;
            string target = string.Empty;
#if DEBUG
            requested = "D:\\";
            target = "D:\\Desktop\\scan_results.txt";
#else

            if (args.Length > 0)
            {
                requested = $"{args[0]}\\";
                target = $"{args[1]}\\scan_results_{DateTime.Now:MMMM_dd_HH:MM:ss}.txt";
            }
            else
            {
                PrintUsage();
                Environment.Exit(0);
            }
#endif
            var scanner = new Scanner(requested);
            Console.WriteLine("Scanning...");
            await scanner.ScanAsync();
            var helper = new ReportHelper(scanner);
            string report = helper.BuildReport();
            File.WriteAllText(target, report);

            Console.WriteLine("Opening results...");
            //Process.Start("d:\\Utilities\\Notepad++\\notepad++.exe", target);
            Process.Start("notepad.exe", target);

            Console.WriteLine("Exit...");
            //Console.ReadLine();
        }

        private static void PrintUsage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---------------------USAGE---------------------");
            sb.AppendLine("| 1st argument -> directory or drive to scan   |");
            sb.AppendLine("| 2nd argument -> report file destination      |");
            sb.AppendLine("| Example: ConsoleScanner.exe D: D:\\Desktop    |");
            sb.AppendLine("-----------------------------------------------");
            Console.Write(sb.ToString());
        }

        
    }
}
