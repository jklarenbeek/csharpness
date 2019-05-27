using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;
using System.DirectoryServices.AccountManagement;

namespace joham.cs_futils.fs
{
    public class ProjectFolder 
    {
        public static readonly string PROJECT_LOGPATH = System.Configuration.ConfigurationManager.AppSettings["PROJECT_LOGPATH"];

        public static readonly string PROJECT_PATH_LOPEND = System.Configuration.ConfigurationManager.AppSettings["PROJECT_PATH_LOPEND"];
        public static readonly string PROJECT_PATH_AFGEMELD = System.Configuration.ConfigurationManager.AppSettings["PROJECT_PATH_AFGEMELD"];
        public static readonly string PROJECT_PATH_AFGEWEZEN = System.Configuration.ConfigurationManager.AppSettings["PROJECT_PATH_AFGEWEZEN"];
        public static readonly string PROJECT_PATH_AFGESLOTEN = System.Configuration.ConfigurationManager.AppSettings["PROJECT_PATH_AFGESLOTEN"];

        public static readonly string PROJECT_TEMPLATE_EXT = System.Configuration.ConfigurationManager.AppSettings["PROJECT_TEMPLATE_EXT"];
        public static readonly string PROJECT_TEMPLATE_INTP = System.Configuration.ConfigurationManager.AppSettings["PROJECT_TEMPLATE_INTP"];
        public static readonly string PROJECT_TEMPLATE_INTT = System.Configuration.ConfigurationManager.AppSettings["PROJECT_TEMPLATE_INTT"];
        public static readonly string PROJECT_TEMPLATE_INT = System.Configuration.ConfigurationManager.AppSettings["PROJECT_TEMPLATE_INT"];

        public static readonly string DESKTOP_STATUS_0 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_0"];
        public static readonly string DESKTOP_STATUS_1 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_1"];
        public static readonly string DESKTOP_STATUS_2 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_2"];
        public static readonly string DESKTOP_STATUS_3 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_3"];
        public static readonly string DESKTOP_STATUS_4 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_4"];
        public static readonly string DESKTOP_STATUS_5 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_5"];
        public static readonly string DESKTOP_STATUS_6 = System.Configuration.ConfigurationManager.AppSettings["DESKTOP_STATUS_6"];

        private const string PROJECT_MAP_VERTROUWELIJK = "Vertrouwelijk";
        private const string PROJECT_MAP_DATA = "Data";
        private const string PROJECT_MAP_OVERLEG = "Overleg";
        private const string PROJECT_MAP_ACHTERGROND = "Achtergrond";

        public static string GetProjectTemplatePath(Dictionary<string, string> project)
        {
            string groupId = project[ProjectPage.KEY_GROUPID];
            switch (groupId)
            {
                case "INTP": return PROJECT_TEMPLATE_INTP;
                case "INTT": return PROJECT_TEMPLATE_INTT;
                case "INT": return PROJECT_TEMPLATE_INT;
                default: return PROJECT_TEMPLATE_EXT;
            }
        }
 
        public static string formatProjectID(string id)
        {
            char[] cid = id.TrimStart().ToUpper().ToCharArray();
            int sp = 0, i = 0;
            if (cid[0] == 'P') sp = 1;
            else if (cid[1] == 'P') sp = 2;

            for (i = sp; i < id.Length; i++)
            {
                if ("0123456789".IndexOf(cid[i]) < 0)
                {
                    break;
                }
            }

            if (i - sp > 3)
            {
                return id.TrimStart().Substring(0, i).ToUpper();
            }

            return null;
        }

        private static string formatFileName(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            string name = fileName.Substring(path.Length, fileName.Length - path.Length).Trim();

            name.Replace("&", "en");
            name.Replace("%", "procent");

            name = Regex.Replace(name, "[^0123456789_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ]{1,}", "-");
            name = name.Substring(0, 2).ToUpper() + name.Substring(2);
            return Path.Combine(path, name.Substring(0, Math.Min(name.Length, 63)));
        }

        private static ProjectFolder CreateProjectFolder(string projectId, string projectName, string projectTemplate)
        {
            string id = formatProjectID(projectId);
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("projectId cannot be null");

            if (new FileSearch(PROJECT_PATH_LOPEND).Count(id + "*", false) > 0)
                throw new FileLoadException("project already exists");

            string name = String.IsNullOrWhiteSpace(projectName)?projectId.Substring(id.Length):projectName;
            string projectFolder = formatFileName(id + " " + name);

            ProjectFolder project = new ProjectFolder();
            project.m_projectId = id;
            project.m_projectName = name;
            project.m_projectDirectory = new DirectoryInfo(Path.Combine(PROJECT_PATH_LOPEND, projectFolder));

            FileSearch template = new FileSearch(projectTemplate);
            template.CopyTo(project.FullPath, true);

            project.m_fileSearch = new FileSearch(project.FullPath);

            Message.Notify(string.Format("Created project {0} at {1}", project.m_projectId, project.FullPath), null);

            return project;
        }

        public static ProjectFolder Create(string projectId, string projectName)
        {
            return CreateProjectFolder(projectId, projectName, PROJECT_TEMPLATE_EXT);
        }

        public static ProjectFolder CreateInternal(string projectId, string projectName)
        {
            return CreateProjectFolder(projectId, projectName, PROJECT_TEMPLATE_INTP);
        }

        public static ProjectFolder Load(string projectId)
        {

            projectId = formatProjectID(projectId);
            if (String.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException("projectId cannot be null or empty");

            FileSearch search = new FileSearch(PROJECT_PATH_LOPEND);
            List<string> folders = search.GetFiles(projectId + "*", false);
            if (folders.Count == 0)
                throw new FileLoadException(projectId + " cannot be found");

            if (folders.Count > 1)
            {
                throw new FileLoadException(projectId + " has two or more folders in project root with the same projectId");
            }

            ProjectFolder project = new ProjectFolder();
            project.m_projectDirectory = new DirectoryInfo(folders[0]);
            project.m_projectId = formatProjectID(project.FolderName);
            project.m_projectName = project.FolderName.Substring(project.m_projectId.Length);
            project.m_fileSearch = new FileSearch(project.FullPath);

            Message.Notify(string.Format("Loaded project {0} at {1}", project.m_projectId, project.FullPath), null);

            return project;
        }

        /************************************
     * 
     * 
     ************************************/
        #region instance members

        private JohamDAL m_dal = new JohamDAL();
        private Dictionary<string, string> m_details = null;

        private string m_projectId = null;
        private string m_projectName = null;
        private DirectoryInfo m_projectDirectory = null;
        private FileSearch m_fileSearch = null;
        private DirectoryInfo m_secretDirectory = null;

        public JohamDAL Dal
        {
            get { return this.m_dal; }
        }

        public Dictionary<string, string> Details
        {
            get
            {
                if (m_details == null)
                {
                    if (String.IsNullOrWhiteSpace(m_projectId) == false)
                        m_details = m_dal.GetProjectRegistration(this.m_projectId);
                }
                return m_details;
            }
        }
        
        public string ProjectId
        {
            get { return this.m_projectId; }
        }

        public string ProjectName
        {
            get { return this.m_projectName; }
        }

        public string FullPath
        {
            get { return this.m_projectDirectory.FullName; }
        }

        public string FolderName
        {
            get { return this.m_projectDirectory.Name; }
        }

        public DirectoryInfo ProjectDirectory
        {
            get { return m_projectDirectory; }
        }
        public DirectoryInfo SecretDirectory
        {
            get 
            {
                if (m_secretDirectory == null)
                {
                    int beperktCount = m_fileSearch.Count("Beperkt*", false);
                    int vertrouwelijkCount = m_fileSearch.Count("Vertrouwelijk*", false);

                    int count = beperktCount + vertrouwelijkCount;
                    if (count == 0)
                    {
                        Message.Warn(string.Format("De bewaakte map in {0} is niet aanwezig", m_fileSearch.SearchRoot), null);
                        return null;
                    }
                    else if (count > 1)
                    {
                        Message.Warn(string.Format("De bewaakte map in {0} is {1} keer aanwezig", m_fileSearch.SearchRoot, count), null);
                        return null;
                    }

                    if (beperktCount == 1)
                        m_secretDirectory = new DirectoryInfo(m_fileSearch.GetFiles("Beperkt*", false)[0]);
                    else
                        m_secretDirectory = new DirectoryInfo(m_fileSearch.GetFiles("Vertrouwelijk*", false)[0]);
                }
                return m_secretDirectory;
            }
        }

        #endregion

        #region Convert Project Folder

        private void UpdateStatusIcon(ProjectState state)
        {
            if (String.IsNullOrWhiteSpace(m_projectId))
                return;

            string iconpath = null;
            switch (state)
            {
                case ProjectState.NewProject:
                case ProjectState.Open:
                    iconpath = DESKTOP_STATUS_1; 
                    break;
                //case ProjectPage.PROJECT_STATUS_AFMELDEN: iconpath = DESKTOP_STATUS_0; break;
                case ProjectState.Proposal: 
                    iconpath = DESKTOP_STATUS_2; 
                    break;
                //case ProjectPage.PROJECT_STATUS_AFGEWEZEN: iconpath = DESKTOP_STATUS_3; break;
                case ProjectState.Accepted: 
                    iconpath = DESKTOP_STATUS_4; 
                    break;
                case ProjectState.WrapUp: 
                    iconpath = DESKTOP_STATUS_5; 
                    break;
                //case ProjectPage.PROJECT_STATUS_AFGESLOTEN: iconpath = DESKTOP_STATUS_6; break;
                default: 
                    break;
            }


            string desktop = Path.Combine(this.FullPath, "desktop.ini");
            FileInfo di = new FileInfo(desktop);
            if (di.Exists == true)
            {
                di.Attributes = FileAttributes.Archive;
                di.Delete();
            }

            if (iconpath != null)
            {
                DirectoryInfo info = new DirectoryInfo(this.FullPath);
                SecurityIdentifier user = WindowsIdentity.GetCurrent().User;

                FileSearch.AddAccessFullControl(info, user);

                File.Copy(iconpath, desktop, true);
                (new FileInfo(desktop)).Attributes = FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden;

                FileSearch.RemoveAccessFullControl(info, user);
            }
        }
        public void UpdateStatusIcon()
        {
            if (this.Details == null)
                UpdateStatusIcon(ProjectState.NewProject);
            else
                UpdateStatusIcon(ProjectStateUtil.ParseState(this.Details[ProjectPage.KEY_STATUSID]));
        }            


        public bool Refresh()
        {
            // reset project directory tree
            FileSearch.ElevatePermissions();

            List<string> secretusers = GetSecretUsers();
            if (m_fileSearch.RemoveAllPermissions(true) == false)
                throw new UnauthorizedAccessException(string.Format("De permissies van project {0} kunnen niet worden verwijdert!", FullPath));

            // add mandatory file access rules
            FileSearch.AddAccessFullControl(m_projectDirectory, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
            FileSearch.AddAccessFullControl(m_projectDirectory, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
            FileSearch.AddAccessFullControl(m_projectDirectory, WindowsIdentity.GetCurrent().User);

            // Now copy filepermissions of project-template over to the project itself.
            Dictionary<string, string> project = this.Details;
            FileSearch template = new FileSearch(GetProjectTemplatePath(project));
            template.CopyPermissions(null, this.ProjectDirectory.FullName, null, false);

            if (this.SecretDirectory != null)
                template.CopyPermissions(PROJECT_MAP_VERTROUWELIJK, this.SecretDirectory.FullName, null, false);

            secretusers = Dal.GetEmployeeTableFromWIN32Account(secretusers.ToArray(), true)
                .AsEnumerable()
                .Select(r => r.Field<string>(JohamDAL.KEY_ADACCOUNT))
                .ToList();

            // move the security back in to the secret folder.
            foreach (string user in secretusers)
            {
                AddSecretUser(new NTAccount(user));
            }

            // add the project leader too, if he/she wasn't already there.
            NTAccount leader = Dal.GetNTAccountByEmployeeId(project[ProjectPage.KEY_LEADERID]);
            if (leader != null)
            {
                AddSecretUser(new NTAccount(leader.Value));
            }

            return true;
        }

        public bool IsConvertable
        {
            get { return !this.m_projectName.StartsWith("-"); }
        }

        /***
         * returns: null on success or string with error message
         ***/
        public string CheckProjectRootPermissions()
        {
            string error = "Vernieuw de permissies van uw project folder";
            return error;
        }
        public string RepairProjectRootPermissions()
        {
            string error = null;
            try
            {
                FileSearch.ElevatePermissions();
                if (m_fileSearch.RemoveAllPermissions(false) == false)
                    throw new UnauthorizedAccessException(string.Format("De permissies van project {0} kunnen niet worden verwijdert!", FullPath));

                // add mandatory file access rules
                FileSearch.AddAccessFullControl(ProjectDirectory, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
                FileSearch.AddAccessFullControl(ProjectDirectory, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                FileSearch.AddAccessFullControl(ProjectDirectory, WindowsIdentity.GetCurrent().User);

                FileSearch template = new FileSearch(GetProjectTemplatePath(this.Details));
                template.CopyPermissions(null, ProjectDirectory.FullName, null, false);

            }
            catch (Exception e)
            {
                error = string.Format("Het beveiligen van map {0} is mislukt.", this.FolderName);
                Message.Error(error, e.ToString());
            }
            finally
            {
                FileSearch.RemoveAccessFullControl(ProjectDirectory, WindowsIdentity.GetCurrent().User);
            }
            return error;
        }
        public string CheckProjectDisclosedPermissions()
        {
            string error = "Beveilig uw map 'Vertrouwelijk' opnieuw.";
            return error;
        }
        public string RepairProjectDisclosedPermissions()
        {
            string error = null;
            try
            {
                FileSearch.ElevatePermissions();
                List<string> secretusers = GetSecretUsers();
                FileSearch template = new FileSearch(GetProjectTemplatePath(this.Details));
                //template.CopyPermissions(SecretDirectory.Name, ProjectDirectory.FullName, null, false);
                
            }
            catch (Exception e)
            {
                error = string.Format("Het beveiligen van map {0} is mislukt.", this.FolderName);
                Message.Error(error, e.ToString());
            }
            finally
            {
                FileSearch.RemoveAccessFullControl(ProjectDirectory, WindowsIdentity.GetCurrent().User);
            }
            return error;


        }

        public bool Convert()
        {
            bool success = true;

            string sourcePath = this.FullPath;
            string targetPath = Path.Combine(PROJECT_PATH_LOPEND, formatFileName(sourcePath));

            try
            {
                Dictionary<string, string> project = this.Details;

                FileSearch.ElevatePermissions();

                List<string> secretusers = GetSecretUsers();
                if (m_fileSearch.RemoveAllPermissions(true) == false)
                    throw new UnauthorizedAccessException(string.Format("De permissies van project {0} kunnen niet worden verwijdert!", FullPath));

                // add mandatory file access rules
                FileSearch.AddAccessFullControl(m_projectDirectory, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
                FileSearch.AddAccessFullControl(m_projectDirectory, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                FileSearch.AddAccessFullControl(m_projectDirectory, WindowsIdentity.GetCurrent().User);

                // check if the secret folder has a new structure
                if (SecretDirectory != null && PROJECT_MAP_VERTROUWELIJK.Equals(
                        Path.GetFileNameWithoutExtension(SecretDirectory.FullName), StringComparison.InvariantCultureIgnoreCase
                    ) == false)
                {
                    string srcSecretPath = SecretDirectory.FullName;
                    string dstSecretPath = Path.Combine(Path.GetDirectoryName(srcSecretPath), PROJECT_MAP_VERTROUWELIJK);

                    Directory.Move(srcSecretPath, dstSecretPath);
                    m_secretDirectory = null;
                }

                ConvertFolder(m_fileSearch, "Achtergrond*", PROJECT_MAP_ACHTERGROND);
                ConvertFolder(m_fileSearch, "Overleg*", PROJECT_MAP_OVERLEG);
                ConvertFolder(m_fileSearch, "Plan van*", "Aanpak");
                ConvertFolder(m_fileSearch, "Website*", "Website");
                ConvertFolder(m_fileSearch, "Bron*", "Analyse");

                if (Path.Equals(sourcePath, targetPath) == false)
                {
                    // first we rename the folder and then create a junction with the old name to it.
                    Directory.Move(sourcePath, targetPath);

                    // create a reparsepoint with the old name to the new projectfolder and hide it
                    Junction.Create(sourcePath, targetPath, false);
                    (new DirectoryInfo(sourcePath)).Attributes |= FileAttributes.Hidden;
                }
                else
                {
                    targetPath = sourcePath;
                }

                // Now copy contents of project-template over to the project itself.
                FileSearch template = new FileSearch(GetProjectTemplatePath(project));
                template.CopyTo(targetPath, true);

                // and switch context
                this.m_fileSearch = new FileSearch(targetPath);
                this.m_projectDirectory = new DirectoryInfo(targetPath);
                this.m_projectName = this.FolderName.Substring(this.m_projectId.Length);
                this.m_secretDirectory = null;

                // now finally: copy all left over files in the root of the project folder to the data directory.
                ConvertData(project, this.m_fileSearch);

                secretusers = Dal.GetEmployeeTableFromWIN32Account(secretusers.ToArray(), true)
                    .AsEnumerable()
                    .Select(r => r.Field<string>(JohamDAL.KEY_ADACCOUNT))
                    .ToList();

                // move the security back in to the secret folder.
                foreach (string user in secretusers)
                {
                    AddSecretUser(new NTAccount(user));
                }

                // add the project leader too, if he/she wasn't already there.
                NTAccount leader = Dal.GetNTAccountByEmployeeId(project[ProjectPage.KEY_LEADERID]);
                if (leader != null)
                {
                    AddSecretUser(new NTAccount(leader.Value));
                }
            
            }
            catch (Exception e)
            {
                success = false;
                Message.Error(string.Format("Het converteren van map {0} is mislukt.", this.FolderName), e.ToString());
            }
            finally
            {
                FileSearch.RemoveAccessFullControl(new DirectoryInfo(sourcePath), WindowsIdentity.GetCurrent().User);
            }
            return success;
        }

        private static bool ConvertFolder(FileSearch search, string searchPattern, string targetFolder)
        {
            List<string> folders = search.GetFiles(searchPattern, false);
            if (folders.Count == 0)
                return false;

            string sourcePath = folders[0];
            string targetPath = Path.Combine(search.SearchRoot, targetFolder);

            try
            {
                if (folders.Count != 1)
                {
                    Message.Warn(string.Format("De map {0} in {1} is {2} keer aanwezig", searchPattern, search.SearchRoot, folders.Count), null);

                    long timestamp = DateTime.Now.Ticks;
                    string newpath = Path.Combine(search.SearchRoot, timestamp.ToString());
                    Directory.CreateDirectory(newpath);

                    foreach (string source in folders)
                    {
                        string target = Path.Combine(newpath, Path.GetFileName(source));
                        Directory.Move(source, target);
                        if (Path.GetFileName(source).Equals(targetFolder, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            Junction.Create(source, target, false);
                            // hide the reparsepoint from view
                            DirectoryInfo info = new DirectoryInfo(source);
                            info.Attributes |= FileAttributes.Hidden;

                        }
                    }

                    Directory.Move(newpath, Path.Combine(search.SearchRoot, targetFolder));
                }
                else
                {

                    if (Path.Equals(sourcePath, targetPath) == false)
                    {
                        Directory.Move(sourcePath, targetPath);
                        Junction.Create(sourcePath, targetPath, false);

                        // hide the reparsepoint for view
                        DirectoryInfo info = new DirectoryInfo(sourcePath);
                        info.Attributes |= FileAttributes.Hidden;

                    }
                }
            }
            catch (Exception e)
            {
                Message.Error(string.Format("De map in {0} kan niet worden geraadpleegd.", sourcePath), e.ToString());
                return false;
            }

            return true;
        }

        private class DataFolderComparer : IEqualityComparer<string>
        {
            public bool Equals(string left, string right)
            {
                if (Object.ReferenceEquals(left, right)) return true;

                if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
                    return false;

                string l = Path.GetFileName(left);
                string r = Path.GetFileName(right);
                return l.ToUpper() == r.ToUpper();
            }

            public int GetHashCode(string path)
            {
                if (Object.ReferenceEquals(path, null)) return 0;

                return Path.GetFileName(path).GetHashCode();
            }
        }

        private static bool ConvertData(Dictionary<string, string> project, FileSearch search)
        {
            string dataPath = Path.Combine(search.SearchRoot, PROJECT_MAP_DATA);
            if (Directory.Exists(dataPath) == false)
            {
                Message.Warn(String.Format("Data folder in {0} doesn't exist, creating new folder.", search.SearchRoot), null);
                Directory.CreateDirectory(dataPath);
            }

            try
            {
                //First we move all folders to the data folder.
                List<string> sourceFiles = search.GetFiles(FileAttributes.Directory, false);
                List<string> templateFiles = (new FileSearch(GetProjectTemplatePath(project))).GetFiles(FileAttributes.Directory, false);

                List<string> items = sourceFiles.Except(templateFiles, new DataFolderComparer()).ToList<string>();
                if (items.Count > 0)
                {
                    foreach (string folder in items)
                    {
                        if (Directory.Exists(Path.Combine(dataPath, Path.GetFileName(folder))) == false)
                        {
                            Message.Notify(String.Format("Moving folder {0} to {1}", Path.GetFileName(folder), dataPath), null);
                            string target = Path.Combine(dataPath, Path.GetFileName(folder));

                            DirectoryInfo info = new DirectoryInfo(folder);
                            FileAttributes attr = info.Attributes;
                            info.Attributes = (attr | FileAttributes.ReadOnly) ^ FileAttributes.ReadOnly;
                            info.Attributes = (attr | FileAttributes.System) ^ FileAttributes.System;
                            Directory.Move(folder, target);
                            info = new DirectoryInfo(target);
                            info.Attributes = attr;


                            Junction.Create(folder, target, false);
                            (new DirectoryInfo(folder)).Attributes |= FileAttributes.Hidden;
                        }
                    }
                }

                //First we move all folders to the data folder.
                sourceFiles = search.GetFiles(FileAttributes.Normal, false);
                templateFiles = (new FileSearch(GetProjectTemplatePath(project))).GetFiles(FileAttributes.Normal, false);

                items = sourceFiles.Except(templateFiles, new DataFolderComparer()).ToList<string>();
                if (items.Count > 0)
                {
                    foreach (string file in items)
                    {
                        string target = Path.Combine(dataPath, Path.GetFileName(file));
                        if (File.Exists(target) == false)
                        {
                            Message.Notify(String.Format("Moving file {0} to {1}", Path.GetFileName(file), dataPath), null);

                            FileInfo info = new FileInfo(file);
                            FileAttributes attr = info.Attributes;
                            info.Attributes = (attr | FileAttributes.ReadOnly) ^ FileAttributes.ReadOnly;
                            info.Attributes = (attr | FileAttributes.System) ^ FileAttributes.System;
                            File.Move(file, target);
                            info = new FileInfo(target);
                            info.Attributes = attr;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Message.Error(string.Format("Moving files and folder to data folder failed in {0}", dataPath), e.ToString());
                return false;
            }

            return true;
        }

        #endregion

        #region Archive Project Folder

        public string GetLongestPath()
        {
            return FileSearch.XGetLongestPath(FullPath, "*", FileAttributes.Normal | FileAttributes.Directory, true);
        }

        public bool Archive()
        {
            try
            {
                Dictionary<string, string> details = this.Details;

                ProjectState state = (details != null) ? ProjectStateUtil.ParseState(details[ProjectPage.KEY_STATUSID]) : ProjectState.NewProject;

                string targetPath = null;
                switch (state)
                {
                    case ProjectState.Cancelled:
                        targetPath = Path.Combine(PROJECT_PATH_AFGEMELD, formatFileName(this.FolderName));
                        break;
                    case ProjectState.Declined:
                        targetPath = Path.Combine(PROJECT_PATH_AFGEWEZEN, formatFileName(this.FolderName));
                        break;
                    case ProjectState.Closed:
                        targetPath = Path.Combine(PROJECT_PATH_AFGESLOTEN, formatFileName(this.FolderName));
                        break;
                    default: 
                        return false;
                }

                // reset project directory tree
                FileSearch.ElevatePermissions();
                if (m_fileSearch.RemoveAllPermissions(true) == false)
                    throw new UnauthorizedAccessException(string.Format("De permissies van project {0} kunnen niet worden verwijdert!", FullPath));

                // copy its content to the target archive path
                m_fileSearch.CopyTo(targetPath, true);

                // now switch context from the old to the new
                string removepath = this.FullPath;
                string removeprojectid = this.ProjectId;
                FileSearch removesearch = m_fileSearch;
                m_fileSearch = new FileSearch(targetPath);
                m_projectDirectory = new DirectoryInfo(targetPath);
                m_secretDirectory = null;

                // protect and reset security on secret folder
                if (SecretDirectory != null)
                {
                    SecretDirectory.Attributes |= FileAttributes.Hidden;
                    DirectorySecurity sd = SecretDirectory.GetAccessControl();
                    sd.SetAccessRuleProtection(true, false);
                    SecretDirectory.SetAccessControl(sd);

                    // add mandatory file access rules
                    FileSearch.AddAccessFullControl(SecretDirectory, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
                    FileSearch.AddAccessFullControl(SecretDirectory, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                    //FileSearch.AddAccessFullControl(SecretDirectory, WindowsIdentity.GetCurrent().User);
                }

                // now we are done and can actually hide the old project in the PROJECT_PATH_LOPEND folder.
                List<string> removefiles = new FileSearch(PROJECT_PATH_LOPEND).GetFiles(
                    string.Format("{0}*", removeprojectid), 
                    FileAttributes.ReparsePoint | FileAttributes.Directory, 
                    true);

                foreach (string file in removefiles)
                {
                    DirectoryInfo info = new DirectoryInfo(file);
                    if ((info.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        Junction.Delete(file);
                        continue;
                    }

                    // first rename them by a move
                    string destfile = Path.Combine(Path.GetDirectoryName(file), string.Format("_STATUS{0}_{1}", state.ToID(), Path.GetFileName(file)));
                    new DirectoryInfo(file).MoveTo(destfile);
                    
                    // reset and protect backup
                    info = new DirectoryInfo(destfile);
                    info.Attributes |= FileAttributes.Hidden;
                    DirectorySecurity sd = info.GetAccessControl();
                    sd.SetAccessRuleProtection(true, false);
                    info.SetAccessControl(sd);

                    // add mandatory file access rules
                    FileSearch.AddAccessFullControl(info, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null));
                    FileSearch.AddAccessFullControl(info, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                    FileSearch.AddAccessFullControl(info, WindowsIdentity.GetCurrent().User);
                }

                return true;
            }
            catch (Exception e)
            {
                Message.Error(string.Format("Het archiveren van map {0} is mislukt.", this.FolderName), e.ToString());
                return false;
            }
        }

        #endregion

public String domainName = "JOHAM";

        public List<string> GetSecretUsers()
        {
            
            List<string> accounts = FileSearch.GetFolderSecurity(this.SecretDirectory);
            return accounts.FindAll(account => account.StartsWith(domainName, StringComparison.OrdinalIgnoreCase));
        }
        public void AddSecretUser(NTAccount account)
        {
            try
            {
                FileSearch.ElevatePermissions();

                DirectoryInfo info = this.SecretDirectory;
                if (info != null)
                    FileSearch.AddAccessFullControl(info, account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier);
            }
            catch (Exception e)
            {
                Message.Error(string.Format("Kan gebruiker {0} niet aan vertrouwelijke map van project {1} toevoegen.", account.Value, this.m_projectId), e.ToString());
            }
        }
        public void RemoveSecretUser(NTAccount account)
        {
            try
            {
                FileSearch.ElevatePermissions();

                DirectoryInfo info = this.SecretDirectory;
                if (info != null)
                    FileSearch.RemoveAccessFullControl(info, account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier);
            }
            catch (Exception e)
            {
                Message.Error(string.Format("Kan gebruiker {0} niet van vertrouwelijke map van project {1} verwijderen.", account.Value, this.m_projectId), e.ToString());
            }

        }

    }
}
