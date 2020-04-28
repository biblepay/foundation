using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.StringExtension;

namespace Saved
{
    public partial class Media : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (false && Debugger.IsAttached)
                 CoerceUser(Session);

            // Respect the category (Current, Historical, Rapture, Guest-Pastor, etc).
            string id = Request.QueryString["id"].ToNonNullString();
            if (id.Length > 1)
            {
                if (!gUser(this).LoggedIn || gUser(this).UserId=="" || gUser(this).UserId == null)
                {
                    MsgBox("Logged Out", "Sorry, you must be logged in to use this feature.", this);
                    return;
                }

                string sql = "select count(*) ct from Tip where UserId=@userid and Added > getdate()-1";
                SqlCommand cmd = new SqlCommand(sql);
                cmd.Parameters.AddWithValue("@userid", gUser(this).UserId);
                double nCt = gData.GetScalarDouble(cmd, "ct");
                if (nCt > 0)
                {
                    MsgBox("Budget Depleted", "Sorry, the budget has been depleted for this video.", this);
                    return;
                }

            }

        }

        public string GetNotesHTML(string data)
        {
            string[] vData = data.Split("\r\n");
            if (vData.Length < 2) return data;
            string sTitle = "<h3>" + vData[0] + "</h3><br>";

            for (int i = 1; i < vData.Length; i++)
            {
                sTitle += vData[i] + "<br>";
            }
            sTitle = sTitle.Replace("\r\n", "<br>");
            return sTitle;

        }

        public string GetMedia()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            string id = Request.QueryString["id"].ToNonNullString();
            string sql = "Select * from Rapture where category=@cat order by added desc";
            string html = "";

            if (id.Length > 1)
            {
                 sql = "Select top 1 * from Rapture where id=@id order by added desc";
                SqlCommand c = new SqlCommand(sql);
                 c.Parameters.AddWithValue("@id", id);
                 string sVideoID = gData.GetScalarString(c, "id");
                 string sCategory = gData.GetScalarString(c, "category");
                 if (sVideoID.Length > 1)
                 {
                     double nReward = 250;
                     string sql1 = "Insert into Tip (id,userid,amount,added,videoid) values (newid(),'" + gUser(this).UserId.ToString() + "','" + nReward.ToString() + "',getdate(),'" + sVideoID + "')";
                     gData.Exec(sql1);
                     // Reward the user
                     AdjBalance(nReward, gUser(this).UserId.ToString(), "Video Reward [" + sCategory  + "]");
                     html += "<br>Congratulations!  You will be rewarded " + nReward.ToString() + " for watching this video.  Please do not click away until you gain something from the video.  <br><br> ";
                 }
            }

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@cat", category);
            command.Parameters.AddWithValue("@id", id);

            DataTable dt = gData.GetDataTable(command);
            
            
            bool fAdmin = gUser(this).Admin;

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sNotes = GetNotesHTML(dt.Rows[y]["Notes"].ToString());
                if (sNotes.Length > 512) sNotes = sNotes.Substring(0, 512);
                string sEditURL = "<a href=Markup.aspx?type=Rapture&id=" + dt.Rows[y]["id"].ToString() + ">Edit</a>";
                string sDiv = "<table cellpadding=20px cellspacing=20px><tr><td><video class='connect-bg' width=400 height=340 controls preload='metadata' style='background-color:black'>"
                    + "<source src='"
                    + dt.Rows[y]["URL"].ToString()
                    + "#t=7' type='video/mp4'></video><td style='padding:20px;font-size:16px;'>&nbsp;&nbsp;" + sNotes + "</td>";
                if (fAdmin)
                    sDiv += "<td>" + sEditURL + "</td>";

                sDiv += "</table>";
                html += sDiv;

            }
            return html;
        }
    }
}