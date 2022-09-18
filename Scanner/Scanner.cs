using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading;

namespace Scanner.Parts
{
    public class DriveScanner
    {
        private IProgress<float> _progress;
        public SortedSet<FsItem> FlattenResult { get; private set; } = new SortedSet<FsItem>(new FsItemComparer());
        public StringCollection Fails { get; } = new StringCollection();
        public DriveInfo CurrentDrive { get; private set; }
        public long Free { get; private set; }
        public long Total { get; private set; }
        public long Occupied { get; private set; }
        public string CurrentEntry { get; private set; }
        public long TraversedBytes { get; private set; } = 0;


        //public double Progress => Occupied == 0 ? 0 : Total * (double) 100/Occupied;

        //TODO: Implement Tree result to use with WPF TreeView


        public DriveScanner(IOptions options)
        {
            CurrentDrive = new DriveInfo(options.DriveToScan);
            Free = CurrentDrive.TotalFreeSpace;
            Total = CurrentDrive.TotalSize;
            Occupied = Total - Free;
        }
        public DriveScanner(IOptions options, IProgress<float> progress)
            :this(options)
        {
            _progress = progress;
        }
        public void Scan()
        {
            try
            {
                Traverse(CurrentDrive.Name, (entryName) =>
                {
                    try
                    {
                        CurrentEntry = entryName;
                        FsItem item = new FsItem(entryName);
                        FlattenResult.Add(item);
                        TraversedBytes += item.ByteSize;
                        _progress.Report(TraversedBytes / (float)Occupied * 100);
#if DEBUG
                        Thread.Sleep(200);
#endif
                    }
                    catch (IndexOutOfRangeException e) { LogFail($"Message: {e.Message} -> StackTrace: {e.StackTrace}"); throw; }
                    catch (Exception e) { LogFail(e.Message); }
                });
            }
            catch (ArgumentException e) { LogFail(e.Message); throw; }
        }
        private void Traverse(string root, Action<string> action)
        {
            if (!Directory.Exists(root))
            {
                throw new ArgumentException("Root directory doesn't exists", nameof(root));
            }
            int procCount = Environment.ProcessorCount;
            long entryCount = 0;

            var dirs = new Stack<string>();
            dirs.Push(root);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] entries;

                try
                {
                    entries = Directory.GetFileSystemEntries(currentDir);
                }
                catch (UnauthorizedAccessException e) { LogFail(e.Message); continue; }
                catch (DirectoryNotFoundException e) { LogFail(e.Message); continue; }
                catch (FileNotFoundException e) { LogFail(e.Message); continue; }

                foreach (var entry in entries)
                {
                    action(entry);
                    entryCount++;
                }

                var subDirs = entries.Where(e => Directory.Exists(e));
                foreach (var str in subDirs)
                {
                    dirs.Push(str);
                }
            }
        }
        private void LogFail(string f)
        {
            Fails.Add(f);
        }
    }
}
