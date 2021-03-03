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

namespace Saved
{
    public partial class TweetView : Page
    {

        private void MarkRead(string id, string userid)
        {
            if (userid == "" || userid==null)
                return;
            string sql = "Select count(*) ct from TweetRead where parentid = '" + id + "' and userid = '" + userid + "'";
            double dCt = gData.GetScalarDouble(sql, "ct");

            sql = "IF NOT EXISTS (SELECT ID FROM TweetRead where parentid='" + id + "' and userid = '" + userid
                + "') Insert into TweetRead (id,userid,added,parentid,ReadTime) values (newid(), '" + userid + "',getdate(),'" + id + "',getdate())";
            gData.Exec(sql);

            if (dCt == 0)
            {
                sql = "Select subject from Tweet where id='" + id + "'";
                string sSubject = gData.GetScalarString(sql, "subject");
                DataOps.AdjBalance(1, userid, "Tweet Read [" + sSubject + "]");
            }

            this.Session["Tweet" + id] = "1";

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            string sSave = Request.Form["btnSaveComment"].ToNonNullString();
            string id = Request.QueryString["id"] ?? "";
            if (sSave != "")
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("Logged Out", "Sorry, you must be logged in to add a comment.", this);
                    
                }

                if (gUser(this).UserName == "")
                {
                    MsgBox("Nick Name must be populated", "Sorry, you must have a username to save a tweet reply.  Please navigate to Account Settings | Edit to set your UserName.", this);
                    return;
                }
                string sql = "Insert into Comments (id,added,userid,body,parentid) values (newid(), getdate(), @userid, @body, @parentid)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId);
                command.Parameters.AddWithValue("@body", Request.Form["txtComment"]);
                command.Parameters.AddWithValue("@parentid", id);
                gData.ExecCmd(command);
            }
        }

        public string GetTweet()
        {
            // Displays the tweet that the user clicked on from the web list.
            string id = Request.QueryString["id"] ?? "";
            if (id == "")            
                return "N/A";
            MarkRead(id, gUser(this).UserId);

            string sql = "Select * from Tweet left Join Users on Users.ID = Tweet.UserID where Tweet.id = @id";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            if (dt.Rows.Count < 1)
            {
                MsgBox("Not Found", "We are unable to find this item.", this);
                return "";
            }
            SavedObject s = RowToObject(dt.Rows[0]);

            string sUserPic = DataOps.GetAvatar(s.Props.Picture); 
            string sUserName = NotNull(s.Props.UserName);
            if (sUserName == "")
                sUserName = "N/A";

            string sHTMLBody = ReplaceURLs(s.Props.Body);

            string sBody = "<div style='min-height:300px'><span style=''>" + sHTMLBody + "</span></div>";

            string div = "<table style='padding:10px;' width=73%><tr><td>User:<td>"+ sUserPic+ "</tr>"
                +"<tr><td>User Name:<td>" + sUserName + "</tr>"
                +           "<tr><td>Added:<td>" + s.Props.Added.ToString()     + "</td></tr>"
                +                "<tr><td>Subject:<td>" + s.Props.Subject + "</td></tr>"
                +               "<tr><td>&nbsp;</tr><tr><td width=8%>Body:<td style='border:1px solid lightgrey;min-height:300px' colspan=1 xbgcolor='grey' width=40%>" + sBody + "</td></tr></table>";
            div += UICommon.GetComments(id, this);

            return div;
        }
    }
}