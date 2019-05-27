using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace JohamWeb.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:RadioButtonListEx runat=server></{0}:RadioButtonListEx>")]
    public class RadioButtonListEx : System.Web.UI.WebControls.RadioButtonList
    {

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public override string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            ListItemCollection items = this.Items;
            int count = items.Count;
            if (count <= 0)
                return;
            bool flag = false;
            for (int index = 0; index < count; ++index)
            {
                ListItem listItem = items[index];
                if (listItem.Enabled)
                {
                    string id = string.Format("{0}_{1}", this.UniqueID, index);

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "list-group-item");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "radio");
                    writer.RenderBeginTag(HtmlTextWriterTag.Label);

                    if (this.TextAlign == System.Web.UI.WebControls.TextAlign.Left)
                        HttpUtility.HtmlEncode(listItem.Text, writer);

                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, listItem.Value, true);
                    if (this.Enabled == false)
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

                    //if (listItem.Attributes.Count > 0)
                    //    listItem.Attributes.Render(writer);
                    if (listItem.Selected)
                    {
                        if (flag)
                            this.VerifyMultiSelect();
                        flag = true;
                        writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag(); // input

                    if (this.TextAlign != System.Web.UI.WebControls.TextAlign.Left)
                        HttpUtility.HtmlEncode(listItem.Text, writer);

                    writer.RenderEndTag(); // label

                    writer.RenderEndTag(); // div class=list-group-item

                    if (this.Page != null)
                        this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, listItem.Value);
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (String.IsNullOrWhiteSpace(this.CssClass))
                this.CssClass = "list-group";
            this.RenderBeginTag(writer);
            this.RenderContents(writer);
            this.RenderEndTag(writer);
        }
    }
}
