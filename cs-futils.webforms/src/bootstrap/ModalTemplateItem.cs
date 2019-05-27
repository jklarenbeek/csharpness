using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace JohamWeb.Controls.Common
{
    public class ModalTemplateItem :
        Control, System.Web.UI.INamingContainer, IDataItemContainer
    {
        private object _CurrentDataItem;
        public ModalTemplateItem(object currentItem)
        {
            _CurrentDataItem = currentItem;
            //this.DataBind();

        }

        #region IDataItemContainer Members

        public object DataItem
        {
            get { return _CurrentDataItem; }
        }

        public int DataItemIndex
        {
            get
            {
                return 0; // throw new Exception("The method or operation is not implemented.");
            }
        }

        public int DisplayIndex
        {
            get
            {
                return 0;// throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }

}