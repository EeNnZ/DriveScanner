using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class Reporter
    {
        private readonly Scanner _scanner;
        private readonly int _resLinesCount, _seconds;
        public string ResPath { get; }
        public string ResultFilename => $"{_scanner.CurrentDrive}_scan_{DateTime.Now:MMM_dd_HH_mm}.txt";

        public Reporter(Scanner scanner, int resLinesCount, int seconds)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            ResPath = Path.Combine(_scanner.CurrentDrive.Name, ResultFilename);
            _resLinesCount = resLinesCount;
            _seconds = seconds;
        }
        public void Report()
        {
            var result = _scanner.FlattenResult;
            var rep = GetStringReport(result);
            File.WriteAllText(ResPath, rep);
        }

        private string GetStringReport(IEnumerable<FsItem> result)
        {
            var grouping = result.Take(_resLinesCount).GroupBy(x => x.Name.Split('\\')[1]);
            //    .Where(x => x != null)
            //    .OrderByDescending(x => x.Size)
            var sb = new StringBuilder();
            sb.AppendFormat("Current drive: {0} | Total size: {1} | Free space: {2} | Occupied: {3}\r\n",
                _scanner.CurrentDrive.Name,
                SizeHelper.FormatSize(_scanner.Total),
                SizeHelper.FormatSize(_scanner.Free),
                SizeHelper.FormatSize(_scanner.Occupied));
            var seconds = Convert.ToString(_seconds);
            foreach (var g in grouping)
            {
                sb.AppendLine($"Key: ---- {g.Key} ----");
                sb.AppendLine("--------------------------------------------------------");
                foreach (var item in g)
                {
                    sb.AppendLine($"      {(item.IsDir ? "Directory" : "File     ")} ||  {item.Name} --> {SizeHelper.FormatSize(item.ByteSize)} || {item.LastModified.ToShortDateString()}");
                }
            }
            sb.AppendLine("----------------------------DONE----------------------------");
            sb.AppendLine();
            sb.AppendLine($"Elapsed time: {seconds} sec");
            return sb.ToString();
        }
    }
}
