using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class DonorMatchList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
        protected string GetDonorMatchList()
        {
            BBP_BTC = BMS.GetPriceQuote("BBP/BTC", 1);
            BTC_USD = BMS.GetPriceQuote("BTC/USD");
            BBP_USD = BBP_BTC * BTC_USD;

            string sql = "Select * from DonorMatch Inner Join Users on Users.ID = DonorMatch.UserID";
            DataTable dt = gData.GetDataTable2(sql);
            string html = "<table class=saved><tr><th>User Name</th><th>Total Donation Amount<th>Added</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
              //  string sAnchor = "<a target='_blank' href='" + s.Props.URL + "'>" + s.Props.Name + "</a>";
               // double nRebate = GetBBPAmountDouble(s.Props.MonthlyAmount) * GetDouble(s.Props.MatchPercentage);
               // double nNetTotal = GetBBPAmountDouble(GetDouble(s.Props.MonthlyAmount)) - nRebate;
                string a1 = "<tr><td>" + s.Props.UserName + "<td>" + s.Props.Amount + "<td>" 
                    + (s.Props.Added).ToString() + "</tr>";
                html += a1 + "\r\n";
            }
            html += "</table>";
            return html;
        }


        protected void btnDonate_Click(object sender, EventArgs e)
        {
            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in first.", this);
                return;
            }

            MsgBox("Disabled", "Sorry, we are upgrading this feature now.  Please try back later.", this);
            return;
        }

        }
    }