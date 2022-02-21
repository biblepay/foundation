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
    public partial class AddExpense : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (!gUser(this).Admin)
            {
                
                MsgBox("Log In Error", "Sorry, you must be an admin", this);
                return;
            }
        }

        protected void btnAddExpenseRecord_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Log In Error", "Sorry, you must be an admin.", this);
                return;
            }

            double dBTC = GetDouble(txtBitcoin.Text);
            double dPool = GetDouble(txtFoundation.Text);
            double dSong = GetDouble(txtSong.Text);
            double dXMRRate = GetDouble(txtXMRRate.Text);
            double camAmt = 35 * 40;
            double kairAmt = 25 * 19;
            string sql = "Insert into Expense (id,added,amount,url,charity,handledBy, superblock) values (newid(), getdate(), '" + camAmt.ToString() + "', 'CAMEROON-ONE','bible_pay','CAMEROON-ONE')";
            gData.Exec(sql);
            sql = "       Insert into Expense (id,added,amount,url,charity,handledBy, superblock) values (newid(), getdate(), '" + kairAmt.ToString() + "','KAIROS','bible_pay','KAIROS')";
            gData.Exec(sql);
            //ouble nTotalRaised = dPool + dSong;
            //double nAmount = dXMRRate * nTotalRaised;
            //double dBTCRaised = nAmount / (dBTC + .01);
            // sql = "Insert into revenue (id,added,bbpamount,btcraised,btcprice,amount,notes,handledBy,Charity) values (newid(),getdate(),0,'" + dBTCRaised.ToString() + "','" + dBTC.ToString() + "','" 
            //     + nAmount.ToString() + "','" + sNotes + "','bible_pay','CAMEROON-ONE')";
            MsgBox("Success", "Added monthly expense", this);


        }

        protected void btnAddExpense_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Log In Error", "Sorry, you must be an admin.", this);
                return;
            }
            string sAdded = txtAdded.Text;
            if (sAdded == "")
            {
                sAdded = DateTime.Now.ToString();
            }

            string sql = "Select count(*) ct from SponsoredOrphan where childid = '" + txtChildID.Text.ToString() + "' and active=1 ";
            double dCt = gData.GetScalarDouble(sql, "ct");
            bool fUpdateAll = txtChildID.Text == "cameroon-one" || txtChildID.Text == "kairos";

            if (dCt == 0 && !fUpdateAll)
            {
                MsgBox("Fail","Orphan does not exist", this);
                return;
            }
            if (txtNotes.Text == "")
            {
                MsgBox("Fail", "Notes blank", this);
                return;

            }
            if (!fUpdateAll)
            {
                sql = "Select charity from sponsoredOrphan where ChildID = '" + BMS.PurifySQL(txtChildID.Text.ToString(),20) + "' and active = 1 ";
                string sCharity = gData.GetScalarString2(sql, "charity");
                sql = "Select top 1 Balance balance from OrphanExpense where childID = '" + txtChildID.Text + "' order by Added desc";
                double dBalance = gData.GetScalarDouble(sql, "Balance");
                double dBalance2 = GetDouble(txtBalance.Text.ToString());

                if (dBalance2 != 0)
                    dBalance = dBalance2;
                dBalance += GetDouble(txtExpenseAmount.Text);

                sql = "Insert into OrphanExpense (id,added,Amount,Charity,HandledBy,ChildID,Balance,Notes) values (newid(),getdate(),'" + txtExpenseAmount.Text + "','" + sCharity 
                    + "','bible_pay','" + txtChildID.Text + "','" + dBalance.ToString() + "','" + txtNotes.Text + "')";
                gData.Exec(sql);

            }
            else
            {
                sql = "Select * from SponsoredOrphan where charity='" + BMS.PurifySQL(txtChildID.Text,20) + "' and active = 1 ";
                DataTable dt = gData.GetDataTable2(sql);
                double dAdjAmt = GetDouble(txtExpenseAmount.Text);

                if (dt.Rows.Count == 0 || dAdjAmt == 0)
                {
                    MsgBox("Fail", "Unable to Locate Charity - or zero entered for amt.", this);
                    return;
                }
                if (dAdjAmt < 0)
                {
                    // This is a payment; apply it across all orphans
                    dAdjAmt = Math.Round(GetDouble(txtExpenseAmount.Text) / dt.Rows.Count, 2);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string sChildID = dt.Rows[i]["ChildID"].ToString();
                    string sCharity = txtChildID.Text.ToUpper();
                    sql = "Select top 1 Balance balance from OrphanExpense where childID = '" + sChildID + "' order by Added desc";
                    double dBalance = gData.GetScalarDouble(sql, "Balance");
                    dBalance += dAdjAmt;

                    sql = "Insert into OrphanExpense (id,added,Amount,Charity,HandledBy,ChildID,Balance,Notes) values (newid(),'" + sAdded + "','" + dAdjAmt.ToString() + "','" + sCharity
                        + "','bible_pay','" + sChildID + "','" + dBalance.ToString() + "','" + txtNotes.Text + "')";
                    gData.Exec(sql);

                }
            }

            MsgBox("Success", "Congratulations you have updated the record(s)!", this);

        }

    }
}