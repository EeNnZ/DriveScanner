using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class ReportHelper //TODO: Move to Scanner class?
    {
        public ReportHelper(Scanner scanner)
        {
            _scanner = scanner;
        }
        private readonly Scanner _scanner;
        public string BuildReport()
        {
            if(_scanner == null)
                return null;
            var grouping = _scanner.FlattenResult
                .Where(x => x != null)
                .OrderByDescending(x => x.Size)
                .Take(300)
                .GroupBy(x => x.Name.Split('\\')[1]);

            decimal round(long l) { return Math.Round(Convert.ToDecimal(l), 3); }

            var sb = new StringBuilder();
            sb.AppendFormat("Current drive: {0} | Total size: {1} | Free space: {2} | Occupied: {3}\r\n",
                _scanner.CurrentDrive.Name,
                round(_scanner.DriveTotalSpace),
                round(_scanner.DriveFreeSpace),
                round(_scanner.DriveOccupiedSpace));
            var seconds = Convert.ToString(_scanner.Watch.ElapsedMilliseconds / 1000);
            foreach (var g in grouping)
            {
                sb.AppendLine($"Key: ---- {g.Key} ----");
                sb.AppendLine("--------------------------------------------------------");
                foreach (var item in g)
                {
                    sb.AppendLine($"      {(item.IsDir ? "Directory" : "File     ")} ||  {item.Name} --> {item.Size} MB || {item.LastModified.ToShortDateString()}");
                }
            }
            sb.AppendLine("----------------------------DONE----------------------------");
            sb.AppendLine($"Elapsed time: {seconds} sec");
            return sb.ToString();
        }
    }
}
