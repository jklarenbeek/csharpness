using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using joham.cs_futils.pscx;

namespace joham.cs_futils.fs
{

    public class FileSearch
    {

        private string m_searchRoot = "C:\\Users\\Public";

        public FileSearch(string rootPath)
        {
            if (Directory.Exists(rootPath))
            {
                this.m_searchRoot = rootPath;
            }
            else
            {
                throw new DirectoryNotFoundException(rootPath);
            }
        }

        public string SearchRoot
        {
            get { return this.m_searchRoot; }
        }

        public static String domainName = "JOHAM";

        #region File and Directory Permissions

        internal static SecurityIdentifier AccountExists(string account)
        {
            // TODO Get domain context
            string win32account = (account.IndexOf("\\") < 0) ? domainName + "\\" + account : account;

            try
            {
                NTAccount acct = new NTAccount(win32account);
                SecurityIdentifier id = (SecurityIdentifier)acct.Translate(typeof(SecurityIdentifier));

                if (id.IsAccountSid())
                    return id;
            }
            catch (IdentityNotMappedException)
            {
                /* Invalid user account */
            }

            return null;
        }

        internal static List<string> GetFolderSecurity(DirectoryInfo info)
        {
            List<string> accounts = new List<string>();
            if (info == null)
                return accounts;

            try
            {
                foreach (FileSystemAccessRule rule in info.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    accounts.Add(rule.IdentityReference.Value);
                }
            }
            catch (Exception e)
            {
                Message.Error("GetFolderSecurity failed!", e.ToString());
            }
            return accounts;
        }

        internal static bool CopyFileSystemSecurity(FileSystemSecurity src, FileSystemSecurity dst)
        {
            //dstSec.SetSecurityDescriptorSddlForm(srcSec.GetSecurityDescriptorSddlForm(AccessControlSections.All));
            //dstSec.SetSecurityDescriptorBinaryForm(srcSec.GetSecurityDescriptorBinaryForm());

            List<FileSystemAccessRule> rules = new List<FileSystemAccessRule>();
            bool hasInheritedRules = false;

            foreach (FileSystemAccessRule rule in src.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                hasInheritedRules |= rule.IsInherited;
                if (!rule.IsInherited && ((int)rule.FileSystemRights) != -1)
                {
                    rules.Add(new FileSystemAccessRule(
                            rule.IdentityReference,
                            rule.FileSystemRights,
                            rule.InheritanceFlags,
                            rule.PropagationFlags,
                            rule.AccessControlType)
                    );
                }
            }

            dst.SetAccessRuleProtection(src.AreAccessRulesProtected, hasInheritedRules);
            //dst.SetOwner(src.GetOwner(typeof(System.Security.Principal.NTAccount)));
            //dst.SetGroup(src.GetGroup(typeof(System.Security.Principal.NTAccount)));

            for (int i = 0; i < rules.Count; i++)
            {
                dst.AddAccessRule(rules[i]);
            }

            return true;
        }
        internal static void AddAccessFullControl(DirectoryInfo info, SecurityIdentifier user)
        {
            DirectorySecurity sec = info.GetAccessControl();
            sec.AddAccessRule(new FileSystemAccessRule(
                user,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow)
            );
            info.SetAccessControl(sec);
        }
        internal static void RemoveAccessFullControl(DirectoryInfo info, SecurityIdentifier user)
        {
            DirectorySecurity sec = info.GetAccessControl();
            sec.RemoveAccessRule(new FileSystemAccessRule(
                user,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow)
            );
            info.SetAccessControl(sec);
        }
        internal static void AddAccessFullControl(DirectoryInfo info, string win32account)
        {
            SecurityIdentifier sid = AccountExists(win32account);
            if (sid != null)
                AddAccessFullControl(info, sid);
        }
        internal static void RemoveAccessFullControl(DirectoryInfo info, string win32account)
        {
            SecurityIdentifier sid = AccountExists(win32account);
            if (sid != null)
                RemoveAccessFullControl(info, sid);
        }

        internal static void ElevatePermissions()
        {
            // other way to do this could be using reflection
            //Type privilegeType = Type.GetType( "System.Security.AccessControl.Privilege" );
            //MethodInfo enable = privilegeType.GetMethod( "Enable" );
            //MethodInfo revert = privilegeType.GetMethod( "Revert" );
            //object seBackupPrivilege = Activator.CreateInstance( privilegeType, "SeBackupPrivilege" );
            //enable.Invoke( seBackupPrivilege, null );
            //...
            //revert.Invoke( seBackupPrivilege, null );

            using (var hToken = new SafeTokenHandle(WindowsIdentity.GetCurrent().Token, false))
            {
                TokenPrivilegeCollection p = new TokenPrivilegeCollection();
                p.Add(new TokenPrivilege("SeRestorePrivilege", true)); //Necessary to set Owner Permissions
                p.Add(new TokenPrivilege("SeBackupPrivilege", true)); //Necessary to bypass Traverse Checking
                p.Add(new TokenPrivilege("SeTakeOwnershipPrivilege", true)); //Necessary to override FilePermissions & take Ownership
                Utils.AdjustTokenPrivileges(hToken, p);
            }
        }

        internal bool RemoveAllPermissions(bool recursive)
        {
            return RemoveAllPermissions("*", FileAttributes.Directory | FileAttributes.Normal, true);
        }
        internal bool RemoveAllPermissions(string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            return RemoveAllPermissionsRecursive(m_searchRoot, searchPattern, fileFilter, recursive);
        }
        private static bool RemoveAllPermissionsRecursive(string path, string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            bool success = true;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectorySecurity dsec = new DirectorySecurity();
                dsec.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                dsec.SetAccessRuleProtection(false, false);
                dir.SetAccessControl(dsec);

                using (WIN32FileInfo info = new WIN32FileInfo(PatternCombine(path, searchPattern)))
                {
                    while (info.SearchNext())
                    {
                        if (info.IsFile)
                        {
                            FileAttributes attr = fileFilter & info.Attributes;
                            if ((fileFilter & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System)) == 0)
                            {
                                if ((fileFilter & FileAttributes.Normal) != 0)
                                {
                                    FileInfo file = new FileInfo(Path.Combine(path, info.FileName));
                                    FileSecurity fsec = new FileSecurity();
                                    fsec.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                                    fsec.SetAccessRuleProtection(false, false);
                                    file.SetAccessControl(fsec);
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("No fileattribute filefilter available.");
                            }
                        }

                        if (info.IsDirectory)
                        {
                            if ((fileFilter & FileAttributes.Directory) != 0)
                            {
                                // in order to understand recursion, one first needs to understand recursion.
                                if (recursive)
                                {
                                    success &= RemoveAllPermissionsRecursive(Path.Combine(path, info.FileName), searchPattern, fileFilter, recursive);
                                }
                            }
                        }

                        if (info.IsReparsePoint)
                        {
                            if ((fileFilter & FileAttributes.ReparsePoint) != 0)
                            {
                                ;
                            }
                        }
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                Message.Error(string.Format("Het recursief verwijderen van de permissies in map {0} is mislukt.", path), e.ToString());
                return false;
            }
        }


        #endregion

        #region File and Folder functions

        internal static string PatternCombine(string path, string pattern)
        {
            return Path.Combine(path, ((String.IsNullOrWhiteSpace(pattern) == true) ? "*" : pattern));
        }


        internal static string XGetLongestPath(string currentPath, string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            string maxpath = currentPath;
            using (WIN32FileInfo info = new WIN32FileInfo(PatternCombine(currentPath, searchPattern)))
            {
                while (info.SearchNext())
                {
                    FileAttributes secattr = (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive);
                    if ((fileFilter & secattr) != 0)
                    {
                        if ((info.Attributes & secattr) == 0)
                            continue;
                    }

                    string filepath = Path.Combine(currentPath, info.FileName);
                    if (info.IsFile && (fileFilter & FileAttributes.Normal) == FileAttributes.Normal)
                    {
                        if (filepath.Length > maxpath.Length)
                            maxpath = filepath;
                    }
                    else if (info.IsDirectory)
                    {
                        if ((fileFilter & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (filepath.Length > maxpath.Length)
                                maxpath = filepath;
                        }
                        if (recursive)
                        {   // in order to understand recursion, one first needs to understand recursion.
                            filepath = XGetLongestPath(filepath, searchPattern, fileFilter, true);
                            if (filepath.Length > maxpath.Length)
                                maxpath = filepath;
                        }
                    }
                    else if (info.IsReparsePoint && (fileFilter & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        if (filepath.Length > maxpath.Length)
                            maxpath = filepath;
                    }
                }
                return maxpath;
            }
        }


        /// <summary>
        /// ????
        /// </summary>
        /// <returns>??????</returns>
        internal static List<string> XDir(string currentPath, string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            List<string> files = new List<string>();

            using (WIN32FileInfo info = new WIN32FileInfo(PatternCombine(currentPath, searchPattern)))
            {
                while (info.SearchNext())
                {
                    if (info.IsFile)
                    {
                        FileAttributes attr = fileFilter & info.Attributes;
                        if ((fileFilter & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System)) == 0)
                        {
                            if ((fileFilter & FileAttributes.Normal) != 0)
                                files.Add(Path.Combine(currentPath, info.FileName));
                        }
                        else
                        {
                            throw new NotImplementedException("No fileattribute filefilter available.");
                        }
                    }

                    if (info.IsDirectory)
                    {
                        if ((fileFilter & FileAttributes.Directory) != 0)
                            files.Add(Path.Combine(currentPath, info.FileName));

                        // in order to understand recursion, one first needs to understand recursion.
                        if (recursive)
                            files.AddRange(XDir(Path.Combine(currentPath, info.FileName), searchPattern, fileFilter, true));
                    }

                    if (info.IsReparsePoint)
                    {
                        if ((fileFilter & FileAttributes.ReparsePoint) != 0)
                        {
                            files.Add(Path.Combine(currentPath, info.FileName));
                        }
                    }
                    //TODO: sparse files and reparse points too?
                }
            }

            return files;
        }

        public List<string> GetFiles(bool recursive)
        {
            return FileSearch.XDir(this.m_searchRoot, null, FileAttributes.Normal | FileAttributes.Directory, recursive);
        }
        public List<string> GetFiles(string searchPattern, bool recursive)
        {
            return XDir(this.m_searchRoot, searchPattern, FileAttributes.Normal | FileAttributes.Directory, recursive);
        }
        public List<string> GetFiles(FileAttributes attr, bool recursive)
        {
            return XDir(this.m_searchRoot, null, attr, recursive);
        }
        public List<string> GetFiles(string searchPattern, FileAttributes attr, bool recursive)
        {
            return XDir(this.m_searchRoot, searchPattern, attr, recursive);
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <returns>??????</returns>
        internal static int CopyFolderPermissions(string templatePath, string destinationPath)
        {
            if (Directory.Exists(destinationPath) == true)
            {
                DirectoryInfo dst = new DirectoryInfo(destinationPath);
                if ((dst.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    throw new InvalidProgramException(string.Format("Cannot copy permissions to reparse point ({0}).", destinationPath.Length));

                DirectoryInfo src = new DirectoryInfo(templatePath);
                if ((src.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    // resolve reparse point to target and copy its permissions to destinationpath
                    string target = Junction.GetTarget(templatePath);
                    src = new DirectoryInfo(templatePath);
                }

                // copy file attributes
                dst.Attributes = src.Attributes;

                // get security descriptor and copy it
                DirectorySecurity srcSec = src.GetAccessControl();
                DirectorySecurity dstSec = new DirectorySecurity();
                CopyFileSystemSecurity(srcSec, dstSec);
                dst.SetAccessControl(dstSec);

                return 1;
            }

            return 0;
        }

        public int CopyPermissions(string folderName, string destinationPath, string searchPattern, bool recursive)
        {
            if (String.IsNullOrWhiteSpace(destinationPath))
                return 0;

            if (recursive == true)
                throw new NotImplementedException();

            if (searchPattern != null)
                throw new NotImplementedException();

            if (String.IsNullOrWhiteSpace(folderName))
                return CopyFolderPermissions(this.m_searchRoot, destinationPath);
            else
                return CopyFolderPermissions(Path.Combine(this.m_searchRoot, folderName), destinationPath);
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <returns>??????</returns>
        internal static int CopyFile(string sourceFile, string destinationFile)
        {
            FileInfo src = new FileInfo(sourceFile);

            if (destinationFile.Length > 260)
            {
                int dw = destinationFile.Length - destinationFile.LastIndexOf('\\');
                if (dw >= 248)
                {
                    Message.Error(
                            string.Format("Destination file base directory name is too long ({0}).", dw), new string[] { destinationFile }
                    );
                }

                int fw = destinationFile.Length - dw;
                if (fw >= (260 - dw))
                {
                    Message.Error(
                        string.Format("Destination file name is too long ({0}).", fw), new string[] { destinationFile }
                    );
                }
                return 0;
            }

            if (File.Exists(destinationFile) == false)
            {
                FileInfo dst = src.CopyTo(destinationFile);
                dst.Attributes = FileAttributes.Archive;
                dst.CreationTimeUtc = src.CreationTimeUtc;
                dst.LastAccessTimeUtc = src.LastAccessTimeUtc;
                dst.LastWriteTimeUtc = src.LastWriteTimeUtc;
                dst.Attributes = src.Attributes;

                FileSecurity dstSec = new FileSecurity();
                CopyFileSystemSecurity(src.GetAccessControl(), dstSec);
                dst.SetAccessControl(dstSec);

                return 1;
            }

            if ((src.Attributes & FileAttributes.System) == FileAttributes.System)
            {
                FileInfo dst = new FileInfo(destinationFile);
                if ((dst.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    dst.Attributes ^= FileAttributes.ReadOnly;
                }
                src.CopyTo(destinationFile, true);
                dst.Attributes = src.Attributes;

                FileSecurity dstSec = new FileSecurity();
                CopyFileSystemSecurity(src.GetAccessControl(), dstSec);
                dst.SetAccessControl(dstSec);

                return 1;
            }

            return 0;
        }
        internal static int CopyFolder(string templatePath, string destinationPath)
        {
            DirectoryInfo src = new DirectoryInfo(templatePath);
            if ((src.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                throw new InvalidProgramException("Cannot copy reparse point " + templatePath);

            if (destinationPath.Length > 248)
            {
                Message.Error(
                        string.Format("Destination directory name is too long ({0}).", destinationPath.Length), new string[] { destinationPath }
                );
                return 0;
            }

            DirectorySecurity srcSec = src.GetAccessControl();

            if (Directory.Exists(destinationPath) == false)
            {

                DirectoryInfo dst = Directory.CreateDirectory(destinationPath);
                dst.CreationTimeUtc = src.CreationTimeUtc;
                dst.LastAccessTimeUtc = src.LastAccessTimeUtc;
                dst.LastWriteTimeUtc = src.LastWriteTimeUtc;
                dst.Attributes = src.Attributes;

                DirectorySecurity dstSec = new DirectorySecurity();
                CopyFileSystemSecurity(srcSec, dstSec);
                dst.SetAccessControl(dstSec);

                return 1;
            }
            else
            {
                DirectoryInfo dst = new DirectoryInfo(destinationPath);
                if ((dst.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    throw new InvalidProgramException("Cannot copy to a reparse point " + templatePath);

                dst.Attributes = src.Attributes;

                DirectorySecurity dstSec = new DirectorySecurity();
                CopyFileSystemSecurity(srcSec, dstSec);
                dst.SetAccessControl(dstSec);

                return 0;
            }

        }

        internal static int XCopy(string sourcePath, string destinationPath, string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            int fileCount = 0;

            using (WIN32FileInfo info = new WIN32FileInfo(PatternCombine(sourcePath, searchPattern)))
            {
                while (info.SearchNext())
                {
                    if (info.IsFile)
                    {
                        FileAttributes attr = fileFilter & info.Attributes;
                        if ((fileFilter & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System)) == 0)
                        {
                            if ((fileFilter & FileAttributes.Normal) != 0)
                                fileCount += CopyFile(
                                    Path.Combine(sourcePath, info.FileName),
                                    Path.Combine(destinationPath, info.FileName));
                        }
                        else
                        {
                            if ((fileFilter & (FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System)) != 0)
                            {
                                fileCount += CopyFile(
                                    Path.Combine(sourcePath, info.FileName),
                                    Path.Combine(destinationPath, info.FileName));
                            }

                        }

                        continue;
                    }

                    if (info.IsDirectory)
                    {
                        if ((fileFilter & FileAttributes.Directory) != 0)
                            fileCount += CopyFolder(
                                Path.Combine(sourcePath, info.FileName),
                                Path.Combine(destinationPath, info.FileName));

                        if (recursive)
                            fileCount += XCopy(
                                Path.Combine(sourcePath, info.FileName),
                                Path.Combine(destinationPath, info.FileName),
                                searchPattern,
                                fileFilter,
                                true);

                        continue;
                    }

                    if (info.IsReparsePoint)
                    {
                        if ((fileFilter & FileAttributes.ReparsePoint) != 0)
                        {
                            ;
                        }
                    }
                }
            }

            return fileCount;
        }

        public int CopyTo(string destinationPath, bool recursive)
        {
            return CopyTo(destinationPath, null, recursive);
        }
        public int CopyTo(string destinationPath, string searchPattern, bool recursive)
        {
            int count = 0;
            if (Directory.Exists(destinationPath) == false)
            {
                count += CopyFolder(this.m_searchRoot, destinationPath);
                AddAccessFullControl(new DirectoryInfo(destinationPath), WindowsIdentity.GetCurrent().User);
                count += XCopy(this.m_searchRoot, destinationPath, searchPattern, FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory, recursive);
                RemoveAccessFullControl(new DirectoryInfo(destinationPath), WindowsIdentity.GetCurrent().User);
            }
            else
            {
                count += XCopy(this.m_searchRoot, destinationPath, searchPattern, FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory, recursive);
            }
            return count;
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <returns>??????</returns>
        internal static int XCount(string currentPath, string searchPattern, FileAttributes fileFilter, bool recursive)
        {
            int fileCount = 0;

            using (WIN32FileInfo info = new WIN32FileInfo(PatternCombine(currentPath, searchPattern)))
            {
                while (info.SearchNext())
                {
                    if (info.IsFile)
                    {
                        FileAttributes attr = fileFilter & info.Attributes;
                        if ((fileFilter & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System)) == 0)
                        {
                            if ((fileFilter & FileAttributes.Normal) != 0)
                                fileCount += 1;
                        }
                        else
                        {
                            throw new NotImplementedException("No fileattribute filefilter available.");
                        }
                    }

                    if (info.IsDirectory)
                    {
                        if ((fileFilter & FileAttributes.Directory) != 0)
                            fileCount += 1;

                        if (recursive)
                            fileCount += XCount(Path.Combine(currentPath, info.FileName), searchPattern, fileFilter, true);

                    }

                    if (info.IsReparsePoint)
                    {
                        if ((fileFilter & FileAttributes.ReparsePoint) != 0)
                        {
                            fileCount += 1;
                        }
                    }
                }
            }

            return fileCount;
        }

        public int Count(bool recursive)
        {
            return Count(null, recursive);
        }
        public int Count(string searchPattern, bool recursive)
        {
            return XCount(this.m_searchRoot, searchPattern, FileAttributes.Normal | FileAttributes.Directory, recursive);
        }

        #endregion

    }

}
