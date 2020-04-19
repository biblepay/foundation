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

namespace Saved
{
    public partial class FractionalSanctuaries : Page
    {

        double GetEstimatedHODL()
        {
            string sql = "select sum(amount)/3/4500001*365 amt from sanctuaryPayment where added > getdate()-3.15";
            double nROI = gData.GetScalarDouble(sql, "amt");
            return nROI;
        }

        protected string  GetROIGauge()
        {
            try
            {
                string s = RenderGauge(250, "HODL %", (int)(GetEstimatedHODL() * 100));
                return s;
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        protected string _report = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (false)
            { 
               // Coerce the user
               User u = new User();
               u.UserName = GetBMSConfigurationKeyValue("administratorusername");
               u.LoggedIn = true;
               u.TwoFactorAuthorized = true;
               u.Require2FA = 1;
               PersistUser(ref u);
               Session["CurrentUser"] = u;
            }

            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            double nReport = GetDouble(Request.QueryString["FractionalSancReport"] ?? "");
            if (nReport == 1)
            {
                _report = FractionalSancReport();
            }
            else
            {
                _report = "";
            }

            double nTotalFracSancBalance = GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
            double nTotalBalance = GetTotalFrom(gUser(this).UserId.ToString(), "Deposit");
            txtGlobalSancInvestments.Text = GetTotalFrom("", "SanctuaryInvestments").ToString();
            txtGlobalSancInvestmentCount.Text = GetCountFrom("SanctuaryInvestments").ToString();

            txtFractionalSancBalance.Text = nTotalFracSancBalance.ToString();
            txtBalance.Text = nTotalBalance.ToString();

            double nRewards = GetTotalFrom(gUser(this).UserId.ToString(), "Deposit", "Notes like 'sanctuary payment%'");
            txtRewards.Text = nRewards.ToString();

            double nROI = GetEstimatedHODL();
            txtHODLPercent.Text = (nROI*100).ToString() + "%";
        }
        protected string GetBalance()
        {
            return GetTotalFrom(gUser(this).UserId.ToString(), "Deposit").ToString();
        }

        protected string GetTotalSancInvestment()
        {
            return GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments").ToString();
        }


        protected string FractionalSancReport()
        {
            string sql = "Select * FROM Deposit where userid=@userid and Amount is not null and Notes like 'Sanctuary Payment%' order by Added";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
            DataTable dt = gData.GetDataTable(command);
            string html = "<br><h4>Fractional Sanctuary Payment Report</h4><br><table class=saved><tr><th>Type</th><th>TXID<th>Date<th>Amount<th>Height<th width=35%>Notes</tr>";
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                double nAmount = GetDouble(dt.Rows[y]["Amount"]);
                string sNarr = nAmount > 0 ? "Reward" : "Withdrawal";
                string div = "<tr><td>" + sNarr + "<td><small><nobr>" + dt.Rows[y]["TXID"].ToString() + "</nobr></small>" 
                    + "<td>" + dt.Rows[y]["added"].ToString()
                    + "<td>" + dt.Rows[y]["Amount"].ToString()
                    + "<td>" + dt.Rows[y]["Height"].ToString() + "<td>" + dt.Rows[y]["Notes"].ToString() + "</tr>";
                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }


        public double GetTotalFrom(string userid, string table)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null";

            if (userid == "")
            {
                sql = "Select sum(amount) amount from " + table + " where amount is not null";
            }

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }

        public double GetCountFrom(string table)
        {
            string sql = "Select count(userid) ct from " + table + " where amount is not null";
            SqlCommand command = new SqlCommand(sql);
            return gData.GetScalarDouble(command, "ct");
        }


        public double GetTotalFrom(string userid, string table, string where)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null and " + where;
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }

        protected void btnFracReport_Click(object sender, EventArgs e)
        {
            Response.Redirect("FractionalSanctuaries.aspx?fractionalsancreport=1");
        }

        protected void btnAddFractionalSanctuary_Click(object sender, EventArgs e)
        {
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            if (gUser(this).TwoFactorAuthorized == false || gUser(this).Require2FA != 1)
            {
                MsgBox("Two Factor Not Enabled", "Sorry, you cannot add fractional sancs unless you enable two factor authorization.  Please go to the Account Edit page to enable 2FA. ", this);
                return;
            }

            double nTotal = GetTotalFrom(gUser(this).UserId.ToString(), "Deposit");
            double nReq = GetDouble(txtAmount.Text);

            if (nTotal == 0 || nReq > nTotal)
            {
                MsgBox("Insufficient Funds", "Sorry, the amount requested exceeds your balance.", this);
                return;
            }

            if (nReq <= 0 || nReq > 1000000)
            {
                MsgBox("Out of Range", "Sorry, the amount requested is too high or low.", this);
                return;
            }

            if (nTotal >= nReq)
            {
                IncrementAmountByFloat("SanctuaryInvestments", nReq, gUser(this).UserId);

                string sql = "Insert into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), '', @txid, @userid, getdate(), @amount, 0, @notes)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
                command.Parameters.AddWithValue("@amount", nReq * -1);
                command.Parameters.AddWithValue("@txid", Guid.NewGuid().ToString());

                command.Parameters.AddWithValue("@notes", "Sanctuary Investment " + nReq.ToString());
                gData.ExecCmd(command, false, true, true);

                string sNarr = "The fractional sanctuary addition was successful <br><br><br>Now you can sit back and relax.  In approximately 24 hours, you will see new transactions in the Fractional Sanctuary report, and your sanctuary reward will automatically be credited to your balance.  <br><br>Thank you for using BiblePay!  ";
                MsgBox("Success", sNarr, this);
                return;

            }
            else
            {
                MsgBox("General Failure", "General failure.", this);
            }

        }


        protected void btnRemoveFractionalSanctuary_Click(object sender, EventArgs e)
        {
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            if (gUser(this).TwoFactorAuthorized == false || gUser(this).Require2FA != 1)
            {
                MsgBox("Two Factor Not Enabled", "Sorry, you cannot change fractional sancs unless you enable two factor authorization.  Please go to the Account Edit page to enable 2FA. ", this);
                return;
            }

            double nTotalFrac = GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
            double nReq = GetDouble(txtRemoveFractionalAmount.Text);

            if (nReq > nTotalFrac)
            {
                MsgBox("Insufficient Funds", "Sorry, the amount requested exceeds your fractional sanctuary balance.", this);
                return;
            }

            if (nReq <= 0 || nReq > 1000000)
            {
                MsgBox("Out of Range", "Sorry, the amount requested is too high or low.", this);
                return;
            }
            // Deduct the balance and add to the fractional sanc


            if (nTotalFrac >= nReq)
            {
                IncrementAmountByFloat("SanctuaryInvestments", nReq*-1, gUser(this).UserId);

                string sql = "Insert into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), '', @txid, @userid, getdate(), @amount, 0, @notes)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
                command.Parameters.AddWithValue("@amount", nReq);
                command.Parameters.AddWithValue("@notes", "Sanctuary Liquidation " + nReq.ToString());
                command.Parameters.AddWithValue("@txid", Guid.NewGuid().ToString());

                gData.ExecCmd(command);

                string sNarr = "The fractional sanctuary removal was successful <br><br><br> Thank you for using BiblePay!  ";
                MsgBox("Success", sNarr, this);
                return;

            }
            else
            {
                MsgBox("General Failure", "General failure.", this);
            }

        }

    }
}