using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerCoreLib
{
    public class Scanner
    {
        public List<string> Failed { get; }
        public string TotalDriveInfo
        {
            get
            {
                if(Root != null)
                {
                    decimal round(long l) { return Math.Round(Convert.ToDecimal(l), 3); }
                    var currentDrive = Drives.Single(d => d.Name[0].Equals(Root[0]));
                    var sb = new StringBuilder();
                    sb.AppendFormat("Current drive: {0} | Total size: {1} | Free space: {2} | Occupied: {3}",
                        currentDrive.Name,
                        round(currentDrive.TotalSize), 
                        round(currentDrive.TotalFreeSpace), 
                        round(currentDrive.TotalSize - currentDrive.TotalFreeSpace));
                    return sb.ToString();
                }
                return null;
            }
        }
        public DriveInfo[] Drives => System.Environment.GetLogicalDrives().Select(s => new DriveInfo(s)).ToArray();
        public Stopwatch Watch { get; private set; }
        public string Root { get; private set; }
        public FsEntry FlattenResult { get; private set; }
        //TODO: Implement Tree view?


        public Scanner(string root)
        {
            Root = root;
            FlattenResult = new FsEntry(root);
            Failed = new List<string>();
        }
        public async Task<FsEntry> ScanAsync()
        {
            Watch = Stopwatch.StartNew();
            try
            {
                await Traverse(Root,
                    async (entry) =>
                {
                    try
                    {
                        FlattenResult.AddChild(entry);
                    }
                    catch (FileNotFoundException e) { await LogAsync(e.Message); }
                    catch (IOException e) { await LogAsync(e.Message); }
                    catch (UnauthorizedAccessException e) { await LogAsync(e.Message); }
                    catch (SecurityException e) { await LogAsync(e.Message); }
                    catch (IndexOutOfRangeException e) { await LogAsync($"Message: {e.Message} -> StackTrace: {e.StackTrace}"); throw; }
                });
            }
            catch (ArgumentException e)
            {
                await LogAsync(e.Message);
                throw;
            }
            Watch.Stop();
            return FlattenResult;
        }


        private async Task Traverse(string root, Action<string> action) //TODO: Too lagre method, refactor ??
        {
            if (!Directory.Exists(root))
            {
                throw new ArgumentException("Root directory doesn't exists", nameof(root));
            }

            int entryCount = 0;
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
                catch (UnauthorizedAccessException e) { await LogAsync(e.Message); continue; }
                catch (DirectoryNotFoundException e) { await LogAsync(e.Message); continue; }
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e) { await LogAsync(e.Message); continue; }
                catch (FileNotFoundException e) { await LogAsync(e.Message); continue; }
                string[] entries = subDirs.Union(files).ToArray();
                try
                {
                    if (entries.Length < procCount)
                    {
                        foreach (var entry in entries)
                        {
                            action(entry);
                            entryCount++;
                        }
                    }
                    else
                    {
                        Parallel.ForEach(entries, () => 0, (entry, loopState, localCount) =>
                        {
                            action(entry);
                            return (int)++localCount;
                        },
                                         (c) => {
                                             Interlocked.Add(ref entryCount, c);
                                         });
                    }
                }
                catch (AggregateException ae) { await LogAsync(ae.Message); }
                

                foreach (var str in subDirs)
                {
                    dirs.Push(str);
                }
            }
        }
        private async Task LogAsync(string f)
        {
            await Task.Run(() =>
            {
                Failed.Add(f);
                Console.WriteLine(f);
            });
        }
    }
}
