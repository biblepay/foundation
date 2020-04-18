using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved
{
    public partial class Deposit : Page
    {

        protected string _depositreport = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }
            double nReport = GetDouble(Request.QueryString["report"] ?? "");
            if (nReport == 1)
            {
                _depositreport = GetHistory();
            }
            else
            {
                _depositreport = "";
            }

        }

        protected string GetHistory()
        {
            int nHeight = _pool._template.height;

            string sql = "Select * FROM DEPOSIT WHERE userid=@userid and Amount is not null order by Added";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
            DataTable dt = gData.GetDataTable(command);
            string html = "<br><h4>Transaction History Report</h4><br><table class=saved><tr><th width=25%>Notes<th>TXID<th>Date<th>Amount<th>Height</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                double nAmount = GetDouble(dt.Rows[y]["Amount"]);
                string div = "<tr><td>" + dt.Rows[y]["Notes"].ToString()
                    + "<td><small><nobr>" + dt.Rows[y]["TXID"].ToString() + "</nobr></small>" 
                    + "<td>" + dt.Rows[y]["added"].ToString()
                    + "<td>" + dt.Rows[y]["Amount"].ToString()
                    + "<td>" + dt.Rows[y]["Height"].ToString() + "</tr>";
                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }

        protected void btnDepositReport_Click (object sender, EventArgs e)
        {
            Response.Redirect("Deposit?report=1");
        }


        public double GetTotal(string userid)
        {
            string sql = "Select sum(amount) amount from Deposit where userid=@userid";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }

        public string GetPendingTXIDs(string userid)
        {
            string sql = "Select TXID from Deposit where userid=@userid and pending=1 and TXID is not null";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            DataTable dt = gData.GetDataTable(command, false);
            string data = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data += (dt.Rows[i]["TXID"].ToString() ?? "") +",";
            }
            if (data.Length > 1) data = data.Substring(0, data.Length - 1);
            return data;
        }

        protected void btnWithdraw_Click(object sender, EventArgs e)
        {
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            if (gUser(this).TwoFactorAuthorized == false || gUser(this).Require2FA != 1)
            {
                MsgBox("Two Factor Not Enabled", "Sorry, you cannot withdraw unless you enable two factor authorization.  Please go to the Account Edit page to enable 2FA. ", this);
                return;
            }

            double nTotal = GetTotal(gUser(this).UserId.ToString());
            double nReq = GetDouble(txtAmount.Text);

            if (nTotal == 0 || nReq > nTotal)
            {
                MsgBox("Insufficient Funds", "Sorry, the amount requested exceeds your balance.", this);
                return;
            }

            if (nReq <= 0 || nReq > 1000000)
            {
                MsgBox("Amount out of Range", "Sorry, the amount requested is either too high, or below zero.", this);
                return;
            }

            bool bValid = ValidateBiblepayAddress(txtWithdrawalAddress.Text);
            if (!bValid)
            {
                MsgBox("Invalid Address", "Sorry, the withdrawal address is invalid.", this);
                return;
            }

            if (nTotal >= nReq)
            {
                List<Payment> p = new List<Payment>();
                Payment p1 = new Payment();
                p1.bbpaddress = txtWithdrawalAddress.Text;
                p1.amount = nReq;
                p.Add(p1);

                string poolAccount = GetBMSConfigurationKeyValue("PoolPayAccount");

                string txid = SendMany(p, poolAccount, "Withdrawal");
                if (txid == "" || txid == null)
                {
                    MsgBox("Withdrawal Failure", "Sorry, the withdrawal failed!  Please contact contact@biblepay.org. ", this);
                    return;
                }
                // Update the amount
                string sql = "insert into Deposit (id, notes, address, txid, userid, added, amount, height) values (newid(), @notes, @address, @txid, @userid, getdate(), @amount, @height)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
                command.Parameters.AddWithValue("@address", txtWithdrawalAddress.Text);
                command.Parameters.AddWithValue("@txid", txid);
                command.Parameters.AddWithValue("@amount", nReq * -1);
                command.Parameters.AddWithValue("@notes", "Withdrawal");
                command.Parameters.AddWithValue("@height", _pool._template.height);
                gData.ExecCmd(command);
                string sNarr = "The withdrawal was successful; the TXID is <font color=green>" + txid.ToString() + "</font>.  <br><br><br> Thank you for using BiblePay! <br><br><h4>Click <a href=Deposit.aspx>here to see your transaction list. </a></h4><br> ";
                MsgBox("Success. ", sNarr, this);
                return;
            }
            else
            {
                MsgBox("General Failure", "General failure.", this);
            }

        }

        public string DepositReport()
        {
            // If their Deposit address is empty, repopulate
            string sql = "Select * from Users where ID='" + gUser(this).UserId.ToString() + "'";
            DataTable dt = gData.GetDataTable(sql, false);
            if (dt.Rows.Count > 0)
            {
                string dep = dt.Rows[0]["DepositAddress"].ToString() ?? "";
                if (dep == "")
                {
                    dep = GetNewDepositAddress();
                    UpdateSingleField("Users", "DepositAddress", dep, gUser(this).UserId.ToString());
                }
                txtDepositAddress.Text = dep;
            }
            txtBalance.Text = GetTotal(gUser(this).UserId.ToString()).ToString();
            // List pending deposit txids
            string sPending = GetPendingTXIDs(gUser(this).UserId.ToString());
            string html = "";

            if (sPending.Length > 1)
            {
                html += "<font color=red>Pending Incoming Transaction(s): <font color=green>" + sPending + "</font></font><br><br>";
            }
            html += "<span>Note: To make a deposit, send BBP to the below address.  If you send BBP to this address, you will see the pending transaction here.  <br><br>It takes 3 confirms for the pending transaction to be credited to your account.</span><br>Note:  After sending a deposit, please wait two minutes before checking for the deposit (for the list to be refreshed).  <br><br><hr>";
            html += "<br>";
            return html;
        }
    }
}