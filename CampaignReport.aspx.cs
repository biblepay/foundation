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
    public partial class CampaignReport : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }



        public static string myhtml = "";
        protected string GetCampaignReport()
        {

            if (myhtml != "")
                return myhtml;

            string sql = "Select sum(amount) amt from Campaign (nolock)";
            double dAmt = gData.GetScalarDouble(sql, "amt");
            DataTable dt = gData.GetDataTable(sql);
            string html = "TOTAL REPORT GIVEAWAYS: 30,000,000 BBP\r\n\r\n";
            html += "<pre>Dash Address                                                                      Claim Date\r\n";
            html += "All DASH receive addresses that sent DASH between Block 1 and Block 1,395,000 (Dec 31, 2020) are being included!  This is 73,000,000 addresses!";
            html += "</pre>";
            myhtml = html;
            return html;
        }
    }
}