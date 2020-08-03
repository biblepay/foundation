using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class TweetList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (!gUser(this).LoggedIn)
            {
                MsgBox("Logged Out", "Sorry, you must be logged in to see a tweet.", this);
                return;
            }
        }




    }
}