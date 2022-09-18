using System;
using System.IO;

namespace Scanner.Parts
{

    public class ComparsionKey
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Length { get; set; }

        public override bool Equals(object obj)
        {
            ComparsionKey other = (ComparsionKey)obj;
            return other.Extension == Extension &&
                   other.Length == Length &&
                   other.Name == Name;
        }

        public override int GetHashCode()
        {
            string str = $"{Length} {Name}.{Extension}";
            return str.GetHashCode();
        }
        public override string ToString()
        {
            return $"{Name} {Length}.{Extension}";
        }
    }
}
