using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
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
                string sql = "Select * from SponsoredOrphan where id = '" + BMS.PurifySQL(id, 100) + "'";
                double dAmt = gData.GetScalarDouble(sql, "MonthlyAmount");
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

                double dUserBalance = GetDouble(GetUserBalance(this));

                UpdateBBPPrices();
                double dMonthly = GetBBPAmountDouble(dAmt);
                if (dUserBalance < dMonthly)
                {
                    MsgBox("Balance too Low", "Sorry, your balance is too low to sponsor this orphan for a minimum of 30 days.", this);
                    return;
                }
                MsgBox("Error - Undefined", "Sorry, this area is under construction.  We will enable this within 3 days for the benefit of Gods Kingdom.", this);
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
        private double GetBBPAmountDouble(double nUSD)
        {
            if (BBP_USD < .000001)
                return 0;
            return Math.Round(nUSD / BBP_USD, 2);
        }
        private string GetBBPAmount(double nUSD)
        {
            if (BBP_USD < .000001)
                return "";
            string sOut = Math.Round(nUSD / BBP_USD, 2) + " BBP";
            return sOut;
        }

        private double BBP_BTC = 0;
        private double BTC_USD = 0;
        private double BBP_USD = 0;

        private void UpdateBBPPrices()
        {
            BBP_BTC = BMS.GetPriceQuote("BBP/BTC", 1);
            BTC_USD = BMS.GetPriceQuote("BTC/USD");
            BBP_USD = BBP_BTC * BTC_USD;
        }

         protected string GetSponsoredOrphanList()
        {
            UpdateBBPPrices();
            string sql = "Select * from SponsoredOrphan where SponsorID is null";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th>Child ID</th><th>Child Name<th>Added<th>Cost per Month<th>Rebate % Available<th>Monthly Rebate Amount<th>Net Due per Month<th>About this Charity<th>Sponsor Now</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string sAnchor = "<a target='_blank' href='" + s.Props.URL + "'>" + s.Props.Name + "</a>";
                double nRebate = GetBBPAmountDouble(s.Props.MonthlyAmount) * GetDouble(s.Props.MatchPercentage);
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
                    + " BBP<td>" + Math.Round(nNetTotal,2).ToString() + " BBP<td>" + sAboutCharityLink + "<td>" + sSponsorAnchor + "</td></tr>";
                html += a1 + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}