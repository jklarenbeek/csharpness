using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.Util;
using System.Web.UI.WebControls;
using System.Reflection;
using AttributeCollection = System.Web.UI.AttributeCollection;


namespace JohamWeb.Controls
{
    [DefaultProperty("Text")]
    [ValidationProperty("Text")]
    [ControlValueProperty("Text", "19750117")]
    [DefaultEvent("SelectionChanged")]
    [DataBindingHandler("System.Web.UI.Design.WebControls.CalendarDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Designer("System.Web.UI.Design.WebControls.CalendarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [SupportsEventValidation]
    [ToolboxData("<{0}:CalendarSingleDate runat=server></{0}:CalendarSingleDate>")]
    public class CalendarSingleDate : System.Web.UI.WebControls.Calendar
    {
        private static readonly object EventDayRender;

        static CalendarSingleDate()
        {
            // force init
            new System.Web.UI.WebControls.Calendar();

            // now use reflection to get the key to the DayRenderEventHandler
            var field = typeof(System.Web.UI.WebControls.Calendar).GetField("EventDayRender", BindingFlags.NonPublic | BindingFlags.Static);
            var value = field.GetValue(null);
            EventDayRender = value;

        }
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                if (this.SelectedDate == DateTime.MinValue.Date)
                    return null;
                else
                    return this.SelectedDate.ToString("yyyyMMdd");
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    this.SelectedDate = DateTime.MinValue.Date;
                else
                    this.SelectedDate = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
        }

        private readonly DateTime baseDate = new DateTime(2000, 1, 1); // derived from microsoft source code

        protected override void OnDayRender(TableCell cell, CalendarDay day)
        {
            DayRenderEventHandler handler = (DayRenderEventHandler)Events[EventDayRender];
            if (handler != null)
            {
                if (day.Date == SelectedDate)
                {
                    cell.AddCssClass("selected");
                }

                int absoluteDay = day.Date.Subtract(baseDate).Days;

                // VSWhidbey 215383: We return null for selectUrl if a control is not in
                // the page control tree.
                string selectUrl = null;
                Page page = Page;
                if (page != null)
                {
                    string eventArgument = absoluteDay.ToString(CultureInfo.InvariantCulture);
                    selectUrl = Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true);
                }
                handler(this, new DayRenderEventArgs(cell, day, selectUrl));
            }
        }
        /*
        protected override void OnDayRender(TableCell c, CalendarDay d)
        {
            if (h_color != Color.Empty)
            {
                c.Attributes["onmouseover"] = "this.style.backgroundColor='" + String.Format("#{0:x2}{1:x2}{2:x2}", this.h_color.R, this.h_color.G, this.h_color.B) +
"';";
                if (this.DayStyle.BackColor != Color.Empty)
                    c.Attributes["onmouseout"] = "this.style.backgroundColor='" + String.Format("#{0:x2}{1:x2}{2:x2}", this.DayStyle.BackColor.R,
this.DayStyle.BackColor.G, this.DayStyle.BackColor.B) + "';";
                else
                    c.Attributes["onmouseout"] = "this.style.backgroundColor='';";
            }
            base.OnDayRender(c, d);
        }
        */
    }
}
