using Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerGui
{
    public class Options : IOptions
    {
        public string DriveToScan { get; set; }
        public string ResultFileDestinationFolder { get; set; }
        public bool OpenFileOnComplete { get; set; }
        public bool FindDuplicates { get; set; }
        public int ResLinesCount { get; set; }
    }
}
