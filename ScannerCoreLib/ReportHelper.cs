using System;
using System.Collections.Generic;
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
            Scanner = scanner;
        }
        private readonly Scanner Scanner;
        public string BuildReport()
        {
            if(Scanner == null)
                return null;
            var grouping = Scanner.FlattenResult.Children
                .Where(x => x != null)
                .OrderByDescending(x => x.Size)
                .Take(300)
                .GroupBy(x => x.Name.Split('\\')[1]);
            var seconds = Convert.ToString(Scanner.Watch.ElapsedMilliseconds / 1000);
            var sb = new StringBuilder();
            foreach (var g in grouping)
            {
                sb.AppendLine($"Key: {g.Key}-------");
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
