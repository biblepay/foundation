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
    public partial class ContactView : Page
    {
        private string _id = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            _id = Request.QueryString["id"] ?? "";
            if (_id != "" && IsPostBack==false)
            {
                string sql = "Select * from Contact where id = @id";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@id", _id);
                DataTable dt = gData.GetDataTable(command);
                txtFirstName.Text = NotNull(dt.Rows[0]["FirstName"]);
                txtLastName.Text = NotNull(dt.Rows[0]["LastName"]);
                txtEmailAddress.Text = NotNull(dt.Rows[0]["EmailAddress"]);
                ddlStatus.Text = NotNull(dt.Rows[0]["Status"]);
            }
            else if (_id == "")
            {
                MsgBox("Invalid Contact", "Sorry, invalid contact.", this);
                return;
            }
        }
        protected string GetHistoricalNotes()
        {
            string sql = "Select * from ContactNotes where parentid=@id order by added";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", _id);
            DataTable dt = gData.GetDataTable(command);
            string html = "<table style='width:100%;'><tr style='border-bottom:1px solid black;'><th>Added Date<th>Subject<th>Body</tr>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string row = "<tr><td width=17%>" + NotNull(dt.Rows[i]["Added"]) + "</td><td width=20%>" + NotNull(dt.Rows[i]["Subject"]) 
                    + "</td><td style='min-width:60%;'>" + NotNull(dt.Rows[i]["Body"]) 
                    + "</td></tr>";

                html += row;
            }
            html += "</table>";
            return html;
        }

        protected void btnSaveNotes_Click(object sender, EventArgs e)
        {
            if (txtNotesSubject.Text == "" || txtNotes.Text == "")
            {
                MsgBox("Salvation Notes Field Empty", "Sorry, both the salvation Notes Subject and the Salvation Notes must be populated.  Click the back button to continue. ", this);
                return;
            }

            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in first.", this);
                return;
            }

            string sql = "Insert into ContactNotes (id,subject,body,parentid,added) values (newid(),@subject,@body,@parentid,getdate())";
            SqlCommand command = new SqlCommand(sql);

            command.Parameters.AddWithValue("@subject", txtNotesSubject.Text);
            command.Parameters.AddWithValue("@body", txtNotes.Text);
            command.Parameters.AddWithValue("@parentid", _id);

            try
            {
                gData.ExecCmd(command, false, false, true);
            }
            catch(Exception ex)
            {
                MsgBox("Error", "Unable to save this Historical Note" + ex.Message, this);
            }
            Response.Redirect("ContactView.aspx?id=" + _id);
        }
            protected void btnUpdate_Click(object sender, EventArgs e)
        {
            // Verify the user does not exist first
            if (txtEmailAddress.Text != "" && !IsEmailValid(txtEmailAddress.Text))
            {
                MsgBox("E-Mail Is Not Valid", "Sorry, this e-mail address is not valid.", this);
                return;
            }

            if (txtFirstName.Text=="" && txtLastName.Text=="")
            {
                MsgBox("Name Empty", "Sorry, name must be populated.", this);
                return;
            }
            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in first.", this);
                return;

            }

            string sql = "Update Contact set emailaddress=@emailaddress, firstname=@firstname,status=@status,lastname=@lastname where id=@id";
            SqlCommand  command = new SqlCommand(sql);

            command.Parameters.AddWithValue("@emailaddress", txtEmailAddress.Text);
            command.Parameters.AddWithValue("@firstname",  txtFirstName.Text);
            command.Parameters.AddWithValue("@lastname", txtLastName.Text);
            command.Parameters.AddWithValue("@status", ddlStatus.Text);

            command.Parameters.AddWithValue("@id", _id);

            try
            {
                gData.ExecCmd(command, false, false, true);
            }
            catch(Exception ex)
            {
                MsgBox("Error", "Unable to Edit the record.", this);
                return;
            }
            Response.Redirect("dashboard.aspx");

        }
    }
}