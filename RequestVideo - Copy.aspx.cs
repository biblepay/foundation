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
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
               string body = txtBody.Text;
               string sql = "Insert into RequestVideo (id,body,url,added) values (newid(),@body,@url,getdate())";
               SqlCommand command = new SqlCommand(sql);
               command.Parameters.AddWithValue("@body", body);
               command.Parameters.AddWithValue("@url", txtURL.Text);
               gData.ExecCmd(command);
               lblStatus.Text = "Updated " + DateTime.Now.ToString();
        }
    }
}