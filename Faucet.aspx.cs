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
    public partial class Faucet : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            MsgBox("Withdrawal Failure", "Sorry, this faucet is down.  Please <a href=https://forum.biblepay.org/index.php?topic=586.new#new> participate in this program instead.  </a> ", this);

        }



        protected void btnGetMoney_Click(object sender, EventArgs e)
        {

            double nFaucetReward = 10;

                List<Payment> p = new List<Payment>();
                Payment p1 = new Payment();
                p1.bbpaddress = txtAddress.Text;
                p1.amount = nFaucetReward;
                p.Add(p1);
                string poolAccount = GetBMSConfigurationKeyValue("PoolPayAccount");
                string sValid = WebServices.VerifyEmailAddress(txtEmail.Text,"");
                Log("Checking For Faucet " + txtEmail.Text + ": " + sValid);
                if (sValid != "deliverable")
                {
                    MsgBox("Email Invalid", "Sorry, the e-mail address is invalid. ", this);
                    return;
                }
                bool fBBPValid = BMS.ValidateAddress(txtAddress.Text, "BBP");
                if (!fBBPValid)
                {
                   MsgBox("BBP Address Invalid", "Sorry, the address is invalid. ", this);
                   return;
                }
            string sEmail = txtEmail.Text.Trim();
            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();
            sIP = sIP.Replace("::ffff:", "");
            string sql = "Select count(*) ct from Faucet where IPAddress='" + sIP + "' OR bbpaddress = '" + BMS.PurifySQL(txtAddress.Text, 250) + "' or EmailAddress='" + BMS.PurifySQL(sEmail, 200) + "'";
            double nDupe = gData.GetScalarDouble(sql, "ct");
            sql = "Select count(*) ct from Users where EmailAddress='" + BMS.PurifySQL(sEmail, 200) + "'";
            double nDupe2 = gData.GetScalarDouble(sql, "ct");
            double nDupe3 = 0;
            if (Session["Faucet"].ToNonNullString()  == "1")
                nDupe3 = 1;
            if (nDupe > 0 || nDupe2 > 0 || nDupe3 > 0)
            {
                MsgBox("Duplicate withdrawal", "Sorry, this is a duplicate withdrawal. ", this);
                return;
            }

            // Update the amount
            string txid = SendMany(p, poolAccount, "Withdrawal");
            if (txid == "" || txid == null)
            {
                MsgBox("Withdrawal Failure", "Sorry, the withdrawal failed!  Please contact contact@biblepay.org. ", this);
                return;
            }
            else
            { 
                sql = "insert into Faucet (id, BBPAddress, IPAddress, EmailAddress, Amount, Added) values (newid(), @address, @ipaddress, @emailaddress, @amount, getdate())";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                command.Parameters.AddWithValue("@ipaddress", sIP);
                command.Parameters.AddWithValue("@emailaddress", sEmail);
                command.Parameters.AddWithValue("@amount", nFaucetReward);
                gData.ExecCmd(command);
                Session["Faucet"] = "1";

                string sNarr = "The withdrawal was successful; the TXID is <font color=green>" + txid.ToString() + "</font>.  <br><br><br> Thank you for using BiblePay! <br><br><h4>Click <a href=media.aspx?category=miscellaneous>here to see our Christian Videos. </a></h4><br> ";
                MsgBox("Success. ", sNarr, this);
                return;
            }
        }
   }
}