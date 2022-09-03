namespace ScannerCoreLib
{
    public interface IOptions
    {
        string Root { get; set; }
        string Path { get; set; }
        bool Open { get; set; }
    }
}
