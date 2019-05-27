using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace joham.cs_futils.fs
{

    static class FileSystemEffectiveRights
    {
        public static FileSystemRights GetRights(string userName, string path)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("userName");
            }

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new ArgumentException(string.Format("path:  {0}", path));
            }

            return GetEffectiveRights(userName, path);
        }

        private static FileSystemRights GetEffectiveRights(string userName, string path)
        {
            FileSystemAccessRule[] accessRules = GetAccessRulesArray(userName, path);
            FileSystemRights denyRights = 0;
            FileSystemRights allowRights = 0;

            for (int index = 0, total = accessRules.Length; index < total; index++)
            {
                FileSystemAccessRule rule = accessRules[index];

                if (rule.AccessControlType == AccessControlType.Deny)
                {
                    denyRights |= rule.FileSystemRights;
                }
                else
                {
                    allowRights |= rule.FileSystemRights;
                }
            }

            return (allowRights | denyRights) ^ denyRights;
        }

        private static FileSystemAccessRule[] GetAccessRulesArray(string userName, string path)
        {
            // get all access rules for the path - this works for a directory path as well as a file path
            AuthorizationRuleCollection authorizationRules = (new FileInfo(path)).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));

            // get the user's sids
            string[] sids = GetSecurityIdentifierArray(userName);

            // get the access rules filtered by the user's sids
            return (from rule in authorizationRules.Cast<FileSystemAccessRule>()
                    where sids.Contains(rule.IdentityReference.Value)
                    select rule).ToArray();
        }

        private static string[] GetSecurityIdentifierArray(string userName)
        {
            // connect to the domain
            PrincipalContext pc = new PrincipalContext(ContextType.Domain);

            // search for the domain user
            UserPrincipal user = new UserPrincipal(pc);
            user.SamAccountName = (userName.IndexOf('\\') < 0) ? userName : userName.Substring(userName.IndexOf('\\') + 1);

            PrincipalSearcher search = new PrincipalSearcher(user);
            user = search.FindOne() as UserPrincipal;
            search.Dispose();

            if (user == null)
            {
                throw new ApplicationException(string.Format("Gebruikers Account is onbekend!:  {0}", userName));
            }

            // use WindowsIdentity to get the user's groups
            WindowsIdentity windowsIdentity = new WindowsIdentity(user.UserPrincipalName);
            string[] sids = new string[windowsIdentity.Groups.Count + 1];

            sids[0] = windowsIdentity.User.Value;

            for (int index = 1, total = windowsIdentity.Groups.Count; index < total; index++)
            {
                sids[index] = windowsIdentity.Groups[index].Value;
            }

            return sids;
        }

    }
}
