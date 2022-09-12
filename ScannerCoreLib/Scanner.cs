using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class Scanner
    {
        private ReportHelper _reportHelper;
        public IOptions Options { get; private set; }
        public string FileName { get => $"scan_{DateTime.Now:MMM_dd_HH_mm}.txt"; }
        public StringCollection Failed { get; }
        public long DriveFreeSpace => _currentDrive.TotalFreeSpace;
        public long DriveTotalSpace => _currentDrive.TotalSize;
        public long DriveOccupiedSpace => DriveTotalSpace - DriveFreeSpace;
        public long EntryCount = 0;
        private DriveInfo _currentDrive;

        public DriveInfo CurrentDrive
        {
            get { return _currentDrive; }
        }
        public Stopwatch Watch { get; private set; }
        public string Root { get; private set; }
        public HashSet<FsItem> FlattenResult { get; private set; }
        //TODO: Implement Tree result to use with WPF TreeView


        public Scanner(IOptions options)
        {
            Options = options;
            Root = Options.Root;
            SetCurrentDrive();
            FlattenResult = new HashSet<FsItem>();
            Failed = new StringCollection();
        }
        public void Scan()
        {
            Watch = Stopwatch.StartNew();
            try
            {
                Traverse(Root, (entry) =>
                    {
                        try
                        {
                            //Action with every reached entry
                            var item = new FsItem(entry);
                            FlattenResult.Add(item);
                        }
                        catch (IndexOutOfRangeException e)
                        { Log($"Message: {e.Message} -> StackTrace: {e.StackTrace}"); throw; }
                        catch (Exception e) { Log(e.Message); }
                    });
            }
            catch (ArgumentException e) { Log(e.Message); throw; }
            Watch.Stop();
        }
        private void Traverse(string root, Action<string> action)
        {
            if (!Directory.Exists(root))
            {
                throw new ArgumentException("Root directory doesn't exists", nameof(root));
            }
            int procCount = Environment.ProcessorCount;

            var dirs = new Stack<string>();
            dirs.Push(root);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = { };
                string[] files = { };

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e) { Log(e.Message); continue; }
                catch (DirectoryNotFoundException e) { Log(e.Message); continue; }
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e) { Log(e.Message); continue; }
                catch (FileNotFoundException e) { Log(e.Message); continue; }
                string[] entries = subDirs.Union(files).ToArray();
                try
                {
                    if (entries.Length < procCount)
                    {
                        foreach (var entry in entries)
                        {
                            action(entry);
                            EntryCount++;
                        }
                    }
                    else
                    {
                        Parallel.ForEach(entries, () => 0, (entry, loopState, localCount) =>
                        {
                            action(entry);
                            return (int)++localCount;
                        },
                                         (c) =>
                                         {
                                             Interlocked.Add(ref EntryCount, c);
                                         });
                    }
                }
                catch (AggregateException ae) { Log(ae.Message); }


                foreach (var str in subDirs)
                {
                    dirs.Push(str);
                }
            }
        }
        private void SetCurrentDrive()
        {
            var drives = Environment.GetLogicalDrives();
            var current = drives.Single(d => d[0].Equals(Root[0]));
            _currentDrive = new DriveInfo(current);
        }
        public void Report()
        {
            _reportHelper = new ReportHelper(this);
            string res = _reportHelper.BuildReport();
            string dest = Options.Path + FileName;

            foreach (var item in Failed)
                Console.WriteLine(item);

            File.WriteAllText(dest, res);
            Process.Start("notepad.exe", dest);
        }
        private void Log(string f)
        {
            Failed.Add(f);
        }
    }
}
