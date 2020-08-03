using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.StringExtension;

namespace Saved
{
    public partial class MessagePage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string GetMessagePage()
        {
            string sTitle = Request.QueryString["Title"] ?? "";
            string sMessageBody = Request.QueryString["Body"] ?? "";

            if (sTitle == "")
            {
                sTitle = Session["MSGBOX_TITLE"].ToNonNullString();
                sMessageBody = Session["MSGBOX_BODY"].ToNonNullString();
            }


            if (sTitle == "")
                sTitle = "An error has occurred.";
            if (sMessageBody == "")
                sMessageBody = "An error of unknown origin has occurred.";

            string sHTML = "<br><h2>" + sTitle + "</h2><hr><br><br><p><span>" + sMessageBody + "</span>";
            return sHTML;
        }


    }
}