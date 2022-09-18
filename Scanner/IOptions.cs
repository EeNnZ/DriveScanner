namespace Scanner
{
    public interface IOptions
    {
        string DriveToScan { get; set; }
        string ResultFileDestinationFolder { get; set; }
        bool OpenFileOnComplete { get; set; }
        bool FindDuplicates { get; set; }
        int ResLinesCount { get; set; }
    }
}
