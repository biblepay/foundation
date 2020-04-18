using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saved
{
    public partial class MessagePage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string GetMessagePage()
        {
            string sTitle = Session["MSGBOX_TITLE"].ToString();
            string sMessageBody = Session["MSGBOX_BODY"].ToString();
            string sHTML = "<br><h2>" + sTitle + "</h2><hr><br><br><p><span>" + sMessageBody + "</span>";
            return sHTML;
        }


    }
}