using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;

namespace Saved
{
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnPost_Click(object sender, EventArgs e)
        {
            if (txtData.Text=="1")
            {
                // Show the dynamic message box here
                divThankYou.Visible = true;
                lblmessage.Text = "Are you sure want to do this?";
            }
        }

        protected void btnMessageBox_Click(object sender, EventArgs e)
        {
            divThankYou.Visible = false;
        }
    }
}