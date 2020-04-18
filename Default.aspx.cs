using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class _Default : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            string sso = Server.UrlDecode(Request.Unvalidated["sso"] ?? "");
            string ss1 = Request.Unvalidated["redir"] ?? "";

            if (ss1 == "0")
            {
                // This just prevents a perpetual redirect.
                return;
            }

            //Handle the SSO return call:
            if (sso.Length > 2)
            {
                string decsso = Common.decipherSSO(sso);
                string[] vData = decsso.Split("|");
                if (vData.Length >= 2)
                {
                    string un = vData[0];
                    string url = vData[1];

                    if (un == "Guest")
                    {
                        User u1 = new User();
                        u1.UserName = un;
                        u1.AvatarURL = "https://forum.biblepay.org/Themes/Offside/images/default-avatar.png";
                        u1.LoggedIn = false;
                        Session["CurrentUser"] = u1;
                    }
                    else if (un != "")
                    {
                        User u1 = new User();
                        u1.UserName = un;
                        u1.AvatarURL = url;
                        PersistUser(ref u1);
                        u1.LoggedIn = u1.Require2FA == 1 ? false : true;
                        Session["CurrentUser"] = u1;
                        string sTarget = u1.Require2FA == 1 ? "Login.aspx" : "Default.aspx";
                        Response.Redirect(sTarget);
                        return;
                    }
                    // Redirect the user to the home page.
                    Response.Redirect("Default.aspx");
                    return;
                }
            }
            else
            {
                Response.Redirect("Default.aspx?redir=0");
            }
        }
    }
}