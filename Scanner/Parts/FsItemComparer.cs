using System.Collections.Generic;

namespace Scanner.Parts
{
    internal class FsItemComparer : IComparer<FsItem>
    {
        public int Compare(FsItem x, FsItem y) => y.ByteSize.CompareTo(x.ByteSize);
    }
}
