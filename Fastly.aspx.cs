using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;

namespace Saved
{
    public partial class Fastly : Page
    {

        public static string GetRequestBody()
        {
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string data = GetRequestBody();
            Saved.Code.Fastly.ProcessFastly(data);
        }
    }
}