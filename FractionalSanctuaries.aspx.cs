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
using static Saved.Code.UICommon;
using static Saved.Code.DataOps;

namespace Saved
{
    public partial class FractionalSanctuaries : Page
    {

        protected string GetNonCompounded()
        {
            double nO1 = 0;
            double nO2 = 0;
            return Math.Round(GetEstimatedHODL(false, 0, out nO1, out nO2)*100, 2).ToString();
        }


        protected string  GetROIGauge()
        {
            try
            {
                double nO1 = 0;
                double nO2 = 0;
                string s = RenderGauge(250, "HODL %", (int)(GetEstimatedHODL(true, 0, out nO1, out nO2) * 100));
                return s;
            }
            catch(Exception)
            {
                return "";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (gUser(this).LoggedIn == false)
            {

                string Promotion = "<br><font color=red></font>Thank you for using BiblePay.";
                
                MsgBox("Log In Error", "Sorry, you must be logged in first." + Promotion, this);
                return;
            }
           
            double nTotalFracSancBalance = DataOps.GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
            double nTotalBalance = DataOps.GetTotalFrom(gUser(this).UserId.ToString(), "Deposit");
            txtGlobalSancInvestments.Text = DataOps.GetTotalFrom("", "SanctuaryInvestments").ToString();
            txtGlobalSancInvestmentCount.Text = GetUserCountFrom("SanctuaryInvestments").ToString();
            txtFractionalSancBalance.Text = nTotalFracSancBalance.ToString();
            txtBalance.Text = nTotalBalance.ToString();

            double nRewards = GetTotalFrom(gUser(this).UserId.ToString(), "Deposit", "Notes like 'sanctuary payment%'");
            txtRewards.Text = nRewards.ToString();
            double nGrossEarningsPerDay = 0;
            double nOrphanDeductions = 0;
            double nROI = GetEstimatedHODL(true, 0, out nGrossEarningsPerDay, out nOrphanDeductions);
            txtBBPEarningsPerDay.Text = nGrossEarningsPerDay.ToString() + " BBP";
            txtOrphanChargesPerDay.Text = nOrphanDeductions.ToString() + " BBP";
            txtHODLPercent.Text = (nROI*100).ToString() + "%";
            double nNet = nGrossEarningsPerDay - nOrphanDeductions;
            double nOrpSancPct = GetOrphanFracSancPercentage();

            txtNetEarningsPerDay.Text = nNet.ToString() + " BBP";
        }


        public double GetUserCountFrom(string table)
        {
            string sql = "Select count(userid) ct from " + table + " where amount is not null";
            SqlCommand command = new SqlCommand(sql);
            return gData.GetScalarDouble(command, "ct");
        }
        

        protected void btnFracReport_Click(object sender, EventArgs e)
        {
            Response.Redirect("Report?name=fractionalsanctx");
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
            // Ensure total does not exceed cap
            double nCap = 4250000;
            double nTotalFracExisting = DataOps.GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
            if (nReq + nTotalFracExisting > nCap)
            {
                MsgBox("Investment not accepted", "Sorry, our fractional sanctuaries are limited to a total of " + nCap.ToString() + " BBP at this time.  Please create a full sanctuary.", this);
                return;
            }

            bool fDisable = false;
            if (fDisable)
            {
                MsgBox("Investment not accepted", "Sorry, our fractional sanctuaries are currently being upgraded to POOS.  Please try back on September 1st, 2020.", this);
                return;
            }

            if (nTotal >= nReq)
            {
                DataOps.IncrementAmountByFloat("SanctuaryInvestments", nReq, gUser(this).UserId);
                DataOps.AdjBalance(-1 * nReq, gUser(this).UserId.ToString(), "Sanctuary Investment " + nReq.ToString());
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

            double nTotalFrac = DataOps.GetTotalFrom(gUser(this).UserId.ToString(), "SanctuaryInvestments");
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
                DataOps.IncrementAmountByFloat("SanctuaryInvestments", nReq*-1, gUser(this).UserId);
                DataOps.AdjBalance(nReq, gUser(this).UserId.ToString(), "Sanctuary Liquidation " + nReq.ToString());
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
