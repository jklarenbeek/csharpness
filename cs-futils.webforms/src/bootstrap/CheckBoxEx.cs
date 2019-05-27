using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using AttributeCollection = System.Web.UI.AttributeCollection;

namespace JohamWeb.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:CheckBoxEx runat=server></{0}:CheckBoxEx>")]
    public class CheckBoxEx : System.Web.UI.WebControls.CheckBox
    {
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return (Inline)?HtmlTextWriterTag.Span : HtmlTextWriterTag.Div;
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

        public bool Inline
        {
            get { Object b = ViewState["Inline"]; return b != null; }
            set
            {
                if (value == false)
                    ViewState["Inline"] = null;
                else
                    ViewState["Inline"] = true;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            // Make sure we are in a form tag with runat=server.
            if (Page != null)
            {
                Page.VerifyRenderingInServerForm(this);
            }

            // On wrapper, render ---- attribute and class according to RenderingCompatibility
            if (!IsEnabled)
            {
                if (!Enabled && !String.IsNullOrEmpty(DisabledCssClass))
                {
                    if (String.IsNullOrEmpty(CssClass))
                    {
                        ControlStyle.CssClass = DisabledCssClass;
                    }
                    else
                    {
                        ControlStyle.CssClass = DisabledCssClass + " " + CssClass;
                    }
                }
            }

            // And Style
            if (ControlStyleCreated)
            {
                System.Web.UI.WebControls.Style controlStyle = ControlStyle;
                if (!controlStyle.IsEmpty)
                {
                    controlStyle.AddAttributesToRender(writer, this);
                }
            }
            // And ToolTip
            string toolTip = ToolTip;
            if (toolTip.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, toolTip);
            }

            string onClick = null;
            string value = null;

            // And other attributes
            if (HasAttributes)
            {
                AttributeCollection attribs = Attributes;

                // remove value from the attribute collection so it's not on the wrapper
                value = attribs["value"];
                if (value != null)
                    attribs.Remove("value");

                // remove and save onclick from the attribute collection so we can move it to the input tag
                onClick = attribs["onclick"];
                if (onClick != null)
                {
                    onClick = TextHelper.EnsureEndWithSemiColon(onClick);
                    attribs.Remove("onclick");
                }

                if (attribs.Count != 0)
                    attribs.AddAttributes(writer);

            }

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "checkbox");

            // we always render label, should this test on text instead?
            writer.RenderBeginTag(this.TagKey); // <div>
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            string text = Text;
            if (!String.IsNullOrWhiteSpace(text) && this.TextAlign == System.Web.UI.WebControls.TextAlign.Left)
                HttpUtility.HtmlEncode(text, writer);

            RenderInputTag(writer, ClientID, onClick, value);

            if (!String.IsNullOrWhiteSpace(text) && this.TextAlign != System.Web.UI.WebControls.TextAlign.Left)
                HttpUtility.HtmlEncode(text, writer);

            writer.RenderEndTag(); // </label>
            writer.RenderEndTag(); // </div>
        }

        protected virtual void RenderInputTag(HtmlTextWriter writer, string clientID, string onClick, string value)
        {
            if (clientID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");

            if (UniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            }

            // Whidbey 20815
            if (value != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            }

            if (Checked)
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");

            // ASURT 119141: Render ---- attribute on the INPUT tag (instead of the SPAN) so the checkbox actually gets disabled when Enabled=false
            if (!IsEnabled && SupportsDisabledAttribute)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            if (AutoPostBack && (Page != null) && /* Page.ClientSupportsJavaScript */ true)
            {

                PostBackOptions options = new PostBackOptions(this, String.Empty);

                if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0)
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = ValidationGroup;
                }

                if (Page.Form != null)
                {
                    options.AutoPostBack = true;
                }

                // ASURT 98368
                // Need to merge the autopostback script with the user script
                onClick = TextHelper.MergeJScript(onClick, Page.ClientScript.GetPostBackEventReference(options, true));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);

                if (/* EnableLegacyRendering */ false)
                {
                    //writer.AddAttribute("language", "javascript", false);
                }
            }
            else
            {
                if (Page != null)
                {
                    Page.ClientScript.RegisterForEventValidation(this.UniqueID);
                }

                if (onClick != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                }
            }

            string s = AccessKey;
            if (s.Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);

            int i = TabIndex;
            if (i != 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, i.ToString(NumberFormatInfo.InvariantInfo));

            if (InputAttributes != null && InputAttributes.Count != 0)
            {
                InputAttributes.AddAttributes(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

    }
}
