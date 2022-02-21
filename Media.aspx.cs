using Saved.Code;
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
using static Saved.Code.Fastly;

namespace Saved
{
    public partial class Media : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                 CoerceUser(Session);


            string sSave = Request.Form["btnSaveComment"].ToNonNullString();
            string mediaid = Request.QueryString["mediaid"] ?? "";
            double dReward = GetDouble(Request.QueryString["reward"].ToNonNullString());

            if (sSave != "" && mediaid.Length > 1)
            {
                if (gUser(this).UserName == "")
                {
                    MsgBox("Nick Name must be populated", "Sorry, you must have a username to save a tweet reply.  Please navigate to Account Settings | Edit to set your UserName.", this);
                    return;
                }
                string sql = "Insert into Comments (id,added,userid,body,parentid) values (newid(), getdate(), @userid, @body, @parentid)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId);
                command.Parameters.AddWithValue("@body", Request.Form["txtComment"]);
                command.Parameters.AddWithValue("@parentid", mediaid);
                gData.ExecCmd(command);
            }
            double dWatching = GetDouble(Request.QueryString["watching"].ToNonNullString());

            // Respect the category (Current, Historical, Rapture, Guest-Pastor, etc).
            //string id = Request.QueryString["id"].ToNonNullString();
            if (dReward == 1 && mediaid != "" && false)
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


                if (DataOps.GetHPS(gUser(this).RandomXBBPAddress.ToNonNullString()) < 1 && !Debugger.IsAttached)
                {
                    MsgBox("Hash Power too low", "Sorry, you must be mining in the leaderboard to use this feature.  Your HPS is too low.", this);
                    return;
                }

                if (mediaid != "" && dWatching == 1 && dReward == 1)
                {
                    string sql1 = "Update Tip set Watching=getdate(),watchcount=watchcount+1 where videoid='" + mediaid + "' and userid='" + gUser(this).UserId.ToString() + "' and starttime > getdate()-1";
                    gData.Exec(sql1);
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
            string sTitle = "<h3>" + vData[0] + "</h3>";

            for (int i = 1; i < vData.Length; i++)
            {
                if (vData[i] != "")
                {
                    sTitle += vData[i] + "<br>";
                }
            }
            sTitle = sTitle.Replace("\r\n", "<br>");
            return sTitle;

        }

        public string GetUserName(string userid)
        {
            if (userid == "")
                return "";
            string sql = "Select * from USERS where id = '" + BMS.PurifySQL(userid,100) + "'";
            string username = gData.GetScalarString2(sql, "username");
            return username;
        }

        public string GetMediaCategory()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            if (category == null || category == "")
                return "";
            category = category.Substring(0, 1).ToUpper() + category.Substring(1, category.Length - 1);
            return category;
        }
        public string GetMedia()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            string mediaid = Request.QueryString["mediaid"].ToNonNullString();
            double dWatching = GetDouble(Request.QueryString["watching"].ToNonNullString());
            double dReward = GetDouble(Request.QueryString["reward"].ToNonNullString());
            double dLimited = GetDouble(Request.QueryString["limit"].ToNonNullString());

            if (dWatching == 1)
                return "";


            string sql = "Select * from Rapture where category=@cat order by added desc";
            if (mediaid != "")
            {
                sql = "Update Rapture set ViewCount=isnull(viewcount,0)+1 where id='" + BMS.PurifySQL(mediaid, 100) + "'";
                gData.Exec(sql);
                sql = "Select * from Rapture Where ID='" + Saved.Code.BMS.PurifySQL(mediaid, 64) + "'";
            }
            string html = "";
           
            if (dReward == 1)
            {

                 sql = "Select top 1 * from Rapture where id=@id order by added desc";
                 SqlCommand c = new SqlCommand(sql);
                 c.Parameters.AddWithValue("@id", mediaid);
                 string sVideoID = gData.GetScalarString(c, "id");
                 string sCategory = gData.GetScalarString(c, "category");
                 string sURL = gData.GetScalarString(c, "url");
                
                 if (sVideoID.Length > 1)
                 {
                    string sSuffix = "?token=" + SignVideoURL();
                    string sFullURL = sURL + sSuffix;
                    double nReward = GetDouble(GetBMSConfigurationKeyValue("VideoRewardAmount"));
                    double dSize = Saved.Code.BMS.GetWebResourceSize(sFullURL);
                     string sql1 = "Insert into Tip (id,userid,amount,added,videoid,starttime,size,category,watchcount) values (newid(),'" + gUser(this).UserId.ToString() 
                        + "','" + nReward.ToString() + "',getdate(),'" + sVideoID + "',getdate(),'" + dSize.ToString() + "','" + sCategory + "',0)";
                     gData.Exec(sql1);
                     html += "<br>Congratulations!  You will be rewarded up to " + nReward.ToString() 
                        + " for watching this video.  Please do not click away until you gain something from the video, otherwise you will not receive the full reward.  <br><br> ";
                 }
            }

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@cat", category);
            command.Parameters.AddWithValue("@id", mediaid);

            DataTable dt = gData.GetDataTable(command);
            
            
            bool fAdmin = gUser(this).Admin;
            bool fRelink = dt.Rows.Count > 1;

            string sTable = "<table cellpadding=25px cellspacing=25px>";
            html += sTable;
            double nLimit = 0;
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                nLimit++;
                if (dLimited > 0 && nLimit > dLimited)
                    break;
                string sNotes = GetNotesHTML(dt.Rows[y]["Notes"].ToString());
                if (sNotes.Length > 256) 
                    sNotes = sNotes.Substring(0, 256);
                string sEditURL = "<a href=Markup.aspx?type=Rapture&id=" + dt.Rows[y]["id"].ToString() + ">Edit</a>";

                string sAnchor = "<div><a href=Media.aspx?mediaid=" + dt.Rows[y]["id"].ToString() + ">";
                string sUserID = dt.Rows[y]["userid"].ToString();
                string sUserName = GetUserName(sUserID);
                double nViewCount = GetDouble(dt.Rows[y]["ViewCount"].ToNonNullString());

                string sWidth = dt.Rows.Count == 1 ? "100%" : "40%";
                string sDiv = "<tr><td width='" + sWidth + "'>";

                if (fRelink)
                    sDiv += sAnchor;

                string sAutoPlay = !fRelink ? "autostart autoplay controls playsinline" : "preload='metadata'";

                string sDims = dt.Rows.Count == 1 ? "width='1000' height='768'" : "width='400' height='240'";

                sDiv += "<video id='video1' class='connect-bg' " + sDims + " " + sAutoPlay + " style='background-color:black'>";

                string sLoc = !fRelink ? "" : "#t=7";
                string sBaseURL = dt.Rows[y]["URL"].ToString();
                string sSuffix = "?token=" + SignVideoURL();

                string sFullURL = sBaseURL + sSuffix + sLoc;
                string sSpeed1 = "<a id='aSlow' href='#' onclick='slowPlaySpeed();'>.5x</a>";
                string sSpeed2 = "<a id='aNormal' href='#' onclick='normalPlaySpeed();'>1x</a>";
                string sSpeed3 = "<a id='aFast' href='#' onclick='fastPlaySpeed();'>1.75x</a>";


                // Add the token
                sDiv += "<source src='" + sFullURL + "' type='video/mp4'></video>";
                if (fRelink)
                    sDiv += "</a></div>";

                string sFooter = sSpeed1 + " • " + sSpeed2 + " • " + sSpeed3 + " • " + nViewCount.ToString() + " view(s) • " + dt.Rows[y]["Added"].ToString();
                if (sUserName != "")
                    sFooter += " • Uploaded by " + sUserName;
                if (dt.Rows.Count == 1)
                {
                    sDiv += "</td></tr><tr>";
                }
                sDiv += "<td style='padding:10px;font-size:14px;' width=70%>" + sNotes + "<br><small>" + sFooter + "</small><br><br></td>";
                
                
                if (fAdmin)
                    sDiv += "<td>" + sEditURL + "</td>";

                sDiv += "</tr>";
                html += sDiv;

            }
            html += "</table>";
            if (mediaid != "")
            {
                html += UICommon.GetComments(mediaid, this);
            }
            return html;
        }
    }
}