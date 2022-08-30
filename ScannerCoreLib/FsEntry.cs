using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class FsEntry
    {
        public FsEntry(string name, long size, bool isDir, DateTime lastModified = default)
        {
            Name = name;
            Size = size;
            IsDir = isDir;
            LastModified = lastModified == default ? DateTime.Now : lastModified;
        }
        public FsEntry()
        {
            
        }

        public List<FsEntry> _children = new List<FsEntry>();

        public string Name { get; set; }
        public long Size { get; set; }
        public bool IsDir { get; set; }
        public DateTime LastModified { get; set; }
    }
}
