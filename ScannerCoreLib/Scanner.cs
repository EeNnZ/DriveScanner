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
    public class Scanner //TODO: Class lacks incapsulation
    {
        private readonly List<string> _failed; //TODO:  Fix
        public long Total { get; set; } //TODO: Need use
        public long Occupied { get; set; } //TODO: Need use

        public List<string> Failed
        {
            get { return _failed; }
        }
        public string CurrentEntry { get; set; } //TODO: Need use

        private FsEntry _result;
        public FsEntry Result { get { return _result; } }

        public Scanner()
        {
            _failed = new List<string>();
            _result = new FsEntry();
        }

        public async Task ScanAsync(string root)
        {
            try
            {
                _result = CreateEntry(root).Result;
                await TraverseEntry(root, async f =>
                {
                    try
                    {
                        var entry = CreateEntry(f).Result;
                        _result._children.Add(entry);
                    }
                    catch (FileNotFoundException) { }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                    catch (SecurityException) { }
                    await LogAsync(f);
                });
            }
            catch (ArgumentException e)
            {
                await LogAsync(e.Message);
                throw;
            }
        }

        private async Task<FsEntry> CreateEntry(string root)
        {
            FileSystemInfo fsi;
            if (Directory.Exists(root)) { fsi = new DirectoryInfo(root); }
            else { fsi = new FileInfo(root); }

            return new FsEntry()
            {
                Name = fsi.FullName,
                IsDir = fsi is DirectoryInfo,
                Size = fsi is DirectoryInfo ? await CalculateDirSizeAsync(fsi as DirectoryInfo) : CalculateFileSize(fsi as FileInfo),
                LastModified = fsi.LastWriteTime
            };
        }

        private long CalculateFileSize(FileInfo fileInfo)
        {
            return fileInfo.Length;
        }

        public  async Task TraverseEntry(string root, Action<string> action) //TODO: Too lagre method, refactor ??
        {
            int entryCount = 0;
            var sw = Stopwatch.StartNew();

            int procCount = Environment.ProcessorCount;

            var dirs = new Stack<string>();
            if (!Directory.Exists(root))
            {
                throw new ArgumentException("Root directory doesn't exists", nameof(root));
            }
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
                catch (UnauthorizedAccessException e)
                {
                    await LogAsync(e.Message);
                    continue;
                }
                // Thrown if another process has deleted the directory after retrieved its name.
                catch (DirectoryNotFoundException e)
                {
                    await LogAsync(e.Message);
                    continue;
                }
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    await LogAsync(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    await LogAsync(e.Message);
                    continue;
                }
                catch (IOException e)
                {
                    await LogAsync(e.Message);
                    continue;
                }
                string[] entries = files.Union(subDirs).ToArray();
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
                catch (AggregateException ae)
                {
                    await LogAsync(ae.Message);
                }
                foreach(var str in subDirs)
                    dirs.Push(str);
            }
        }

        private async Task<long> CalculateDirSizeAsync(DirectoryInfo info)
        {
            return await Task.Run(() => CalculateDirSize(info));
        }

        public  long CalculateDirSize(DirectoryInfo info)
        {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(info.FullName);
            foreach(string entry in fileEntries)
            {
                Interlocked.Add(ref size, (new FileInfo(entry)).Length);
            }
            string[] subdirEntries = Directory.GetDirectories(info.FullName);
            Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
            {
                if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    subtotal += CalculateDirSize(new DirectoryInfo(subdirEntries[i]));
                    return subtotal;
                }
                return 0;
            },
                (x) => Interlocked.Add(ref size, x)
            );
            return size;
        }

        private async Task LogAsync(string f)
        {
            await Task.Run(() =>
            {
                _failed.Add(f);
                Console.WriteLine(f);
            });
        }
    }
}
