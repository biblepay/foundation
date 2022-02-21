using Saved.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Saved
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Saved.Code.PoolCommon.TallyBXMRC();
            Saved.Code.Common._pool = new Pool();
            if (Pool.fUseLocalXMR)
                Saved.Code.Common._xmrpool = new XMRPool();
        }

        void Application_Error(object sender, EventArgs e)
        {
            System.Web.UI.Page page = System.Web.HttpContext.Current.Handler as System.Web.UI.Page;

            bool fSessionExists = false;
            if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
            {
                fSessionExists = true;
            }


            Exception ex = Server.GetLastError().GetBaseException();
            bool fHandled = false;
            if (ex.Message.Contains("does not exist") && ex.Message.Contains("System.Web.UI.Util.CheckVirtualFileExists(VirtualPath"))
            {
                // 404
                fHandled = true;
            }
            else if (ex.Message.Contains("Invalid length for a Base-64 char array or string."))
            {
                fHandled = true;
            }
            else if (ex.Message.Contains("This is an invalid webresource"))
            {
                fHandled = true;
            }
            else if (ex.Message.Contains("A potentially dangerous Request.Path"))
            {
                fHandled = true;
            }
            else if (ex.Message.Contains("The file") && ex.Message.Contains("does not exist"))
            {
                fHandled = true;
            }

            Server.ClearError();
            string sNarr = "User: " + "\r\nGlobalExceptionHandler::Error Caught in Application_Error event" +
                            "Error in: " + Request.Url.ToString() + " \r\n" +
                            "Error Message: " + ex.Message.ToString() +
                            "Stack Trace:" + ex.StackTrace.ToString();

            if (!fHandled)
            {
            }

            BiblePayCommon.Common.Log2(Request.Url.ToString() + "\r\n" + sNarr);
            if (fSessionExists)
            {

                Session["MSGBOX_TITLE"] = "An exception occurred, but we see the problem!";
                Session["MSGBOX_BODY"] = "Sorry, an exception occurred while processing your request.  However, we were able to e-mail the team with the entire set of details leading up to the error.  Please accept our apologies and continue to enjoy our system.  <br><br>";
                Response.Redirect("MessagePage");
            }
            else
            {
                Response.Redirect("Illustrations");
            }
            //            UICommon.MsgBox("Error", "Sorry, an error occurred while performing this request.  I have e-mailed the support team with all of the details.   "                    +"We will open a ticket to work this problem.  ",  page);

        }

    }
}