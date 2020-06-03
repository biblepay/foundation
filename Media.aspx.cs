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
            if (true && Debugger.IsAttached)
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

                if (gUser(this).RandomXBBPAddress.ToNonNullString() == "")
                {
                    MsgBox("Value not populated", "Sorry, your RandomX BBP Address is not populated on your user account.  Please paste your RX mining address in your User Account first. ", this);
                    return;
                }

                double dWatching = GetDouble(Request.QueryString["watching"].ToNonNullString());
                if (dWatching == 1)
                {
                    string sql1 = "Update Tip set Watching=getdate() where videoid='" + id + "' and userid='" + gUser(this).UserId.ToString() + "' and starttime > getdate()-1";
                    gData.Exec(sql1);
                    return;
                }

                if (GetHPS(gUser(this).RandomXBBPAddress.ToNonNullString()) < 1 && !Debugger.IsAttached)
                {
                    MsgBox("Hash Power too low", "Sorry, you must be mining in the leaderboard to use this feature.  Your HPS is too low.", this);
                    return;
                }
                

                string sql = "select count(*) ct from Tip where UserId=@userid and Added > getdate()-1";
                SqlCommand cmd = new SqlCommand(sql);
                cmd.Parameters.AddWithValue("@userid", gUser(this).UserId);
                double nCt = gData.GetScalarDouble(cmd, "ct");
                if (nCt > 0 && !Debugger.IsAttached)
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
            string mediaid = Request.QueryString["mediaid"].ToNonNullString();
            double dWatching = GetDouble(Request.QueryString["watching"].ToNonNullString());
            if (dWatching == 1)
                return "";


            string sql = "Select * from Rapture where category=@cat order by added desc";
            if (mediaid != "")
                sql = "Select * from Rapture Where ID='" + Saved.Code.BMS.PurifySQL(mediaid, 64) + "'";

            string html = "";

            if (id.Length > 1)
            {
                 sql = "Select top 1 * from Rapture where id=@id order by added desc";
                 SqlCommand c = new SqlCommand(sql);
                 c.Parameters.AddWithValue("@id", id);
                 string sVideoID = gData.GetScalarString(c, "id");
                 string sCategory = gData.GetScalarString(c, "category");
                 string sURL = gData.GetScalarString(c, "url");

                 if (sVideoID.Length > 1)
                 {
                     double nReward = 250;
                     double dSize = Saved.Code.BMS.GetWebResourceSize(sURL);

                     string sql1 = "Insert into Tip (id,userid,amount,added,videoid,starttime,size) values (newid(),'" + gUser(this).UserId.ToString() + "','" + nReward.ToString() + "',getdate(),'" + sVideoID + "',getdate(),'" + dSize.ToString() + "')";
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
            bool fRelink = dt.Rows.Count > 1;
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sNotes = GetNotesHTML(dt.Rows[y]["Notes"].ToString());
                if (sNotes.Length > 512) sNotes = sNotes.Substring(0, 512);
                string sEditURL = "<a href=Markup.aspx?type=Rapture&id=" + dt.Rows[y]["id"].ToString() + ">Edit</a>";

                string sAnchor = "<div><a href=Media.aspx?mediaid=" + dt.Rows[y]["id"].ToString() + ">";

                string sDiv = "<table cellpadding=20px cellspacing=20px><tr><td>";

                if (fRelink)
                    sDiv += sAnchor;

                string sAutoPlay = !fRelink ? "autostart autoplay controls playsinline" : "preload='metadata'";

                sDiv += "<video class='connect-bg' width='400' height='340' " + sAutoPlay + " style='background-color:black'>";

                string sLoc = !fRelink ? "" : "#t=7";

                sDiv += "<source src='" + dt.Rows[y]["URL"].ToString() + sLoc + "' type='video/mp4'></video>";
                if (fRelink)
                    sDiv += "</a></div>";

                sDiv += "<td style='padding:20px;font-size:16px;'>&nbsp;&nbsp;" + sNotes + "</td>";
                
                
                if (fAdmin)
                    sDiv += "<td>" + sEditURL + "</td>";

                sDiv += "</tr></table>";
                html += sDiv;

            }
            return html;
        }
    }
}