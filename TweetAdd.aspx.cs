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
using static Saved.Code.DataOps;

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

        protected double GetTweetCost()
        {
            string sql = "select count(*) ct from users where verification='deliverable'";
            double dCost = gData.GetScalarDouble(sql, "ct");
            return dCost;
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string sql = "Insert Into Tweet (id,userid,added,subject,body) values (newid(),@userid,getdate(),@subject,@body)";

            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in to save a tweet.", this);
                return;
            }
            
            double dBalance = GetUserBalance(this);
            double dCost = GetTweetCost();
            if (dBalance < dCost)
            {
                MsgBox("Not Logged In", "Sorry, your balance must be greater than " + dCost.ToString() + " BBP to advertise a tweet.", this);
                return;

            }
            if (txtSubject.Text.Length < 4 || txtBody.Text.Length < 10)
            {
                MsgBox("Content Too Short", "Sorry, the content of the Body or the Subject must be longer.", this);
                return;
            }
            if (gUser(this).UserName == "")
            {
                MsgBox("Nick Name must be populated", "Sorry, you must have a username to add a tweet.  Please navigate to Account Settings | Edit to set your username.", this);
                return;
            }

            double nAmt = GetTweetCost();
            AdjBalance(nAmt * -1, gUser(this).UserId.ToString(), "Tweet Out [" + Left(txtSubject.Text, 100) + "]");

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@subject", txtSubject.Text);
            command.Parameters.AddWithValue("@body", txtBody.Text);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId);

            gData.ExecCmd(command);
            Response.Redirect("TweetList.aspx");
        }
    }
}