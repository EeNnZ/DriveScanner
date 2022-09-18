using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Scanner.Parts
{
    public class FsItem
    {
        public string Name { get; }
        public bool IsDir { get; }
        public DateTime LastModified { get;}
        public DateTime Created { get;}
        public long ByteSize { get; }
        public FsItem(string entryName)
        {
            Name = entryName;
            IsDir = Directory.Exists(entryName);
            ByteSize = IsDir ? GetDirSize(entryName) : GetFileSize(entryName);
            LastModified = GetLastAccessedDate(entryName);
            Created = GetCreatedDate(entryName);
        }

        private DateTime GetCreatedDate(string entryName)
        {
            return IsDir ? Directory.GetCreationTime(entryName) : File.GetCreationTime(entryName);
        }

        private DateTime GetLastAccessedDate(string entryName)
        {
            return IsDir ? Directory.GetLastAccessTime(entryName) : File.GetLastWriteTime(entryName);
        }
        private long GetFileSize(string entryName)
        {
            try { return SizeHelper.GetFileSizeEx(entryName); }
            catch (Exception) { return 0; }
        }
        
        private long GetDirSize(string entryName)
        {
            try { return SizeHelper.GetWSHFolderSize(entryName); }
            catch (Exception) { return 0; }
        }

        [Obsolete]
        private long DirSizeManagedAPI(string entryName, ref long size)
        {
            throw new NotImplementedException();
            //string[] fileEntries = Directory.GetFiles(entryName);
            //foreach (string entry in fileEntries)
            //{
            //    Interlocked.Add(ref size, (new FileInfo(entry)).Length);
            //}
            //string[] subdirEntries = Directory.GetDirectories(entryName);
            //Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
            //{
            //    if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
            //    {
            //        subtotal += GetDirSize(subdirEntries[i]);
            //        return subtotal;
            //    }
            //    return 0;
            //},
            //    (x) => Interlocked.Add(ref size, x)
            //);
            //return size;
        }
    }
}
