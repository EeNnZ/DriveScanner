using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Parts
{
    //TODO: Rewrite as generic to accept key as external object?
    public class DuplicateFinder
    {
        public DuplicateFinder()
        {
        }
        public IEnumerable<IGrouping<ComparsionKey, string>> GetDuplicates(IEnumerable<FsItem> fsItems)
        {
            var source = fsItems.Where(x => !(x.IsDir)).Select(x => new FileInfo(x.Name));
            var resultSb = QueryDuplicates(source);
            return resultSb;
        }
        private IEnumerable<IGrouping<ComparsionKey, string>> QueryDuplicates(IEnumerable<FileInfo> files)
        {
            var grouping = files
                .GroupBy(file => new ComparsionKey()
                {
                    Name = file.Name,
                    Extension = file.Extension,
                    Length = SizeHelper.GetFileSizeEx(file.FullName)
                }, file => file.FullName)
                .Where(group => (group.Count() > 1));

            var list = grouping.ToList();

            int i = grouping.Count();

            return grouping;
        }
    }
}
