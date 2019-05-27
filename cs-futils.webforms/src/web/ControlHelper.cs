using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace joham.cs_futils.web {

  public class ControlHelper {

        public static int GetListItemIndexByValue(DropDownList ddl, string employeeId)
        {
            return GetListItemIndexByValue(ddl.Items, employeeId);
        }
        public static int GetListItemIndexByValue(ListItemCollection items, string employeeId)
        {
            return items.IndexOf(items.FindByValue(employeeId));
        }

        public static ListItemCollection ToListCollection(Dictionary<string, object> data, ListItem header, ListItem footer)
        {
            ListItemCollection list = new ListItemCollection();
            if (header != null)
                list.Add(header);
            foreach (string key in data.Keys)
            {
                list.Add(new ListItem(TextHelper.FormatString(data[key], true), key));
            }
            if (footer != null)
                list.Add(footer);
            return list;
        }
        public static ListItemCollection ToListCollection(Dictionary<string, object> data)
        {
            return ToListCollection(data, null, null);
        }


        public static void AddCssClass(IAttributeAccessor control, string cssClass)
        {
            List<string> classes;
            string htmlclass = control.GetAttribute("class");
            if (!string.IsNullOrWhiteSpace(htmlclass))
            {
                classes = htmlclass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!classes.Contains(cssClass))
                    classes.Add(cssClass);
            }
            else
            {
                classes = new List<string> { cssClass };
            }
            control.SetAttribute("class", string.Join(" ", classes.ToArray()));
        }

        public static void RemoveCssClass(IAttributeAccessor control, string cssClass)
        {
            List<string> classes = new List<string>();
            string htmlclass = control.GetAttribute("class");
            if (!string.IsNullOrWhiteSpace(htmlclass))
            {
                classes = htmlclass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            classes.Remove(cssClass);
            control.SetAttribute("class", string.Join(" ", classes.ToArray()));
        }

  }
}