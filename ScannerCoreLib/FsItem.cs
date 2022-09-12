using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class FsItem
    {
        public string Name { get; set; }
        public bool IsDir { get; set; }
        public DateTime LastModified { get; set; }
        private long _byteSize;
        public long ByteSize => _byteSize;
        public decimal Size
        {
            get
            {
                var dec = Convert.ToDecimal(_byteSize);
                var mb = Math.Round((dec / 1000000), 2);
                return mb;
            }
        }
        public FsItem(string root)
        {
            Name = root;
            IsDir = Directory.Exists(root);
            if (IsDir) { _byteSize = CalculateDirectorySize(root); }
            else { _byteSize = CalculateFileSize(root); }
            LastModified = Directory.GetLastWriteTime(root);
        }

        private long CalculateFileSize(string root)
        {
            var fileInfo = new FileInfo(root);
            return fileInfo.Length;
        }
        private long CalculateDirectorySize(string root)
        {
            long size = 0;
            try
            {
                string[] fileEntries = Directory.GetFiles(root);
                foreach (string entry in fileEntries)
                {
                    Interlocked.Add(ref size, (new FileInfo(entry)).Length);
                }
                string[] subdirEntries = Directory.GetDirectories(root);
                Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
                {
                    if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                    {
                        subtotal += CalculateDirectorySize(subdirEntries[i]);
                        return subtotal;
                    }
                    return 0;
                },
                    (x) => Interlocked.Add(ref size, x)
                );
                return size;
            }
            catch (Exception) { return 0; }
        }
    }
}
