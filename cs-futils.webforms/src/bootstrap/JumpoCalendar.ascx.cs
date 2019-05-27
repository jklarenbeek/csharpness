using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace JohamWeb.Controls.Common
{
    public partial class JumpoCalendar : System.Web.UI.UserControl
    {

        public string ValidationGroup
        {
            set { 
                cdrSingleDateValidator.ValidationGroup = value; 
            }
            get {
                return cdrSingleDateValidator.ValidationGroup; 
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //cdrSingleDateValidator.ValidationGroup = "texty";
            }
        }
        
        public DateTime VisibleDate
        {
            set { cdrSingleDate.VisibleDate = value; }
            get { return cdrSingleDate.VisibleDate; }
        }

        public DateTime SelectedDate
        {
            set { cdrSingleDate.SelectedDate = value; }
            get { return cdrSingleDate.SelectedDate; }
        }

        public bool Enabled
        {
            set { cdrSingleDateValidator.Enabled = cdrSingleDate.Enabled = value; }
            get { return cdrSingleDate.Enabled; }
        }

        protected void cdrSingleDateValidator_Validate(object sender, ServerValidateEventArgs e)
        {
            if (this.Visible == true && this.Enabled == true)
            {
                if (cdrSingleDate.SelectedDate >= DateTime.Now.AddDays(1))
                {
                    e.IsValid = true;
                }
                else
                {
                    e.IsValid = false;
                }
            }
            else
            {
                e.IsValid = true;
            }
        }


        protected void cdrSingleDate_DayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.Date < DateTime.Today || e.Day.Date.DayOfWeek == DayOfWeek.Saturday || e.Day.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                e.Day.IsSelectable = false;
            }
        }


    }
}