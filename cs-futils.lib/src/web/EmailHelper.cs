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
using System.Configuration;
using System.Web;

namespace joham.cs_futils.web
{
    public class EmailMessage
    {
        #region static functions

        public static readonly string EMAIL_BEHEER = ConfigurationManager.AppSettings["EMAIL_BEHEER"];

        public static Dictionary<string, string> ParsePropertyKeys(Dictionary<string, string> properties)
        {
            if (properties == null || properties.Count <= 3)
                throw new ArgumentNullException("The properties argument is not well formed.");

            Dictionary<string, string> replacements = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string k in properties.Keys)
            {
                string value = properties[k];

                if (k.StartsWith("{") == false)
                {
                    replacements.Add(("{" + k.ToUpper() + "}"), value);
                }
                else
                {
                    replacements.Add(k.ToUpper(), value);
                }
            }
            return replacements;
        }
        public static DataTable ParsePropertyKeys(DataTable properties)
        {
            if (properties.Rows.Count == 0)
                return properties;

            DataTable p = properties.AsEnumerable().CopyToDataTable();
            p.TableName = properties.TableName + "_PARSED";
            foreach(DataRow r in p.AsEnumerable())
            {
                string key = r.Field<string>(RigoDAL.TL_KEY_KEY).ToUpper();
                r[RigoDAL.TL_KEY_KEY] = key.StartsWith("{")?key:string.Format("{0}{1}{2}", '{', key, '}');
            }
            p.AcceptChanges();
            return p;

        }

        public static string parseTemplate(string text, Dictionary<string, string> properties, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("The text template argument is not well formed.");

            // normalise property keys
            properties = ParsePropertyKeys(properties);

            // build template
            StringBuilder result = new StringBuilder();

            int pos = 0;
            int esc = -2;
            int srt = -2;
            int tail = 0;
            string key = null;

            while (pos < text.Length)
            {
                char c = text[pos];
                switch(c)
                { 
                    case '\\':
                    {
                        esc = pos;
                        break;
                    }
                    case '{':
                    {
                        if (esc == (pos - 1))
                            break;
                        if (srt > 0)
                            errors.Add(string.Format("Nesting of opening {0} tags not allowed at {1}.", '{', pos));

                        result.Append(text.Substring(tail, pos - tail));

                        srt = tail = pos;
                        key = null;
                        break;
                    }
                    case '}':
                    {
                        if (esc == (pos - 1))
                            break;
                        if (srt < 0)
                        {
                            errors.Add(string.Format("The closing {0} tag was found without the opening {1} tag at {2}.", '}', '{', pos));
                            result.Append(text.Substring(tail, pos - tail));
                            break;
                        }

                        key = text.Substring(srt, pos - srt + 1);
                        if (properties.ContainsKey(key))
                            result.Append(properties[key]);
                        else
                            errors.Add(string.Format("The property list did not contain key {0} at {1}", key, srt));

                        srt = -2;
                        tail = pos+1;
                        break;
                    }
                    default: 
                        break;
                }
                pos++;
            }

            result.Append(text.Substring(tail, pos - tail));

            if (errors.Count == 0)
                errors = null;

            return result.ToString();

        }

        public static string loadTemplate(string templatePath)
        {
            return File.ReadAllText(templatePath);
        }

        public static string ToRawAddress(string address)
        {
            if (address == null || String.IsNullOrWhiteSpace(address))
                return null;

            int f = address.IndexOf('<');
            int l = address.LastIndexOf('>');
            if (f < 0 || l < 0)
                return address;

            string raw = address.Substring(f + 1, l - (f + 1));
            return raw;
        }

        #endregion

        private readonly string subject;
        private readonly string body;
        private readonly List<string> errors = null;

        public EmailMessage(string subject, string body, List<string> errors)
        {
            if (errors == null) throw new ArgumentNullException("errors");
            if (String.IsNullOrWhiteSpace(body)) throw new ArgumentNullException("body");
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");

            this.subject = subject;
            this.body = body;
            this.errors = errors; 
        }

        public string Send(string aan, Dictionary<string, string> replacements)
        {
            string from = ConfigurationManager.AppSettings["SMTP_FROM"];
            if (String.IsNullOrWhiteSpace(from))
                from = "Projectenadministratie <post@rigo.nl>";

            // save to email
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(from);
            foreach (string addr in aan.Split(';'))
            {
                msg.To.Add(new MailAddress(addr));
            }
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;
            if (replacements == null)
            {
                msg.Subject = this.subject;
                msg.Body = this.body;
            }
            else
            {
                msg.Subject = EmailMessage.parseTemplate(this.subject, replacements, this.errors);
                msg.Body = EmailMessage.parseTemplate(this.body, replacements, this.errors);
            }

            SmtpClient clnt = new SmtpClient();

            string host = ConfigurationManager.AppSettings["SMTP_SERVER"];
            if (!String.IsNullOrWhiteSpace(host))
            {
                clnt.Host = host;
                string port = ConfigurationManager.AppSettings["SMTP_PORT"];
                if (!String.IsNullOrWhiteSpace(port))
                    clnt.Port = Int32.Parse(port);
            }
            clnt.Send(msg);

            return aan;
        }

        /*
        public static string Send(string aan, string subject, string body, Dictionary<string, string> replacements, System.Web.HttpPostedFile postedFile)
        {

            EmailMessage self = new EmailMessage(subject, body, replacements);
            {
                // save to email
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("Projectenadministratie <admin@rigo.nl>");
                foreach (string addr in aan.Split(';'))
                {
                    msg.To.Add(new MailAddress(addr));
                }
                msg.Priority = MailPriority.High;

                msg.IsBodyHtml = true;
                if (self.replacements == null)
                {
                    msg.Subject = self.subject;
                    msg.Body = self.body;
                }
                else
                {
                    msg.Subject = EmailMessage.parseTemplate(self.subject, self.replacements);
                    msg.Body = EmailMessage.parseTemplate(self.body, self.replacements);
                }

                if (postedFile.ContentLength > 0)
                {
                    msg.Attachments.Add(new Attachment(
                            postedFile.InputStream, 
                            Path.GetFileName(postedFile.FileName), 
                            postedFile.ContentType
                        )
                    );
                }

                SmtpClient clnt = new SmtpClient();
                clnt.Host = ConfigurationManager.AppSettings["SMTP_SERVER"];
                clnt.Send(msg);

                return aan;
            }
        }
         */

    }

    public static class Message
    {
        private static StringBuilder builder = new StringBuilder();

        private static bool isNotify = false;
        private static bool isWarn = false;
        private static bool isError = false;

        private static void ResetStream()
        {
            builder.Clear();
            isNotify = isWarn = isError = false;

        }

        private static void BuildMessage(string caption, string message, object lines)
        {
            builder.AppendLine("<div>");
            builder.AppendLine(string.Format("<h5>{0} {1}</h5>", DateTime.Now, caption));
            builder.AppendLine(string.Format("<p>{0}</p>", message));

            if (lines != null && lines != DBNull.Value)
            {
                builder.AppendLine("<pre><code>");
                if (lines is string)
                {
                    builder.AppendLine(string.Format("\t{0}", lines));
                }
                else
                {
                    var enumerable = lines as IEnumerable;
                    if (enumerable != null)
                        foreach (object line in enumerable)
                            builder.AppendLine(string.Format("\t{0}", line.ToString()));
                }
                builder.AppendLine("</code></pre>");
            }

            builder.AppendLine("</div>");

        }

        private static void SaveToLogFile()
        {
            if ((isNotify | isWarn | isError) == false)
                return;

            // save to log file.
            string logfile = Path.Combine(
                ProjectFolder.PROJECT_LOGPATH, 
                string.Format("RigoPM-{0}.htm", DateTime.Now.ToString("MM-dd-yy")));

#if DEBUG
            logfile = Path.Combine(
                ProjectFolder.PROJECT_LOGPATH,
                string.Format("DebugPM-{0}.htm", DateTime.Now.ToString("MM-dd-yy")));
#endif

            if (!File.Exists(logfile))
                File.Create(logfile).Close();

            File.AppendAllText(logfile, builder.ToString());

        }

        private static void SendEmail()
        {
            if (isError == false)
                return;

            List<string> html = new List<string>();
            html.Add("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><HTML>");
            html.Add("<HEAD><META content=\"MSHTML 6.00.2900.3020\" name=GENERATOR></HEAD>");
            html.Add("<BODY>");
            html.Add("<H3>Mail Stream from Project Management</H3>");


            html.Add(builder.ToString());

            html.Add("</BODY>");
            html.Add("</HTML>");

            string from = ConfigurationManager.AppSettings["SMTP_FROM"];
            if (String.IsNullOrWhiteSpace(from))
                from = "Projectenadministratie <post@rigo.nl>";

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(from);
            msg.To.Add(new MailAddress(EmailMessage.EMAIL_BEHEER));

            try
            {
                RigoDAL dal = new RigoDAL();
                string employeeId = dal.GetEmployeeIdByNTAccount(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                msg.To.Add(new MailAddress(dal.GetEmployeeEmailAccount(employeeId)));
            }
            catch (Exception e) { }

            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;
            msg.Subject = "Project Management Debug Message";
            msg.Body = String.Join("", html);

            SmtpClient clnt = new SmtpClient();

            string host = ConfigurationManager.AppSettings["SMTP_SERVER"];
            if (!String.IsNullOrWhiteSpace(host))
            {
                clnt.Host = host;
                string port = ConfigurationManager.AppSettings["SMTP_PORT"];
                if (!String.IsNullOrWhiteSpace(port))
                    clnt.Port = Int32.Parse(port);
            }

            clnt.Send(msg);
        }

        public static void Notify(string message, object lines)
        {
            isNotify = true;

            BuildMessage("Notify", message, lines);

            SaveToLogFile();

            ResetStream();
        }

        public static void Warn(string message, object lines)
        {
            isWarn = true;

            BuildMessage("Warning", message, lines);

            SaveToLogFile();

            ResetStream();
        }

        public static void Error(string message, object lines)
        {
            isError = true;

            BuildMessage("Error", message, lines);

            SaveToLogFile();

            SendEmail();

            ResetStream();

        }

    }
}
