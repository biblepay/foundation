using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;

namespace Saved
{
    public partial class BlockData : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            double nHeight = Common.GetDouble(Request.QueryString["height"] ?? "");
            string sResult = Code.Uplink.GetBlockData((int)nHeight);
            Response.Write(sResult);
            Response.End();
        }
    }
}