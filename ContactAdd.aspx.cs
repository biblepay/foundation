using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class ContactAdd : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSave_Click(object sender, EventArgs e)
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
            // Look for duplicate
            string sql = "select count(*) ct from Contact where firstname=@firstname and lastname=@lastname";

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@firstname", txtFirstName.Text);
            command.Parameters.AddWithValue("@lastname", txtLastName.Text);

            double dCt = gData.GetScalarDouble(command, "ct");
            if (dCt > 0)
            {
                MsgBox("Already Used", "Sorry, this contact is already taken, please try a different one.", this);
                return;
            }

            sql = "Insert into Contact (id,firstname,lastname,emailaddress, added, updated, userid) values (newid(),@firstname,@lastname,@emailaddress,getdate(),getdate(),@userid)";
            command = new SqlCommand(sql);

            command.Parameters.AddWithValue("@emailaddress", txtEmailAddress.Text);
            command.Parameters.AddWithValue("@firstname",  txtFirstName.Text);
            command.Parameters.AddWithValue("@lastname", txtLastName.Text);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId);

            try
            {
                gData.ExecCmd(command, false, false, true);
            }catch(Exception ex)
            {
                MsgBox("Error", "Unable to add the record.", this);
                return;
            }

            MsgBox("Added", "Success!  Prayers and blessings in converting this person.  <a href=Dashboard.aspx>Click here to return to your dashboard.</a>", this);

        }
    }
}