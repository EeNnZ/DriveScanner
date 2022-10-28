using Scanner.Parts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Parts
{
    public class Reporter
    {
        private readonly DriveScanner _scanner;
        private DuplicateFinder _analyzer;
        private readonly int _resLinesCount, _seconds;
        private readonly bool _findDuplicates;
        public string ResPath { get; }
        public string ResultFilename
        {
            get
            {
                DriveInfo[] driveInfos = DriveInfo.GetDrives();
                char sysDriveLetter = Path.GetPathRoot(Environment.SystemDirectory)[0];
                char curDriveLetter = _scanner.CurrentDrive.Name[0];
                string res = $"{driveInfos.First(d => d.Name[0] != sysDriveLetter).Name}_scan_{DateTime.Now:MMM_dd_HH_mm}.txt";
                return res;
            }
        }

        public Reporter(DriveScanner scanner, int resLinesCount, int seconds, bool findDuplicates=false)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            ResPath = Path.Combine(_scanner.CurrentDrive.Name, ResultFilename);
            _resLinesCount = resLinesCount;
            _seconds = seconds;
            _findDuplicates = findDuplicates;
        }
        public StringBuilder Report()
        {
            var scanResult = _scanner.FlattenResult;
            var report = GetMainReport(scanResult);
            if (_findDuplicates)
            {
                _analyzer = new DuplicateFinder();
                var results = _analyzer.GetDuplicates(scanResult);
                var dr = results;
                report.Append(dr);
            }
            return report;
            //File.WriteAllText(ResPath, report.ToString());
        }

        private StringBuilder GetMainReport(IEnumerable<FsItem> result)
        {
            var grouping = result.Take(_resLinesCount).GroupBy(x => x.Name.Split('\\')[1]);
            var sb = new StringBuilder();
            sb.AppendLine("----------------------------SUMMARY----------------------------");
            sb.AppendFormat("Current drive: {0} | Total size: {1} | Free space: {2} | Occupied: {3}\r\n",
                _scanner.CurrentDrive.Name,
                SizeHelper.FormatSize(_scanner.Total),
                SizeHelper.FormatSize(_scanner.Free),
                SizeHelper.FormatSize(_scanner.Occupied));
            sb.AppendLine("----------------------------MAIN SECTION----------------------------");
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
            sb.AppendLine($"Elapsed time: {_seconds} sec");
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine();
            return sb;
        }
        private StringBuilder GetDuplicatesReport<K, V>(IEnumerable<IGrouping<K, V>> groupByExtList)
        {
            int numLines = Console.WindowHeight - 3;
            var sb = new StringBuilder();
            sb.AppendLine("----------------------------DUPLICATES SECTION----------------------------");
            sb.AppendLine($"FOUND: {groupByExtList.Count()} POSSIBLE DUPLICATED FILES - CHECK IT OUT BELOW");
            sb.AppendLine("----------------------------------------------------------------------------------");
            foreach (var filegroup in groupByExtList)
            {
                int currentLine = 0;
                do
                {
                    sb.AppendFormat("Filename = {0}\r\n", filegroup.Key.ToString() == string.Empty ? "[none]" : filegroup.Key.ToString());

                    var resultPage = filegroup.Skip(currentLine).Take(numLines);

                    foreach (var fileName in resultPage)
                    {
                        sb.AppendFormat("\t{0}\r\n", fileName);
                    }

                    currentLine += numLines;

                } while (currentLine < filegroup.Count());
            }
            return sb;
        }
    }
}
