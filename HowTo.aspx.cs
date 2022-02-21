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

        DataTable GetDataTableMarkup(string sName, int nStep)
        {
            string sql = "Select * From MARKUP where name=@name and stepno=@stepno";
            SqlCommand command1 = new SqlCommand(sql);

            command1.Parameters.AddWithValue("@name", sName);
            command1.Parameters.AddWithValue("@stepno", nStep);
            DataTable dt = gData.GetDataTable(command1, false);
            return dt;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            LQS();
            if (StepNo < 1)
                StepNo = 1;


            DataTable dt = GetDataTableMarkup(Guide, StepNo);
            if (dt.Rows.Count > 0)
            {
                Title1 = dt.Rows[0]["title"].ToString();
                Body = dt.Rows[0]["body"].ToString();
            }
            if (StepNo == 1)
                btnPrevious.Visible = false;

            dt = GetDataTableMarkup(Guide, StepNo + 1);
            if (dt.Rows.Count < 1)
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