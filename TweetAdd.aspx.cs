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
    public partial class TweetAdd : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (true && Debugger.IsAttached)
                CoerceUser(Session);


            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in to save a tweet.", this);
                return;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string sql = "Insert Into Tweet (id,userid,added,subject,body) values (newid(),@userid,getdate(),@subject,@body)";

            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in to save a tweet.", this);
                return;
            }
            if (txtSubject.Text.Length < 5 || txtBody.Text.Length < 25)
            {
                MsgBox("Content Too Short", "Sorry, the content of the Body or the Subject must be longer.", this);
                return;
            }
            if (gUser(this).UserName == "")
            {
                MsgBox("Nick Name must be populated", "Sorry, you must have a username to add a tweet.  Please navigate to Account Settings | Edit to set your username.", this);
                return;
            }
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@subject", txtSubject.Text);
            command.Parameters.AddWithValue("@body", txtBody.Text);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId);

            gData.ExecCmd(command);
            Response.Redirect("TweetList.aspx");
        }
    }
}