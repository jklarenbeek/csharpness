using System;
using System.Web;

namespace joham.cs_futils.web {

  public static class HtmlHelper {
        public static string RenderUrlHref(string url, string text)
        {
            return string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", url, text);
        }
        public static string RenderUrlHref(string url)
        {
            return RenderUrlHref(url, url);
        }
        public static string RenderUrlHref(object url, object text)
        {
            string turl = url as string;
            if (String.IsNullOrWhiteSpace(turl))
                return "";

            string txt = text as string;
            if (String.IsNullOrWhiteSpace(txt))
                return RenderUrlHref(txt);
            else
                return RenderUrlHref(url, txt);
        }
        public static string RenderUrlHref(object url)
        {
            return RenderUrlHref(url, null);
        }

        public static string RenderEmailHref(string mailto, string text)
        {
            return string.Format(
                        "<a href=\"mailto:{0}\">{1}</a>",
                        EmailMessage.ToRawAddress(mailto),
                        HttpUtility.HtmlEncode(text));
        }
        public static string RenderEmailHref(string mailto)
        {
            return RenderEmailHref(mailto, mailto);
        }

        public static string MergeJScript(string firstScript, string secondScript)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(secondScript));

            if (!String.IsNullOrEmpty(firstScript))
            {
                // 
                return firstScript + secondScript;
            }
            else
            {
                if (secondScript.TrimStart().StartsWith("javascript:", StringComparison.Ordinal))
                {
                    return secondScript;
                }
                return "javascript:" + secondScript;
            }
        }


  }
}