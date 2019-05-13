using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;


namespace joham.cs_futils.web
{
    public static class HtmlControlExtensions
    {
        public static void AddCssClass(this HtmlControl control, string cssClass)
        {
            string attr = control.Attributes["class"];

            List<string> classes;
            if (!string.IsNullOrWhiteSpace(attr))
            {
                classes = attr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!classes.Contains(cssClass))
                    classes.Add(cssClass);
            }
            else
            {
                classes = new List<string> { cssClass };
            }
            control.Attributes["class"] = string.Join(" ", classes.ToArray());
        }

        public static void RemoveCssClass(this HtmlControl control, string cssClass)
        {
            string attr = control.Attributes["class"];

            List<string> classes = new List<string>();
            if (!string.IsNullOrWhiteSpace(attr))
            {
                classes = attr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            classes.Remove(cssClass);
            control.Attributes["class"] = string.Join(" ", classes.ToArray());
        }
    }

}
