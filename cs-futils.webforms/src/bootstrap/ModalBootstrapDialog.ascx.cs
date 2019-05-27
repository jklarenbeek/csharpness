using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using JohamWeb;
//using System.Web.UI.Design;
using System.ComponentModel;

namespace JohamWeb.Controls.Common
{
    //[PersistChildren(false)]
    [ParseChildren(ChildrenAsProperties = true)]
    public partial class ModalBootstrapDialog : System.Web.UI.UserControl, IDialogControl
    {
        #region ITemplate properties

        private ITemplate m_HeaderTemplate;

        [Browsable(false)]
        [TemplateInstance(TemplateInstance.Single)]
        [TemplateContainer(typeof(ModalTemplateItem))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate HeaderTemplate
        {
            get { return m_HeaderTemplate; }
            set
            {
                m_HeaderTemplate = value;
            }
        }

        private ITemplate m_BodyTemplate;

        [Browsable(false)]
        [TemplateInstance(TemplateInstance.Single)]
        [TemplateContainer(typeof(ModalTemplateItem))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate BodyTemplate
        {
            get { return m_BodyTemplate; }
            set
            {
                m_BodyTemplate = value; 
            }
        }

        private ITemplate m_FooterTemplate;

        [Browsable(false)]
        [TemplateInstance(TemplateInstance.Single)]
        [TemplateContainer(typeof(ModalTemplateItem))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate FooterTemplate
        {
            get { return m_FooterTemplate; }
            set {
                m_FooterTemplate = value;
            }
        }

        [MergableProperty(false)]
        [DefaultValue(null)]
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public UpdatePanelTriggerCollection Triggers
        {
            get
            {
                return pnlItemBody.Triggers;
            }
        }
        #endregion
        public bool IsLargeDialog 
        { 
            get {
                return ViewState["bootstrap-modal-size"] == null
                    ? false
                    : (bool)ViewState["bootstrap-modal-size"];
            } 
            set { ViewState["bootstrap-modal-size"] = value; } 
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), 
                    string.Format("{0}_Move", ID), 
                    string.Format("$('#{0}').appendTo('form');", ID), 
                    true);

                Control control = sender as Control;
                System.Diagnostics.Debug.WriteLine(string.Format("Page_Load (PostBack:{0}, ASync:{1}): EventTarget:{2} EventArg:{3} sender:{4}",
                    IsPostBack, ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack,
                    Request.Form["__EVENTTARGET"],
                    Request.Form["__EVENTARGUMENT"],
                    control == null ? sender.GetType().ToString() : control.ClientID),
                    "ModalBootstrapDialog");


                //base.DataBind();
            }

        }

        public void OpenDialog(UpdatePanel panel, int empty)
        {
            if (panel == null)
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), //RegisterStartupScript
                    string.Format("{0}_Open", ID), 
                    string.Format("$('#{0}').modal();", ID), 
                    true);
            else
                ScriptManager.RegisterClientScriptBlock(panel.Page, panel.GetType(), //RegisterStartupScript
                    string.Format("{0}_Open", ID),
                    string.Format("$('#{0}').modal();", ID), //
                    true);

            //base.DataBind();

            Update();
        }

        public void CloseDialog(UpdatePanel panel, int empty)
        {
            if (panel == null)
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), // RegisterStartupScript
                    string.Format("{0}_Close", ID), 
                    string.Format("$('#{0}').modal('hide');", ID), 
                    true);
            else
                ScriptManager.RegisterClientScriptBlock(panel.Page, panel.GetType(), //RegisterStartupScript
                    string.Format("{0}_Close", ID),
                    string.Format("$('#{0}').modal('hide');", ID),
                    true);

            //base.DataBind();

            Update();
        }

        public void Update()
        {
            pnlItemHeader.Update();
            pnlItemBody.Update();
            pnlItemFooter.Update();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            ltrModalTag.Text = string.Format(ltrModalTag.Text, this.ID);
            ltrDialogTag.Text = string.Format(ltrDialogTag.Text, IsLargeDialog ? "modal-lg" : "");
        }

        protected void pnlItemBody_Init(object sender, EventArgs e)
        {
          
            if (m_BodyTemplate != null)
            {
                pnlItemBody.ContentTemplate = m_BodyTemplate;
                //Control body = new Control();
                //body.ID = "ModalBody";
                //m_BodyTemplate.InstantiateIn(body);
                //pnlItemBody.ContentTemplateContainer.Controls.Add(body);
                //hldBody.Controls.Add(body);
                //hldBody.Controls.Clear();
                //hldBody.Controls.Add(body);
            }

            Control control = sender as Control;
            System.Diagnostics.Debug.WriteLine(string.Format("pnlItemBody_Init (PostBack:{0}, ASync:{1}): EventTarget:{2} EventArg:{3} sender:{4}",
                IsPostBack, ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack,
                Request.Form["__EVENTTARGET"],
                Request.Form["__EVENTARGUMENT"],
                control == null ? sender.GetType().ToString() : control.ClientID),
                "ModalBootstrapDialog");

        }

        protected void pnlItemHeader_Init(object sender, EventArgs e)
        {
            if (m_HeaderTemplate != null)
            {
                pnlItemHeader.ContentTemplate = m_HeaderTemplate;
                //Control header = new Control();
                //header.ID = "ModalHeader";
                //m_HeaderTemplate.InstantiateIn(header);
                //pnlItemHeader.ContentTemplateContainer.Controls.Add(header);
                //hldHeader.Controls.Clear();
                //hldHeader.Controls.Add(header);
            }
        }

        protected void pnlItemFooter_Init(object sender, EventArgs e)
        {
            if (m_FooterTemplate != null)
            {
                pnlItemFooter.ContentTemplate = m_FooterTemplate;
                //Control foot = new Control();
                //foot.ID = "ModalFoot";
                //m_FooterTemplate.InstantiateIn(foot);
                //hldFooter.Controls.Clear();
                //hldFooter.Controls.Add(foot);
            }

        }


    }
}