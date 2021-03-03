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
    public partial class PrayerView : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string sSave = Request.Form["btnSaveComment"].ToNonNullString();
            string id = Request.QueryString["id"] ?? "";
            if (sSave != "")
            {

                if (!gUser(this).LoggedIn)
                {
                    MsgBox("Not Logged In", "Sorry, you must be logged in to save a prayer comment.", this);
                    return;
                }
                if (gUser(this).UserName == "")
                {
                    MsgBox("Nick Name must be populated", "Sorry, you must have a username to add a prayer.  Please navigate to Account Settings | Edit to set your UserName.", this);
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

        public string GetPrayer()
        {
            // Displays the prayer that the user clicked on from the web list.
            string id = Request.QueryString["id"] ?? "";
            if (id == "")
                return "N/A";
            string sql = "Select * from PrayerRequest Inner Join Users on Users.ID = PrayerRequest.UserID where prayerrequest.id = @id";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            if (dt.Rows.Count < 1)
            {
                MsgBox("Not Found", "We are unable to find this prayer.", this);
                return "";
            }
            SavedObject s = RowToObject(dt.Rows[0]);

            string sUserPic = DataOps.GetAvatar(s.Props.Picture); 
            string sUserName = NotNull(s.Props.UserName);
            if (sUserName == "")
                sUserName = "N/A";
            string sBody = " <textarea style='width: 70%;' id=txtbody rows=25 cols=65>" + s.Props.Body + "</textarea>";

            string div = "<table style='padding:10px;' width=100%><tr><td>User:<td>"+ sUserPic+ "</tr>"
                +"<tr><td>User Name:<td><h2>" + sUserName + "</h2></tr>"
                +           "<tr><td>Added:<td>" + s.Props.Added.ToString()     + "</td></tr>"
                +                "<tr><td>Subject:<td>" + s.Props.Subject + "</td></tr>"
                +               "<tr><td>Body:<td colspan=2>" + sBody + "</td></tr></table>";
            div += UICommon.GetComments(id,this);

            return div;
        }
    }
}