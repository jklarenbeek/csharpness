using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace joham.cs_futils.ldap
{
    public static class AccountManagementExtensions
    {

        public static String GetProperty(this Principal principal, String property)
        {
            DirectoryEntry directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry.Properties.Contains(property))
                return directoryEntry.Properties[property].Value.ToString();
            else
                return String.Empty;
        }

        public static String GetCompany(this Principal principal)
        {
            return principal.GetProperty("company");
        }

        public static String GetDepartment(this Principal principal)
        {
            return principal.GetProperty("department");
        }
        
        public static String GetEmail(this Principal principal)
        {
            return principal.GetProperty("mail");
        }

        public static String GetManagedBy(this Principal principal)
        {
            return principal.GetProperty("managedBy");
        }

        public static String GetMembers(this Principal principal)
        {
            return principal.GetProperty("Members");
        }

    }

}
