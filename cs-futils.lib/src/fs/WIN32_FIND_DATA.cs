using System;
using System.IO;
using System.Runtime.InteropServices;


namespace joham.cs_futils.fs
{
      /// <summary>
    /// struct to WIN32_FIND_DATA for FindFile request to the WIN32 API.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
    struct WIN32_FIND_DATA
    {
        private int dwFileAttributes;
        private int ftCreationTime_dwLowDateTime;
        private int ftCreationTime_dwHighDateTime;
        private int ftLastAccessTime_dwLowDateTime;
        private int ftLastAccessTime_dwHighDateTime;
        private int ftLastWriteTime_dwLowDateTime;
        private int ftLastWriteTime_dwHighDateTime;
        private int nFileSizeHigh;
        private int nFileSizeLow;
        private int dwReserved0;
        private int dwReserved;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 260)]
        private string cFileName;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 14)]
        private string cAlternateFileName;

        public string FileName
        {
            get { return this.cFileName; }
        }
        public long FileLength
        {
            get { return (this.IsFile) ? ToLong(this.nFileSizeHigh, this.nFileSizeLow) : 0; }
        }

        public FileAttributes Attributes
        {
            get { return (FileAttributes)this.dwFileAttributes; }
        }

        public bool IsFile
        {
            get
            {
                return ((this.Attributes & FileAttributes.Directory) != FileAttributes.Directory) &&
                        ((this.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint);
            }
        }
        public bool IsDirectory
        {
            get
            {
                return ( ((this.Attributes & FileAttributes.Directory) == FileAttributes.Directory) &&
                        !(this.cFileName == "." || this.cFileName == "..") &&
                        ((this.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                );
            }
        }
        public bool IsReparsePoint
        {
            get { return (this.Attributes & FileAttributes.ReparsePoint) != 0; }
        }

        public bool HasReadOnlyFlag
        {
            get { return (this.Attributes & FileAttributes.ReadOnly) != 0; }
        }
        public bool HasHiddenFlag
        {
            get { return (this.Attributes & FileAttributes.Hidden) != 0; }
        }
        public bool HasSystemFlag
        {
            get { return (this.Attributes & FileAttributes.System) != 0; }
        }

        public DateTime CreationTime
        {
            get
            {
                return DateTime.FromFileTimeUtc(
                    ToLong(this.ftCreationTime_dwHighDateTime, this.ftCreationTime_dwLowDateTime)
                ).ToLocalTime();
            }
        }
        public DateTime LastWriteTime
        {
            get
            {
                return DateTime.FromFileTimeUtc(
                    ToLong(this.ftLastWriteTime_dwHighDateTime, this.ftLastWriteTime_dwLowDateTime)
                ).ToLocalTime();
            }
        }
        public DateTime LastAccessTime
        {
            get
            {
                return DateTime.FromFileTimeUtc(
                    ToLong(this.ftLastAccessTime_dwHighDateTime, this.ftLastAccessTime_dwLowDateTime)
                ).ToLocalTime();
            }
        }

        private static long ToLong(int height, int low)
        {
            long v = (uint)height;
            v = v << 0x20;
            v = v | ((uint)low);
            return v;
        }


    }

}