using System.Collections.Generic;

namespace ScannerCoreLib
{
    internal class FsItemComparer : IComparer<FsItem>
    {
        public int Compare(FsItem x, FsItem y)
        {
            return y.ByteSize.CompareTo(x.ByteSize);
        }
    }
}
