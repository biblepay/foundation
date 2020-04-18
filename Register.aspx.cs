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
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (true)
                throw new HttpException((int)501, "The variable was not found");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Verify the user does not exist first
            if (!IsEmailValid(txtEmailAddress.Text))
            {
                MsgBox("E-Mail Is Not Valid", "Sorry, this e-mail address is not valid.", this);
                return;
            }

            string sql = "Select count(*) ct from Users where EmailAddress = @email";

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@email", txtEmailAddress.Text);
            double dCt = gData.GetScalarDouble(command, "ct");
            if (dCt > 0)
            {
                MsgBox("Already Used", "Sorry, this username or email address is already taken, please try a different one.", this);
                return;
            }
            if (!IsPasswordStrong(txtPassword.Text))
            {
                MsgBox("Minimum Password Requirements Failed",
                    "Sorry, your password must meet these minimum guidelines: " + Common.GetPWNarr() + ".", this);
                return;
            }

            sql = "Insert into Users (id,emailaddress,passwordhash,username, added,updated) values (newid(),@email,@password,@username,getdate(),getdate())";
            
            command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@email", txtEmailAddress.Text);
            command.Parameters.AddWithValue("@password", Saved.Code.Common.GetSha256Hash(txtPassword.Text));
            command.Parameters.AddWithValue("@username", txtUserName.Text);
            gData.ExecCmd(command);
          
        }
    }
}