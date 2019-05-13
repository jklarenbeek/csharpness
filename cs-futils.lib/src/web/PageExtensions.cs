using System.Web.UI;
using System.Web.UI.WebControls;

namespace joham.cs_futils.web {


    public static class PageExtensions
    {
        /// <summary>
        /// Can be called during the Page.PreInit stage to make child controls available.
        /// Needed when a master page is applied.
        /// </summary>
        /// <remarks>
        /// This is needed to fire the getter on the top level Master property, which in turn
        /// causes the ITemplates to be instantiated for the content placeholders, which
        /// in turn makes our controls accessible so that we can make the calls below.
        /// </remarks>
        public static void PrepareChildControlsDuringPreInit(this Page page)
        {
            // Walk up the master page chain and tickle the getter on each one
            if (page.Master != null)
            {
                MasterPage master = page.Master;
                while (master != null) master = master.Master;
            }
        }
    }
}