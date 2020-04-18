using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using static Saved.Code.Common;
using System.Data.SqlClient;
using System.Data;

namespace Saved
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["opt"] == "logout")
            {
                // Clear the values
                Session["CurrentUser"] = null;
                MsgBox("Log Out", "You have been logged out - have a wonderful day.", this);
                return;
            }
            else
            {
                txtUserName.Text = gUser(this).UserName;
                if (gUser(this).UserName == "Guest")
                {
                    txtUserName.Text = "";
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if ((gUser(this).UserId ?? "") == "")
            {
                MsgBox("Login Error", "Sorry, you must be logged in first to authenticate with 2FA. ", this);
                return;
            }
            bool fLogged = Login(gUser(this).UserName, txtPin.Text, Session, "");
            if (fLogged)
            {
                Response.Redirect("Default.aspx");
            }
            else
            {
                MsgBox("Invalid Credentials Entered", "Invalid 2FA pin entered. ", this);
            }
        }


        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect("Register.aspx");
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {

            string sql = "Select * from users where emailaddress=@email";
            SqlCommand command = new SqlCommand(sql);

            command.Parameters.AddWithValue("@email", txtUserName.Text);

            DataRow dr1 = gData.GetScalarRow(command);

            if (dr1 == null)
            {
                MsgBox("Not Found", "Sorry, we cant find this username to reset.", this);
                return;
            }

            string sEmail = dr1["EmailAddress"].ToString();
            string sUserName = dr1["UserName"].ToString();
            if (sUserName == "") sUserName = "Unknown";

            // Send a code to the user
            MailAddress r = new MailAddress("rob@saved.one", "BiblePay Team");
            MailAddress t = new MailAddress(sEmail, sUserName);
            MailMessage m = new MailMessage(r, t);
            m.Subject = "Password Reset Request";
            string sTempPassword = GetSha256Hash(System.DateTime.Now.ToString());

            m.Body = "Hello " + txtUserName.Text + ", <br><br>We have provided an alternate way to log into your account.  Please use this temporary password:  "
                + "<br>" + sTempPassword + ".  Then, after you log in you may change your password using Account Edit.<br><br>  Best Regards,<br>The BiblePay Team<br>";
            m.IsBodyHtml = true;

            sql = "update Users set TemporaryPassword=@tp where emailaddress=@ea";
            command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@ea", sEmail);
            command.Parameters.AddWithValue("@tp", sTempPassword);
            gData.ExecCmd(command);

            SendMail(m);
            MsgBox("Sent", "A temporary password has been sent to your e-mail address.  Thank you.", this);


        }



    }
}