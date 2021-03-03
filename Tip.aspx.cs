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
    public partial class Tip : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            if (!IsPostBack)
            {
                string sToAddress = Request.QueryString["ToAddress"].ToNonNullString();
                double dAmt = Code.Common.GetDouble(Request.QueryString["Amount"].ToNonNullString());
                txtAddress.Text = sToAddress;
                txtAmount.Text = dAmt.ToString();
            }
        }
        
        protected void btnTip_Click(object sender, EventArgs e)
        {
            bool bValid = PoolCommon.ValidateBiblepayAddress(txtAddress.Text);
            double nBalance = DataOps.GetUserBalance(gUser(this).UserId);
            double dAmt = GetDouble(txtAmount.Text);
            string sReferrer = Request.QueryString["referrer"].ToNonNullString();
            if (dAmt > nBalance)
            {
                MsgBox("Balance Too Low", "Sorry, unable to tip user because your balance is too low.", this);
                return;
            }
            if (dAmt < 0 || dAmt > 1000000)
            {
                MsgBox("Out of Range", "Sorry you must tip between .01 and 1MM BBP.", this);
                return;
            }

            if (!bValid)
            {
                MsgBox("Invalid address", "Sorry, the address is invalid.", this);
                return;
            }
            string txid = Withdraw(gUser(this).UserId, txtAddress.Text, dAmt, "Tip to " + txtAddress.Text);

            if (txid == "")
            {
                MsgBox("Send Failure", "Sorry, the tip failed. Please contact rob@biblepay.org", this);
                return;
            }
            else
            {
                string sRedirect = "Click <a href='" + sReferrer + "'>here to return to the page you came from</a>.";
                if (sReferrer == "")
                {
                    sRedirect = "Have a great day.";
                }
                MsgBox("Success!", "You have tipped " + txtAddress.Text + " the amount " + dAmt.ToString() + " BBP.  <br><br>" + sRedirect, this);
                return;
            }
        }

    }
}