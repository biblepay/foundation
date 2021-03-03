using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class UnchainedPayPerByte : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string GetPlayURL()
        {
            string sURL = "https://biblepay.global.ssl.fastly.net/PPB_10BBP/7001699791551.mp4";
            string sSuffix = "?token=" + Saved.Code.Fastly.SignVideoURL() + "#t=7";
            string sFullURL = sURL + sSuffix;
            return sFullURL;
        }

    }
}