using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.IO;
using System.Security;

namespace Scanner.Parts
{
    public static class SizeHelper
    {
        static readonly string[] _suffixes =
        { "Bytes", "KB", "MB", "GB", "TB" };
        public static string FormatSize(long bytesize)
        {
            int i = 0;
            decimal dsize = bytesize;
            while (Math.Round(dsize / 1024) >= 1)
            {
                dsize /= 1024;
                i++;
            }
            return string.Format("{0:n1}{1}", dsize, _suffixes[i]);
        }
        #region Native
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
                             [MarshalAs(UnmanagedType.LPTStr)] string filename,
                             [MarshalAs(UnmanagedType.U4)] FileAccess access,
                             [MarshalAs(UnmanagedType.U4)] FileShare share,
                             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
                             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
                             IntPtr templateFile);
        #endregion
        public static long GetFileSizeEx(string filename)
        {
            IntPtr hFile = CreateFile(filename,
                                      FileAccess.ReadWrite,
                                      FileShare.Read,
                                      IntPtr.Zero,
                                      FileMode.Open,
                                      FileAttributes.ReadOnly,
                                      IntPtr.Zero);
            if(hFile.ToInt32() == -1) { return 0; }
            bool res = GetFileSizeEx(hFile, out long lpFileSize);
            if (!res) { return 0; }
            return lpFileSize;
        }
        public static long GetWSHFolderSize(string root)
        {
            var fso = new IWshRuntimeLibrary.FileSystemObject();
            try
            {
                long dirSize = (long)fso.GetFolder(root).Size;
                int released = Marshal.FinalReleaseComObject(fso);
                return dirSize;
            }
            catch (SecurityException)
            {
                int released = Marshal.FinalReleaseComObject(fso);
                return 0;
            }
        }
    }
}
