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
        


        protected void btnAddExpense_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Log In Error", "Sorry, you must be an admin.", this);
                return;
            }

            string sql = "Select count(*) ct from SponsoredOrphan where childid = '" + txtChildID.Text.ToString() + "'";
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
                sql = "Select charity from sponsoredOrphan where ChildID = '" + txtChildID.Text.ToString() + "'";
                string sCharity = gData.GetScalarString(sql, "charity");
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
                sql = "Select * from SponsoredOrphan where charity='" + txtChildID.Text + "'";
                DataTable dt = gData.GetDataTable(sql);
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

                    sql = "Insert into OrphanExpense (id,added,Amount,Charity,HandledBy,ChildID,Balance,Notes) values (newid(),getdate(),'" + dAdjAmt.ToString() + "','" + sCharity
                        + "','bible_pay','" + sChildID + "','" + dBalance.ToString() + "','" + txtNotes.Text + "')";
                    gData.Exec(sql);

                }
            }

            MsgBox("Success", "Congratulations you have updated the record(s)!", this);

        }

    }
}