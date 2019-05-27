using System;
using System.IO;
using System.Runtime.InteropServices;

namespace joham.cs_futils.fs
{
    // Contains information that the SHFileOperation function uses to perform 
    // file operations. 
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;   // Window handle to the dialog box to display 
        // information about the status of the file 
        // operation. 
        public UInt32 wFunc;   // Value that indicates which operation to 
        // perform.
        public IntPtr pFrom;   // Address of a buffer to specify one or more 
        // source file names. These names must be
        // fully qualified paths. Standard Microsoft®   
        // MS-DOS® wild cards, such as "*", are 
        // permitted in the file-name position. 
        // Although this member is declared as a 
        // null-terminated string, it is used as a 
        // buffer to hold multiple file names. Each 
        // file name must be terminated by a single 
        // NULL character. An additional NULL 
        // character must be appended to the end of 
        // the final name to indicate the end of pFrom. 
        public IntPtr pTo;   // Address of a buffer to contain the name of 
        // the destination file or directory. This 
        // parameter must be set to NULL if it is not 
        // used. Like pFrom, the pTo member is also a 
        // double-null terminated string and is handled 
        // in much the same way. 
        public UInt16 fFlags;   // Flags that control the file operation. 

        public Int32 fAnyOperationsAborted;

        // Value that receives TRUE if the user aborted 
        // any file operations before they were 
        // completed, or FALSE otherwise. 

        public IntPtr hNameMappings;

        // A handle to a name mapping object containing 
        // the old and new names of the renamed files. 
        // This member is used only if the 
        // fFlags member includes the 
        // FOF_WANTMAPPINGHANDLE flag.

        [MarshalAs(UnmanagedType.LPWStr)]
        public String lpszProgressTitle;

        // Address of a string to use as the title of 
        // a progress dialog box. This member is used 
        // only if fFlags includes the 
        // FOF_SIMPLEPROGRESS flag.

        // Copies, moves, renames, or deletes a file system object. 
        // lpFileOp : Address of an SHFILEOPSTRUCT 
        //  structure that contains information this function needs 
        //  to carry out the specified operation. This parameter must 
        //  contain a valid value that is not NULL. You are 
        //  responsible for validating the value. If you do not 
        //  validate it, you will experience unexpected results.
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

    }
}
