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
    public partial class Leaderboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string RenderControlToHtml(Control ControlToRender)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter stWriter = new System.IO.StringWriter(sb);
            System.Web.UI.HtmlTextWriter htmlWriter = new System.Web.UI.HtmlTextWriter(stWriter);
            ControlToRender.RenderControl(htmlWriter);
            return sb.ToString();
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
           
        }

        private string GetTd(DataRow dr, string colname, string sAnchor)
        {
            string val = dr[colname].ToString();
            string td = "<td>" + sAnchor + val + "</a></td>";
            return td;
        }

        protected string GetLeaderboard()
        {
            int nHeight = Common.GetHeight();
            string sql = "Select * from Leaderboard order by bbpaddress";
            //bbpaddress, shares, fails, sucXMR, FailXMR, SuxXMRC, FailXMRC, Updated, Height
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>BBP Address</th><th>BBP Shares<th>BBP Invalid<th>XMR Shares<th>XMR Charity Shares<th>Efficiency<th>Hash Rate<th>Updated<th>Height</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                double xmrfails = GetDouble(dt.Rows[y]["FailXMR"]);
                string quizid = dt.Rows[y]["quizid"].ToString() ?? "";
                string bbpaddress = dt.Rows[y]["bbpaddress"].ToString() ?? "";
                string sAnchor = "<a href='Quiz.aspx?id=" + quizid + "&bbpaddress=" + bbpaddress + "'>" + bbpaddress + "</a>";
                if (quizid == "")
                {
                    sAnchor = bbpaddress;
                }

                string div = "<tr><td>" + sAnchor
                    + "<td>" + dt.Rows[y]["shares"].ToString()
                    + "<td>" + dt.Rows[y]["fails"].ToString()
                    + "<td>" + dt.Rows[y]["bxmr"].ToString()
                    + "<td>" + dt.Rows[y]["bxmrc"].ToString()
                    + "<td>" + dt.Rows[y]["efficiency"].ToString() + "%"
                    + "<td>" + dt.Rows[y]["hashrate"].ToString() + " HPS"
                    + "<td>" + dt.Rows[y]["Updated"].ToString()
                    + "<td>" + dt.Rows[y]["Height"].ToString() + "</tr>";
                html += div + "\r\n";

            }
            // Check for quizzes
            html += "</table>";

            sql = "Select id,reward from Quiz where solved is null";
            dt = gData.GetDataTable(sql, false);
            if (dt.Rows.Count > 0)
            {
                string id = dt.Rows[0]["id"].ToString();
                double nReward = GetDouble(dt.Rows[0]["Reward"]);
                if (id != "" && nReward > 0)
                {
                    html += "<br><div><p><span><font color=red>A gospel quiz is available.  </font><small>To try to win, click on your Pool BBP Address.  The winner receives " 
                        + nReward.ToString() + " BBP.</small></span></div>";
                }
            }

            return html;
        }
    }
}