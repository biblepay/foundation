using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class HowTo : Page
    {
        public string Title1 = "NA";
        public string Body = "NA";
        int StepNo = 0;
        string Guide = "";

        protected void LQS()
        {
            Guide = Request.QueryString["name"];
            StepNo = Convert.ToInt32(Request.QueryString["stepno"]);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            LQS();
            if (StepNo < 1)
                StepNo = 1;

            string sql = "Select * from Markup where name='" + Guide + "' and stepno='" + StepNo.ToString() + "'";
            DataRow dr = gData.GetScalarRow(sql);
            if (dr != null)
            {
                Title1 = dr["title"].ToString();
                Body = dr["body"].ToString();
            }
            if (StepNo == 1)
                btnPrevious.Visible = false;
            sql = "Select count(*) ct from Markup where name='" + Guide + "' and stepno='" + (StepNo + 1).ToString() + "'";
            double dCt = gData.GetScalarDouble(sql, "ct");
            if (dCt == 0)
                btnNext.Visible = false;

        }
        
        public string GetNextUrl()
        {
            StepNo++;
            string url = "HowTo.aspx?name=" + Guide + "&stepno=" + StepNo.ToString();
            return url;
        }
        protected void btnNext_Click(object sender, EventArgs e)
        {
            StepNo++;
            string url = "HowTo.aspx?name=" + Guide + "&stepno=" + StepNo.ToString();
            Response.Redirect(url);

        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            StepNo--;
            string url = "HowTo.aspx?name=" + Guide + "&stepno=" + StepNo.ToString();
            Response.Redirect(url);

        }
    }
}