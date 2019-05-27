using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Data;

namespace joham.cs_futils.ldap
{
    public class LdapDAL
    {
        private const string LDAP_ROOT_CONTAINER = "OU=JOHAM,dc=joham,dc=nl";
        private const string LDAP_DOMAIN_NAME = "JOHAM";


        private const string ADGROUP_PMROOT = "JOHAM_Roles";

        public const string ADGROUP_MANAGEMENT = "Management";
        public const string ADGROUP_ONDERNEMINGSRAAD = "Ondernemingsraad";
        public const string ADGROUP_SECRETARIAAT = "secretariaat";
        public const string ADGROUP_ADMINISTRATION = "Administratie";
        public const string ADGROUP_KERNTEAM = "KernTeam";
        public const string ADGROUP_KWALITEITSTEAM = "KwaliteitsTeam";
        public const string ADGROUP_SYSADMIN = "Beheer";
        public const string ADGROUP_INTERNETREDACTIE = "InternetRedactie";
        public const string ADGROUP_NIEUWSBRIEFREDACTIE = "NieuwsbriefRedactie";

        public const string KEY_ADGROUP = "ADGroup";
        public const string KEY_ADDOMAIN = "domain";
        public const string KEY_ADACCOUNT = "account";
        public const string KEY_ADEMAIL = "mail";
        public const string KEY_MANAGEDBYID = "managedById";


        public readonly TimeSpan CACHE_ADROOTGROUPS_EXPIRATION = new TimeSpan(12, 0, 0);
        private const string CACHE_ADROOTGROUPS_KEY = "JohamADRootGroups";
        private readonly object CACHE_ADROOTGROUPS_LOCK = new object();
        public DataTable GetADRootGroups()
        {
            DataTable ldap = HttpRuntime.Cache[CACHE_ADROOTGROUPS_KEY] as DataTable;
            if (ldap == null)
            {
                lock (CACHE_ADROOTGROUPS_LOCK)
                {
                    ldap = HttpRuntime.Cache[CACHE_ADROOTGROUPS_KEY] as DataTable;
                    if (ldap != null)
                        return ldap;

                    ldap = new DataTable(CACHE_ADROOTGROUPS_KEY);
                    ldap.Columns.Add(KEY_ADGROUP, typeof(string));
                    ldap.Columns.Add(KEY_ADDOMAIN, typeof(string));
                    ldap.Columns.Add(KEY_ADACCOUNT, typeof(string));
                    ldap.Columns.Add(KEY_ADEMAIL, typeof(string));
                    ldap.Columns.Add(KEY_MANAGEDBYID, typeof(string));

                    // TODO: set ldap distinguisedname in web.config
                    using (PrincipalContext context = new PrincipalContext(ContextType.Domain, LDAP_DOMAIN_NAME, LDAP_ROOT_CONTAINER))
                    {
                        using (GroupPrincipal root = GroupPrincipal.FindByIdentity(context, IdentityType.Name, ADGROUP_PMROOT))
                        {
                            if (root != null)
                            {
                                string[] groups = root.Members.Select(r=>r.Name).ToArray();

                                foreach (string name in groups)
                                {
                                    
                                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, name))
                                    {
                                        if (group != null)
                                        {
                                            DataRow row = ldap.NewRow();
                                            row[KEY_ADGROUP] = group.Name;
                                            row[KEY_ADDOMAIN] = group.Context.Name;
                                            row[LdapDAL.KEY_ADACCOUNT] = group.SamAccountName;
                                            row[KEY_ADEMAIL] = group.GetEmail();
                                            row[KEY_MANAGEDBYID] = GetEmployeeIdByNTAccount(context.Name, UserPrincipal.FindByIdentity(
                                                context, 
                                                IdentityType.DistinguishedName,
                                                group.GetManagedBy()).SamAccountName);
                                            ldap.Rows.Add(row);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ldap.AcceptChanges();

                    HttpRuntime.Cache.Insert(
                        CACHE_ADROOTGROUPS_KEY,
                        ldap,
                        null,
                        DateTime.Now.Add(CACHE_ADROOTGROUPS_EXPIRATION),
                        System.Web.Caching.Cache.NoSlidingExpiration);

                }
            }

            return ldap;
        }

        public readonly TimeSpan CACHE_ADGROUPUSERS_EXPIRATION = new TimeSpan(12, 0, 0);
        private const string CACHE_ADGROUPUSERS_KEY = "JohamPMADGroupUsers";
        private readonly object CACHE_ADGROUPUSERS_LOCK = new object();
        protected DataTable SelectADGroupUsers(bool disabled)
        {
            DataTable ldap = new DataTable(CACHE_ADGROUPUSERS_KEY);
            ldap.Columns.Add(KEY_ADDOMAIN, typeof(string));
            ldap.Columns.Add(KEY_ADGROUP, typeof(string));
            ldap.Columns.Add(ProjectPage.KEY_EMPLOYEEID, typeof(string));
            ldap.Columns.Add(LdapDAL.KEY_ADACCOUNT, typeof(string));

            string[] groups = GetADRootGroups().AsEnumerable()
                .Select(r => r.Field<string>(KEY_ADGROUP))
                .ToArray();

            // TODO: set ldap distinguisedname in web.config
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, LDAP_DOMAIN_NAME, LDAP_ROOT_CONTAINER))
            {
                foreach (string groupName in groups)
                {
                    using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName))
                    {
                        if (group != null)
                        {
                            foreach(Principal member in group.GetMembers())
                            {
                                UserPrincipal user = member as UserPrincipal;
                                if (user != null)
                                {
                                    /*
                                    int uac = Convert.ToInt32(user.GetProperty("userAccountControl")[0]);

                                    const int ADS_UF_ACCOUNTDISABLE = 0x00000002;
                                    const int ADS_UF_LOCKOUT = 0x00000010;

                                    bool accountIsDisabled = (uac & ADS_UF_ACCOUNTDISABLE) == ADS_UF_ACCOUNTDISABLE;
                                    bool accountIsLockedOut = (uac & ADS_UF_LOCKOUT) == ADS_UF_LOCKOUT;
                                    */

                                    if (user.Enabled == true || disabled == true)
                                    {
                                        string employeeId = GetEmployeeIdByNTAccount(group.Context.Name, user.SamAccountName);
                                        if (String.IsNullOrWhiteSpace(employeeId) == false)
                                        {
                                            DataRow row = ldap.NewRow();
                                            row[KEY_ADDOMAIN] = group.Context.Name;
                                            row[KEY_ADGROUP] = group.Name;
                                            row[ProjectPage.KEY_EMPLOYEEID] = employeeId;
                                            row[LdapDAL.KEY_ADACCOUNT] = user.SamAccountName;
                                            ldap.Rows.Add(row);
                                        }
                                    }
                                }
                            }
                            /*
                            string[] members = group.Members.Select(r => r.SamAccountName).ToArray();
                            foreach (string samaccount in members)
                            {
                                string employeeId = GetAfasEmployeeId(group.Context.Name, samaccount);
                                if (String.IsNullOrWhiteSpace(employeeId) == false)
                                {
                                    DataRow row = ldap.NewRow();
                                    row[KEY_DOMAIN] = group.Context.Name;
                                    row[KEY_ADGROUP] = group.Name;
                                    row[ProjectPage.KEY_EMPLOYEEID] = employeeId;
                                    row[LdapDAL.KEY_ADACCOUNT] = samaccount;
                                    ldap.Rows.Add(row);
                                }
                            }
                             */
                        }
                    }
                }
            }

            ldap.AcceptChanges();

            return ldap;
        }
        public DataTable GetADGroupUsers()
        {
            DataTable ldap = HttpRuntime.Cache[CACHE_ADGROUPUSERS_KEY] as DataTable;
            if (ldap == null)
            {
                lock (CACHE_ADGROUPUSERS_LOCK)
                {
                    ldap = HttpRuntime.Cache[CACHE_ADGROUPUSERS_KEY] as DataTable;
                    if (ldap != null)
                        return ldap;

                    //System.Diagnostics.Debug.WriteLine("GETADGROUPUSERS()");

                    ldap = SelectADGroupUsers(false);

                    System.Web.HttpRuntime.Cache.Insert(
                        CACHE_ADGROUPUSERS_KEY,
                        ldap,
                        null,
                        DateTime.Now.Add(CACHE_ADGROUPUSERS_EXPIRATION),
                        System.Web.Caching.Cache.NoSlidingExpiration);
                }
            }

            return ldap;
        }
        public string[] GetADGroupUsers(string ADGroup)
        {
            DataTable ut = GetADGroupUsers();
            var users = ut.AsEnumerable()
                .Where(r => r.Field<string>(LdapDAL.KEY_ADGROUP).Equals(ADGroup, StringComparison.InvariantCultureIgnoreCase))
                .Select(r => r.Field<string>(ProjectPage.KEY_EMPLOYEEID));
            return (users.Count() > 0) ? users.ToArray() : null;
        }
        public string GetADGroupEmail(string ADGroup)
        {
            return GetADRootGroups().AsEnumerable()
                .Where(r => r.Field<string>(KEY_ADGROUP).Equals(ADGroup, StringComparison.InvariantCultureIgnoreCase))
                .Select(r => r.Field<string>(KEY_ADEMAIL))
                .ToList()
                .First();
        }
        public string GetADGroupManager(string ADGroup)
        {
            return GetADRootGroups().AsEnumerable()
                .Where(r => r.Field<string>(KEY_ADGROUP).Equals(ADGroup, StringComparison.InvariantCultureIgnoreCase))
                .Select(r => r.Field<string>(KEY_MANAGEDBYID))
                .ToList()
                .First();
        }

        public bool IsEmployeeInADGroup(string employeeId, string ADGroup)
        {
            if (String.IsNullOrWhiteSpace(employeeId) || String.IsNullOrWhiteSpace(ADGroup))
                return false;

            DataTable table = GetADGroupUsers();

            return table.AsEnumerable()
                .Where(r 
                    => r.Field<string>(ProjectPage.KEY_EMPLOYEEID).Equals(employeeId, StringComparison.InvariantCultureIgnoreCase)
                    && r.Field<string>(KEY_ADGROUP).Equals(ADGroup, StringComparison.InvariantCultureIgnoreCase))
                    .Count() > 0;
        }

        #region LDAP to Afas Conversion

        public string GetEmployeeIdByNTAccount(NTAccount ntAccount)
        { 
            return GetEmployeeIdByNTAccount(ntAccount.Value);
        }
        public string GetEmployeeIdByNTAccount(string ntAccount)
        {
            string domain, user;

            domain = ntAccount.Substring(0, ntAccount.IndexOf("\\"));
            user = ntAccount.Substring(ntAccount.IndexOf("\\") + 1);
            return GetEmployeeIdByNTAccount(domain, user);
        }
        public string GetEmployeeIdByNTAccount(string domain, string user)
        {

            DataTable current = GetEmployeeCurrentTable();
            var emp = from record in current.AsEnumerable()
                      where String.Equals(record.Field<string>(LdapDAL.KEY_ADDOMAIN), domain, StringComparison.OrdinalIgnoreCase)
                          && String.Equals(record.Field<string>(LdapDAL.KEY_ADACCOUNT), user, StringComparison.OrdinalIgnoreCase)
                      select record.Field<string>(ProjectPage.KEY_EMPLOYEEID);
            if (emp.ToList().Count == 0)
                throw new IdentityNotMappedException(string.Format("The ActiveDirectory user {0}\\{1} is not known in Afas Profit.", domain, user));

            return emp.ToList().First();
        }
        public NTAccount GetNTAccountByEmployeeId(string employeeId)
        {
            const string sql = "SELECT TOP 1 domain, account FROM [Get-EmployeeAccount] WHERE lower(employeeId) = @ID";

            NTAccount id = null;

            using (var conn = CreateConnectionSqlDAL())
            {
                conn.Open();
                using (var cmd = CreateCommand(sql, conn))
                {
                    cmd.Parameters.Add(NewSqlParam("ID", employeeId.ToLower()));

                    System.Data.Common.DbDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    if (dr.Read())
                    {
                        id = new NTAccount((dr[LdapDAL.KEY_ADDOMAIN] as string) + "\\" + (dr[LdapDAL.KEY_ADACCOUNT] as string));
                    }
                }
            }
            return id;
        }

        public DataTable GetEmployeeTableFromWIN32Account(string[] accounts, bool removeUnknownUsers)
        {
            DataTable result = new DataTable("SecretEmployees");
            result.Columns.AddRange(new DataColumn[] {
                new DataColumn(ProjectPage.KEY_EMPLOYEEID, typeof(System.String)),
                new DataColumn(ProjectPage.KEY_EMPLOYEENAME, typeof(System.String)),
                new DataColumn(LdapDAL.KEY_ADACCOUNT, typeof(System.String))
            });

            DataTable data = GetEmployeeCurrentTable();
            foreach (string account in accounts)
            {
                string domain = account.Substring(0, account.IndexOf('\\'));
                string user = account.Substring(domain.Length + 1);

                var employee = (from r in data.AsEnumerable()
                                where String.Equals(r.Field<string>(LdapDAL.KEY_ADDOMAIN), domain, StringComparison.InvariantCultureIgnoreCase)
                                 && String.Equals(r.Field<string>(LdapDAL.KEY_ADACCOUNT), user, StringComparison.InvariantCultureIgnoreCase)
                                select r);
                if (employee.Count() > 0)
                {
                    DataRow row = employee.First();
                    result.Rows.Add(new Object[] { row.Field<string>("employeeId"), row.Field<string>("fullName"), account });
                }
                else
                {
                    if (!removeUnknownUsers)
                        result.Rows.Add(new Object[] { "", "Onbekend", account });
                }
            }

            return result;
        }


        #endregion

    }
}