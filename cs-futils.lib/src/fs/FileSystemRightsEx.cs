using System;
using System.Security.AccessControl;

namespace joham.cs_futils.fs
{

    static class FileSystemRightsEx
    {
        public static bool HasRights(this FileSystemRights left, FileSystemRights right)
        {
            return (left & right) == right;
        }

    }

}