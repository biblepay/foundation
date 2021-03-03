using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved
{
    public partial class Scratchpad : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {


        }



        protected void btnScratchpad_Click(object sender, EventArgs e)
        {

            sScratchpad = txtBody.Text;
            MsgBox("Scratchpad", "Your URL is:  https://foundation.biblepay.org/Scratchpad <br><br> It will be erased once viewed.<br>.Thank you.", this);
            
        }

   }
}