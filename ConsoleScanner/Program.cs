using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScannerCoreLib; 

namespace ConsoleScanner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var scanner = new Scanner();
            await scanner.ScanAsync("D:\\Games");
            var result = scanner.Result;

            Console.ReadLine();
        }
    }
}
