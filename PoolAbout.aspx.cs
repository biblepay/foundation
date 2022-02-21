using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class PoolAbout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        public string GetTR(string key, string value)
        {
            string tr = "<TR><TD width='55%'>" + key + ":</TD><TD>" + value + "</TD></TR>\r\n";
            return tr;
        }

        public string GetImgSource()
        {
            try
            {
                string sql = "Select * from Bio";
                DataTable dt = gData.GetDataTable2(sql, false);
                int nHour = (DateTime.Now.Hour+DateTime.Now.DayOfYear) % dt.Rows.Count;
                string url = dt.Rows[nHour]["URL"].ToString();
                return url;
            }
            catch(Exception)
            {
                return "https://i.ibb.co/W691XWC/Screen-Shot-2019-12-12-at-16-01-29.png";
            }
        }


        public string GetPoolAboutMetrics()
        {
            string html = "<table>";
            string sql = "Select sum(Hashrate) hr From Leaderboard";
            double dHR = gData.GetScalarDouble(sql, "hr");
            sql = "Select count(bbpaddress) ct from Leaderboard";
            double dCt = gData.GetScalarDouble(sql, "ct");
            html += GetTR("Miners", dCt.ToString());
            html += GetTR("Speed", UICommon.GetHPSLabel(dHR));

            html += GetTR("Charity Address", GetBMSConfigurationKeyValue("MoneroAddress"));
            html += GetTR("Contact E-Mail", GetBMSConfigurationKeyValue("OperatorEmailAddress"));
            html += GetTR("Pool Fees XMR", "1% (minexmr.com)");
            html += GetTR("Pool Fees BBP", Math.Round(GetDouble(GetBMSConfigurationKeyValue("PoolFee")) * 100, 2) + "%");
            html += GetTR("Block Bonus", Math.Round(GetDouble(GetBMSConfigurationKeyValue("PoolBlockBonus")), 0) + " BBP Per Block");
            
            html += GetTR("Build Version", PoolCommon.pool_version.ToString());
            html += GetTR("Startup Time", PoolCommon.start_date.ToString());

            html += GetTR("Height", PoolCommon.nGlobalHeight.ToString());
            html += GetTR("Job Count", PoolCommon.dictJobs.Count().ToString());
            html += GetTR("Worker Count", PoolCommon.dictWorker.Count().ToString());

            sql = "Select sum(shares) suc, sum(fails) fail from Share (nolock) where updated > getdate()-1";
            double ts24 = gData.GetScalarDouble(sql, "suc");
            double tis24 = gData.GetScalarDouble(sql, "fail");
            html += GetTR("Total Shares (24 hours)", ts24.ToString());
            html += GetTR("Total Invalid Shares (24 hours)", tis24.ToString());

            sql = "Select count(distinct height) h from Share (nolock) where updated > getdate()-1 and subsidy > 0 and reward > .05";
            double tbf24 = gData.GetScalarDouble(sql, "h");
            html += GetTR("Total Blocks Found (24 hours)", tbf24.ToString());

            html += "</table>";
            return html;
        }
        
    }
}