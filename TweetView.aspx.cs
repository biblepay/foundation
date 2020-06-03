﻿using Saved.Code;
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
    public partial class TweetView : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string sSave = Request.Form["btnSaveComment"].ToNonNullString();
            string id = Request.QueryString["id"] ?? "";
            if (sSave != "")
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
                command.Parameters.AddWithValue("@parentid", id);
                gData.ExecCmd(command);
            }
        }

        public string GetTweet()
        {
            // Displays the prayer that the user clicked on from the web list.
            string id = Request.QueryString["id"] ?? "";
            if (id == "")
                return "N/A";
            string sql = "Select * from Tweet Inner Join Users on Users.ID = Tweet.UserID where Tweet.id = @id";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            if (dt.Rows.Count < 1)
            {
                MsgBox("Not Found", "We are unable to find this item.", this);
                return "";
            }
            SavedObject s = RowToObject(dt.Rows[0]);

            string sUserPic = GetAvatar(s.Props.Picture); 
            string sUserName = NotNull(s.Props.UserName);
            if (sUserName == "")
                sUserName = "N/A";
            string sBody = " <textarea style='width: 70%;' id=txtbody rows=25 cols=65>" + s.Props.Body + "</textarea>";

            string div = "<table style='padding:10px;' width=100%><tr><td>User:<td>"+ sUserPic+ "</tr>"
                +"<tr><td>User Name:<td><h2>" + sUserName + "</h2></tr>"
                +           "<tr><td>Added:<td>" + s.Props.Added.ToString()     + "</td></tr>"
                +                "<tr><td>Subject:<td>" + s.Props.Subject + "</td></tr>"
                +               "<tr><td>Body:<td colspan=2>" + sBody + "</td></tr></table>";
            div += GetComments();

            return div;
        }
        public string GetComments()
        {
            // Shows the comments section for the object.  Also shows the replies to the comments.
            string id = Request.QueryString["id"].ToString();
            string sql = "Select * from Comments Inner Join Users on Users.ID = Comments.UserID  where comments.ParentID = @id";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            string sHTML = "<div><h3>Comments:</h3><br>"
                +"<table style='padding:10px;' width=100% >"
                +"<tr><th>User<th>Added<th>Comment</tr>";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SavedObject s = RowToObject(dt.Rows[i]);

                string sUserPic = GetAvatar(s.Props.Picture);

                string sUserName = NotNull(s.Props.UserName);
                if (sUserName == "")
                    sUserName = "N/A";

                string div = "<tr><td>" + sUserPic  + "<br>"+ sUserName + "</br></td><td>"+ s.Props.Added.ToString() + "</td><td>"+ s.Props.Body
                    + "</td></tr>";
                sHTML += div;

            }
            sHTML += "</table><table width=100%><tr><th colspan=2><h2>Add a Comment:</h2></tr>";

            string sButtons = "<tr><td>Comment:</td><td><textarea id='txtComment' name='txtComment' rows=10  style='width: 70%;' cols=70></textarea><br><br><button id='btnSaveComment' name='btnSaveComment' value='Save'>Save Comment</button></tr>";

            sButtons += "</table></div>";

            sHTML += sButtons;
            return sHTML;
        }
    }
}