using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Web;
using static Saved.Code.StringExtension;

namespace Saved
{
    /// <summary>
    /// Summary description for Web
    /// </summary>
    public class Web : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string path = HttpContext.Current.Request.Url.AbsolutePath;
            string[] vPath = path.Split("/");
            string doc = vPath[vPath.Length - 1];
            string url = "https://ewr1.vultrobjects.com/san1/17e21f2b-77e1-4c69-a1cf-e0cbcdb92d8a.jpg";
            HttpContext.Current.Response.Redirect(url);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}