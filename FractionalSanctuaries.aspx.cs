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

namespace Saved
{
    public partial class FractionalSanctuaries : Page
    {

        protected string GetNonCompounded()
        {

            double nBP = GetBonusPercent();
            return Math.Round(GetEstimatedHODL(false, nBP)*100, 2).ToString();
        }

        double GetBonusPercent()
        {
            string sql = "Select BonusPercent from Users where id='" + gUser(this).UserId.ToString() + "'";
            double nPerc = gData.GetScalarDouble(sql, "bonusPercent");
            return nPerc;
        }

        protected string  GetROIGauge()
        {
            try
            {
                double nBP = GetBonusPercent();

                string s = RenderGauge(250, "HODL %", (int)(GetEstimatedHODL(true, nBP) * 100));
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
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (gUser(this).LoggedIn == false)
            {

                string Promotion = "<br><font color=red>LIMITED TIME PROMOTION:  <br><br>For a limited time, we will be giving away an extra 10% bonus resulting in a <b>52%</b> ROI for new users who set up fractional sanctuaries!<br>To Claim the reward, <ul>Please meet the following conditions: <li>Create an account at forum.biblepay.org after May 6th, 2020.  <li>Create a new Fractional Sanctuary.<li>Your Fractional Sanctuary page will clearly show the BONUS percentage on the page if you qualified.<li>There is no end date yet for this promotion.<li>If you have any questions or need help, please e-mail rob@biblepay.org </ul></font>Thank you for using BiblePay.";
                
                MsgBox("Log In Error", "Sorry, you must be logged in first." + Promotion, this);
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

            double nTotalFracSancBalance = Common.GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
            double nTotalBalance = Common.GetTotalFrom(gUser(this).UserId.ToString(), "Deposit");
            txtGlobalSancInvestments.Text = Common.GetTotalFrom("", "SanctuaryInvestments").ToString();
            txtGlobalSancInvestmentCount.Text = GetCountFrom("SanctuaryInvestments").ToString();
            txtBonusPercent.Text = (GetBonusPercent() * 100).ToString() + "%";
            txtFractionalSancBalance.Text = nTotalFracSancBalance.ToString();
            txtBalance.Text = nTotalBalance.ToString();

            double nRewards = Common.GetTotalFrom(gUser(this).UserId.ToString(), "Deposit", "Notes like 'sanctuary payment%'");
            txtRewards.Text = nRewards.ToString();

            double nBP = GetBonusPercent();
            double nROI = GetEstimatedHODL(true, nBP);
            txtHODLPercent.Text = (nROI*100).ToString() + "%";
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


        public double GetCountFrom(string table)
        {
            string sql = "Select count(userid) ct from " + table + " where amount is not null";
            SqlCommand command = new SqlCommand(sql);
            return gData.GetScalarDouble(command, "ct");
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
                // Update the Bonus Percent
                
                AdjBalance(-1 * nReq, gUser(this).UserId.ToString(), "Sanctuary Investment " + nReq.ToString());

                string sql = "Update SanctuaryInvestments set bonuspercent = (Select bonuspercent from Users where id='" 
                    + gUser(this).UserId.ToString() + "') where userid = '" + gUser(this).UserId.ToString() + "'";
                gData.Exec(sql);
                
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

                AdjBalance(nReq, gUser(this).UserId.ToString(), "Sanctuary Liquidation " + nReq.ToString());

                
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