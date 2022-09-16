namespace ScannerCoreLib
{
    public interface IOptions
    {
        string DriveToScan { get; set; }
        string ResultFileDestinationFolder { get; set; }
        bool OpenFileOnComplete { get; set; }
        int ResLinesCount { get; set; }
    }
}
