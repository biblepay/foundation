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
    public partial class RequestVideo : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!gUser(this).LoggedIn || gUser(this).UserId == "" || gUser(this).UserId == null)
            {
                MsgBox("Logged Out", "Sorry, you must be logged in to use this feature.", this);
                return;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
              string sql = "Insert into RequestVideo (id,body,url,added,userid) values (newid(),@body,@url,getdate(),'" + gUser(this).UserId.ToString() + "')";
              SqlCommand command = new SqlCommand(sql);
              command.Parameters.AddWithValue("@body", "");
              command.Parameters.AddWithValue("@url", txtURL.Text);
              gData.ExecCmd(command);
              MsgBox("Success", "Your video request will be processed ASAP. <br><br> Thank you!  <br>Please check back <a href=Media?category=Miscellaneous>here</a> in about 1 hour to find the video.", this);
        }
    }
}