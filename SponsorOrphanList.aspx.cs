using Saved.Code;
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
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class SponsorOrphanList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);
            string action = Request.QueryString["action"] ?? "";
            string id = Request.QueryString["id"] ?? "";
            if (action=="sponsornow" && id.Length > 1)
            {
                string sql = "Select * from SponsoredOrphan where userid is null and id = '" + BMS.PurifySQL(id, 100) + "' ";
                double dAmt = 0;
                try
                {
                    dAmt = gData.GetScalarDouble(sql, "MonthlyAmount");
                }catch(Exception ex)
                {
                    MsgBox("Error", "Please contact rob@biblepay.org for more information", this);
                    return;
                }
                string sChildID = gData.GetScalarString(sql, "childid");
                string sName = gData.GetScalarString(sql, "name");

                if (dAmt == 0)
                {
                    MsgBox("Orphan does not exist", "Sorry, this orphan no longer exists. ", this);
                    return;
                }

                if (!gUser(this).LoggedIn)
                {
                    MsgBox("Not Logged In", "Sorry, you must be logged in first.", this);
                    return;
                }

                double dUserBalance = GetDouble(DataOps.GetUserBalance(this));
                
                UpdateBBPPrices();
                double dMonthly = GetBBPAmountDouble(dAmt);
                if (dUserBalance < dMonthly)
                {
                    MsgBox("Balance too Low", "Sorry, your balance is too low to sponsor this orphan for a minimum of 30 days.", this);
                    return;
                }
                // They have enough BBP; lets remove the first months payment, and set the last payment date:
                string sql1 = "Update SponsoredOrphan set Userid=@userid, LastPaymentDate=getdate() where id='" + id.ToString() + "'";
                SqlCommand command = new SqlCommand(sql1);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());
                gData.ExecCmd(command);
                //IncrementAmountByFloat("SponsoredOrphanPayments", dMonthly, gUser(this).UserId);
                string sNotes = "Initial Sponsorship";
                sql1 = "Insert into SponsoredOrphanPayments (id,childid,amount,added,userid,updated,notes) values (newid(),'" 
                    + sChildID + "','" + dMonthly.ToString() + "',getdate(),'" + gUser(this).UserId.ToString() + "',getdate(),'" + sNotes + "')";
                gData.Exec(sql1);


                DataOps.AdjBalance(-1 * dMonthly, gUser(this).UserId.ToString(), "Sponsor Payment " + sChildID);

                MsgBox("Success", "Thank you for sponsoring " + sName + "!  You are bearing Christian Fruit and fulfilling James 1:27.  <br><br>Starting in 30 days, we will deduct the sponsorship amount automatically each month.  ", this);

                return;
            }
        }

        private string FormatTwoPlaces(double nAmt)
        {
            return string.Format("{0:#.00}", nAmt);
        }
        private string GetPerc(object oPerc)
        {
            double nPerc = GetDouble(oPerc) * 100;
            string nOut = Math.Round(nPerc, 2).ToString() + "%";
            return nOut;
        }


         protected string GetSponsoredOrphanList()
        {
            UpdateBBPPrices();
            string sql = "Select * from SponsoredOrphan where UserID is null and matchpercentage > .01 order by MatchPercentage desc";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th>Child ID</th><th>Child Name<th>Added<th>Cost per Month<th>Rebate % Available<th>Monthly Rebate Amount<th>Net Due per Month<th>Net Due in USD<th>About this Charity<th>Sponsor Now</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string sAnchor = "<a target='_blank' href='" + s.Props.URL + "'>" + s.Props.Name + "</a>";
                double nRebate = GetBBPAmountDouble(s.Props.MonthlyAmount) * GetDouble(s.Props.MatchPercentage);
                double nUSDRebate = s.Props.MonthlyAmount * s.Props.MatchPercentage;
                double nUSDTotal = s.Props.MonthlyAmount - nUSDRebate;

                double nNetTotal = GetBBPAmountDouble(GetDouble(s.Props.MonthlyAmount)) - nRebate;
                string sID = dt.Rows[y]["id"].ToString();
                string sSponsorLink = "SponsorOrphanList?action=sponsornow&id=" + sID;


                string sSponsorAnchor = "<div><a href=\"" + sSponsorLink + "\"><input type='button' id='btnsponsornow' submit='true' value='Sponsor Me' style='width:140px' /></a></div>";

                string sCharityName = dt.Rows[y]["Charity"].ToString();

                string sAboutCharityLink = "<a target='_blank' href='" + s.Props.AboutCharity + "'>" + sCharityName + "</a>";

                string a1 = "<tr><td>" + s.Props.ChildID + "<td>" + sAnchor + "<td>" 
                    + (s.Props.Added).ToString() + "<td>" 
                    + GetBBPAmount(GetDouble(s.Props.MonthlyAmount)) 
                    + "<td>" + GetPerc(s.Props.MatchPercentage) + "<td>" + Math.Round(nRebate,2).ToString() 
                    + " BBP<td>" + Math.Round(nNetTotal,2).ToString() + " BBP<td>$" + FormatTwoPlaces(nUSDTotal) + "<td>" + sAboutCharityLink + "<td>" + sSponsorAnchor + "</td></tr>";
                html += a1 + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}