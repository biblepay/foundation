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
using static Saved.Code.StringExtension;

namespace Saved
{
    public partial class DbSchemaAdd : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (true && Debugger.IsAttached)
                CoerceUser(Session);


            if (!gUser(this).Admin)
            {
                MsgBox("Not Logged In", "Sorry, you must be an admin to save a tweet.", this);
                return;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string sResult = Saved.Code.UnchainedDatabase.CreateSchema(txtTableName.Text, txtColumnNames.Text, txtDataTypes.Text);
            if (sResult != "")
            {
                MsgBox("Failure", sResult, this);

            }
            else
            {
                MsgBox("Success", "Schema saved.", this);
            }
        }
    }
}