using Scanner;
using Scanner.Parts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WeatherGuiApp;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace ScannerGui
{
    public class MainViewModel : AbstractBindable
    {
        private string _drive; 
        private string _textResult;
        private string _resultDestination;
        private string _textStatus = "Ready";
        private int _linesCount = 300;
        private bool _openOnComplete;
        private int _progressValue;

        private DriveScanner _scanner;
        private Reporter _reporter;
        private DispatcherTimer _dispatcherTimer;
        private readonly IProgress<float> _progress;

        public string Drive
        {
            get => _drive;
            set => SetProperty(ref _drive, value);
        }
        public string ResultDestination
        {
            get => _resultDestination;
            set => SetProperty(ref _resultDestination, value);
        }
        public string TextStatus
        {
            get => _textStatus;
            set => SetProperty(ref _textStatus, value);
        }
        public string TextResult
        {
            get => _textResult;
            set => SetProperty(ref _textResult, value);
        }
        public int LinesCount
        {
            get => _linesCount;
            set => SetProperty(ref _linesCount, value);
        }
        public bool OpenOnComplete
        {
            get => _openOnComplete;
            set => SetProperty(ref _openOnComplete, value);
        }
        public bool SearchDuplicates { get; set; }
        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }
        public string[] Drives => GetDrives();

        public DelegateCommand ScanCommand => new DelegateCommand(async () => await DoWork());

        public MainViewModel()
        {
            _progress = new Progress<float>(value => ProgressValue = (int)value);
        }

        private async Task DoWork()
        {
            var options = new Options()
            {
                DriveToScan = Drive,
                ResultFileDestinationFolder = ResultDestination,
                OpenFileOnComplete = OpenOnComplete,
                FindDuplicates = SearchDuplicates,
                ResLinesCount = LinesCount
            };
            TextStatus = "Scanning...";
            _scanner = new DriveScanner(options, _progress);

            var stopwatch = Stopwatch.StartNew();
            
            await Task.Run(() => _scanner.Scan());


            stopwatch.Stop();

            _reporter = new Reporter(_scanner, options.ResLinesCount, stopwatch.Elapsed.Seconds);
            string report = _reporter.Report().ToString();
            
            if (options.OpenFileOnComplete) { Notepad.SendText(report); }
            else { TextResult = report; }
            TextStatus = "Done";
        }
        private string[] GetDrives()
        {
            var drives = DriveInfo.GetDrives().Select(x => x.Name[0].ToString()).ToArray();
            _resultDestination = $"{drives[1]}:\\";
            return drives;
        }
    }
}
