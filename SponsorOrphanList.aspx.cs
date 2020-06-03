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
    public partial class SponsorOrphanList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }



        private string GetTd(DataRow dr, string colname, string sAnchor)
        {
            string val = dr[colname].ToString();
            string td = "<td>" + sAnchor + val + "</a></td>";
            return td;
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
        protected string GetSponsoredOrphanList()
        {
            BBP_BTC = BMS.GetPriceQuote("BBP/BTC", 1);
            BTC_USD = BMS.GetPriceQuote("BTC/USD");
            BBP_USD = BBP_BTC * BTC_USD;

            string sql = "Select * from SponsoredOrphan where SponsorID is null";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th>Child ID</th><th>Child Name<th>Added<th>Cost per Month<th>Rebate % Available<th>Monthly Rebate Amount<th>Net Due per Month</tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string sAnchor = "<a target='_blank' href='" + s.Props.URL + "'>" + s.Props.Name + "</a>";
                double nRebate = GetBBPAmountDouble(s.Props.MonthlyAmount) * GetDouble(s.Props.MatchPercentage);
                double nNetTotal = GetBBPAmountDouble(GetDouble(s.Props.MonthlyAmount)) - nRebate;
                string a1 = "<tr><td>" + s.Props.ChildID + "<td>" + sAnchor + "<td>" 
                    + (s.Props.Added).ToString() + "<td>" 
                    + GetBBPAmount(GetDouble(s.Props.MonthlyAmount)) 
                    + "<td>" + GetPerc(s.Props.MatchPercentage) + "<td>" + Math.Round(nRebate,2).ToString() + " BBP<td>" + Math.Round(nNetTotal,2).ToString() + " BBP</tr>";
                html += a1 + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}