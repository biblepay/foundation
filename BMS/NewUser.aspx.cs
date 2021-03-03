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
    public partial class NewUser : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
                string sResult = Saved.Code.BMS.CheckNewUserReward(Request);
                Response.Write(sResult);
                Response.End();
                return;
        }
    }
}