using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/*
 * 
 *     [ValidationPropertyAttribute("FileName")]
 *   [System.ComponentModel.DefaultProperty("FileName")]
 */

namespace JohamWeb.Controls.Common
{
    public partial class SingleFileUpload : System.Web.UI.UserControl
    {

        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        public string ContentType 
        { 
            get 
            { 
                return AsyncFileUpload1.ContentType; 
            } 
        }
        public int ContentLength 
        { 
            get 
            { 
                return AsyncFileUpload1.PostedFile.ContentLength; 
            } 
        }

        public String FileName
        {
            get
            {
                if (AsyncFileUpload1.HasFile)
                    return AsyncFileUpload1.PostedFile.FileName;
                else
                    return String.Empty;
            } 
        }

        public HttpPostedFile PostedFile 
        { 
            get 
            {
                return AsyncFileUpload1.PostedFile;
            } 
        }

        public void ClearFiles()
        {
            AsyncFileUpload1.ClearAllFilesFromPersistedStore();
            txtFileUploadHelper.Text = "";
            pnlFileUpload1.Update();

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AsyncFileUpload1Validator.ValidationGroup = ValidationGroup;
            }
        }

        protected void txtFileName1_Click(object sender, EventArgs e)
        {
            ClearFiles();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            //const string jsid = @"$('#{0}')";
            //string jscmd = "";
            if (AsyncFileUpload1.HasFile)
            {
                AsyncFileUpload1.Style["display"] = "none";
                pnlFileList.Style["display"] = "block";
                txtFileName1.Text = FileName;
            }
            else
            {
                AsyncFileUpload1.Style["display"] = "block";
                pnlFileList.Style["display"] = "none";
            }
        }

    }
}