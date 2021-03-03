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
    public partial class UnchainedPayPerView : Page
    {
        protected string _cpk = "";
        protected string _mynickname = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            // Pull the user
            string sAgent = Request.Headers["User-Agent"].ToNonNullString();
            _cpk = ExtractXML(sAgent, "<key>", "</key>").ToString();
            if (WebServices.dicNicknames.ContainsKey(_cpk))
                _mynickname = WebServices.dicNicknames[_cpk];

            string mytest = "";

        }
        protected string GetPlayURL()
        {
            string sURL = "https://biblepay.global.ssl.fastly.net/PPV_99BBP/7001699791551.mp4";
            string sSuffix = "?token=" + Code.Fastly.SignVideoURL() + "#t=7";
            string sFullURL = sURL + sSuffix;
            return sFullURL;
        }

    }
}