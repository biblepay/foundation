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
    public partial class Leaderboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

            double nQuizReward = GetDouble(GetBMSConfigurationKeyValue("quizreward"));
            if (nQuizReward == 1)
            {
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
            }

            double nVideoReward = GetDouble(GetBMSConfigurationKeyValue("videoreward"));

            if (nVideoReward == 1)
            {
                // Offer BBP to people who have 2fa and who haven't watched a video in a while:
                sql = "Select id, category, notes from Rapture";
                DataTable dt1 = gData.GetDataTable(sql, false);
                Random r = new Random();
                int rInt = r.Next(0, dt1.Rows.Count);
                int nGospelRow = (int)(r.NextDouble() * dt1.Rows.Count);
                if (nGospelRow <= dt1.Rows.Count)
                {
                    double nCt = 0;
                    if (gUser(this).LoggedIn)
                    {
                        sql = "select count(*) ct from Tip where UserId=@userid and added > getdate()-1";
                        SqlCommand cmd = new SqlCommand(sql);
                        cmd.Parameters.AddWithValue("@userid", gUser(this).UserId);
                        nCt = gData.GetScalarDouble(cmd, "ct");
                    }
                    if (nCt == 0)
                    {
                        string sCategory = dt1.Rows[nGospelRow]["category"].ToString();
                        string sName = dt1.Rows[nGospelRow]["Notes"].ToString();
                        string sId = dt1.Rows[nGospelRow]["id"].ToString();
                        double nAmt = GetDouble(GetBMSConfigurationKeyValue("VideoRewardAmount"));
                        string sAnchor = "<a href='Media?id=" + sId + "'>here</a>";
                        string sNarr = "A " + sCategory + " video is available.  Click " + sAnchor + " to earn a " + nAmt.ToString() + " BBP reward for watching the video.";
                        html += "<br><div><span>" + sNarr + "</span></div>";
                    }
                }
            }

            return html;
        }
    }
}