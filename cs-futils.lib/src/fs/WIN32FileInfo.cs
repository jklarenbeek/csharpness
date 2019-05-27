using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace joham.cs_futils.fs
{
  /*
 * 
// In order to get the target path for shortcut "Shortcut to documents" folder:
string linkPathName = "C:\\Shortcut to documents\\target.lnk";
if (System.IO.File.Exists(linkPathName))
{
   IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
   IWshRuntimeLibrary.IWshShortcut link = IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(linkPathName);
   string Target = link.TargetPath);
}
 */
    class WIN32FileInfo : IEnumerable<WIN32_FIND_DATA>, IDisposable
    {

        #region WIN32_FIND_DATA

        enum WIN32_FILE_ERROR : int
        {
            ERROR_SUCCESS = 0,
            ERROR_INVALID_FUNCTION = 1,
            ERROR_FILE_NOT_FOUND = 2,
            ERROR_PATH_NOT_FOUND = 3,
            ERROR_ACCESS_DENIED = 5,
            ERROR_INVALID_HANDLE = 6,
            ERROR_NOT_ENOUGH_MEMORY = 8,
            ERROR_INVALID_DATA = 13,
            ERROR_INVALID_DRIVE = 15,
            ERROR_NO_MORE_FILES = 18,
            ERROR_NOT_READY = 21,
            ERROR_BAD_LENGTH = 24,
            ERROR_SHARING_VIOLATION = 32,
            ERROR_NOT_SUPPORTED = 50,
            ERROR_FILE_EXISTS = 80,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_BROKEN_PIPE = 109,
            ERROR_CALL_NOT_IMPLEMENTED = 120,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_NAME = 123,
            ERROR_BAD_PATHNAME = 161,
            ERROR_ALREADY_EXISTS = 183,
            ERROR_ENVVAR_NOT_FOUND = 203,
            ERROR_FILENAME_EXCED_RANGE = 206,
            ERROR_NO_DATA = 232,
            ERROR_PIPE_NOT_CONNECTED = 233,
            ERROR_MORE_DATA = 234,
            ERROR_DIRECTORY = 267,
            ERROR_OPERATION_ABORTED = 995,
            ERROR_NOT_FOUND = 1168,
            ERROR_NO_TOKEN = 1008,
            ERROR_DLL_INIT_FAILED = 1114,
            ERROR_NON_ACCOUNT_SID = 1257,
            ERROR_NOT_ALL_ASSIGNED = 1300,
            ERROR_UNKNOWN_REVISION = 1305,
            ERROR_INVALID_OWNER = 1307,
            ERROR_INVALID_PRIMARY_GROUP = 1308,
            ERROR_NO_SUCH_PRIVILEGE = 1313,
            ERROR_PRIVILEGE_NOT_HELD = 1314,
            ERROR_NONE_MAPPED = 1332,
            ERROR_INVALID_ACL = 1336,
            ERROR_INVALID_SID = 1337,
            ERROR_INVALID_SECURITY_DESCR = 1338,
            ERROR_BAD_IMPERSONATION_LEVEL = 1346,
            ERROR_CANT_OPEN_ANONYMOUS = 1347,
            ERROR_NO_SECURITY_ON_OBJECT = 1350,
            ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789
        }

        /// <summary>
        /// struct to WIN32_FIND_DATA for FindFile request to the WIN32 API.
        /// </summary>
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr fileHandle, ref WIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr fileHandle);

        private static void WinIOError(WIN32_FILE_ERROR errorCode, string str)
        {
            switch (errorCode)
            {
                case WIN32_FILE_ERROR.ERROR_FILE_EXISTS:
                    throw new System.IO.IOException("IO_FileExists :" + str);
                case WIN32_FILE_ERROR.ERROR_INVALID_PARAMETER:
                    throw new System.IO.IOException("IOError:" + MakeHRFromErrorCode(errorCode));
                case WIN32_FILE_ERROR.ERROR_FILENAME_EXCED_RANGE:
                    throw new System.IO.PathTooLongException("PathTooLong:" + str);
                case WIN32_FILE_ERROR.ERROR_FILE_NOT_FOUND:
                    throw new System.IO.FileNotFoundException("FileNotFound:" + str);
                case WIN32_FILE_ERROR.ERROR_PATH_NOT_FOUND:
                    throw new System.IO.DirectoryNotFoundException("PathNotFound:" + str);
                case WIN32_FILE_ERROR.ERROR_ACCESS_DENIED:
                    throw new UnauthorizedAccessException("UnauthorizedAccess:" + str);
                case WIN32_FILE_ERROR.ERROR_SHARING_VIOLATION:
                    throw new System.IO.IOException("IO_SharingViolation:" + str);
            }
            throw new System.IO.IOException("IOError:" + MakeHRFromErrorCode(errorCode));
        }

        private static int MakeHRFromErrorCode(WIN32_FILE_ERROR errorCode)
        {
            return (-2147024896 | (int)errorCode);
        }

        #endregion

        private string searchPath = null;

        private System.IntPtr fileHandle = INVALID_HANDLE_VALUE;
        private WIN32_FIND_DATA fileInfo = new WIN32_FIND_DATA();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        /// <example>
        /// using (WIN32FileInfo info = new WIN32FileInfo("C:\\WINDOWS\\*.exe")) 
        /// {
        ///     while (info.SearchNext())
        ///     {
        ///         Console.WriteLine(info.FileName);
        ///     }
        /// }
        /// 
        /// </example>
        public WIN32FileInfo(string searchPath)
        {
            if (String.IsNullOrWhiteSpace(Path.GetDirectoryName(searchPath)))
                throw new ArgumentException("searchPath argument cannot be null or empty");

            this.searchPath = searchPath;
        }

        public string SearchPath
        {
            get
            {
                return this.searchPath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        /// <example>
        /// using (WIN32FileInfo info = new WIN32FileInfo("C:\\WINDOWS\\*.exe")) 
        /// {
        ///     bool success = info.SearchFirst();
        ///     while (success)
        ///     {
        ///         Console.WriteLine(info.FileName);
        ///         success = info.SearchNext();
        ///     }
        /// }
        /// 
        /// </example>
        public bool SearchFirst()
        {
            this.SearchClose();

            this.fileHandle = FindFirstFile(this.searchPath, ref this.fileInfo);
            if (this.fileHandle == INVALID_HANDLE_VALUE)
            {
                WIN32_FILE_ERROR error = (WIN32_FILE_ERROR)System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                if (error != WIN32_FILE_ERROR.ERROR_FILE_NOT_FOUND)
                    WinIOError(error, this.searchPath);

                return false;
            }
            return true;
        }

        public bool SearchNext()
        {
            if (this.fileHandle == INVALID_HANDLE_VALUE)
            {
                return SearchFirst();
            }
            else
            {
                if (FindNextFile(this.fileHandle, ref this.fileInfo) == true)
                    return true;

                WIN32_FILE_ERROR error = (WIN32_FILE_ERROR)System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                if (error != WIN32_FILE_ERROR.ERROR_SUCCESS && error != WIN32_FILE_ERROR.ERROR_NO_MORE_FILES)
                    WinIOError(error, this.searchPath);

                this.SearchClose();

                return false;
            }
        }

        public void SearchClose()
        {
            if (this.fileHandle != INVALID_HANDLE_VALUE)
            {
                FindClose(fileHandle);
                this.fileHandle = INVALID_HANDLE_VALUE;
            }
        }

        #region Current fileInfo properties

        public string FileName
        {
            get { return this.fileInfo.FileName; }
        }
        public long FileLength
        {
            get { return this.fileInfo.FileLength; }
        }

        public FileAttributes Attributes
        {
            get { return this.fileInfo.Attributes; }
        }

        public bool IsFile
        {
            get { return this.fileInfo.IsFile; }
        }
        public bool IsDirectory
        {
            get { return this.fileInfo.IsDirectory; }
        }
        public bool IsReparsePoint
        {
            get { return this.fileInfo.IsReparsePoint; }
        }

        public bool HasReadOnlyFlag
        {
            get { return this.fileInfo.HasReadOnlyFlag; }
        }
        public bool HasHiddenFlag
        {
            get { return this.fileInfo.HasHiddenFlag; }
        }
        public bool HasSystemFlag
        {
            get { return this.fileInfo.HasSystemFlag; }
        }

        public DateTime CreationTime
        {
            get { return this.fileInfo.CreationTime; }
        }
        public DateTime LastWriteTime
        {
            get { return this.fileInfo.LastWriteTime; }
        }
        public DateTime LastAccessTime
        {
            get { return this.fileInfo.LastAccessTime; }
        }

        #endregion

        #region IEnumerable and IEnumerator implementations

        public IEnumerator<WIN32_FIND_DATA> GetEnumerator()
        {
            SearchClose();
            while (SearchNext())
                yield return this.fileInfo;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            this.SearchClose();
        }

        #endregion

    }

}