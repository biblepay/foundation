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
    public partial class Report : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (!gUser(this).LoggedIn)
            {
                MsgBox("Logged Out", "Sorry, you must be logged in to run personalized reports.", this);
                return;
            }

            string sName = Request.QueryString["Name"] ?? "";
            if (sName == "myorphans")
            {
                Session["ReportSQL"] = "select ID,ChildID as [Child ID],Charity,URL,Added,MonthlyAmount as [Monthly Amount],MatchPercentage as [Match Percentage],"
                    + "LastPaymentDate as [Last Payment Date] from sponsoredOrphan where userid='" + gUser(this).UserId + "'";
                Session["ReportColumns"] = "Child ID,Charity,URL,Added,Monthly Amount,Match Percentage,Last Payment Date";
                Session["ReportName"] = "My Sponsored Orphans Report";
            }
            else if (sName == "orphantx")
            {
                Session["ReportSQL"] = "select sponsoredorphan.ChildID as [Child ID],Charity,URL,Notes,Amount,Updated as [Payment Date]"
                    + " from sponsoredOrphan "
                    + " inner join sponsoredorphanPayments on Sponsoredorphanpayments.childid = sponsoredorphan.childid "
                    + " where sponsoredorphan.userid='" + gUser(this).UserId + "' order by Updated desc "; 
                Session["ReportColumns"] = "Child ID,Charity,URL,Notes,Amount,Payment Date";
                Session["ReportName"] = "My Sponsored Orphan(s) Payment Report";
            }
            else if (sName == "deposithistory")
            {
                Session["ReportSQL"] = "Select * from DEPOSIT where Userid='" + gUser(this).UserId + "' and AMOUNT is not NULL order by Added desc";
                Session["ReportColumns"] = "Notes,TXID,Added,Amount,Height";
                Session["ReportName"] = "My Deposit Report (Ordered by Most Recent Descending)";
            }
            else if (sName == "fractionalsanctx")
            {
                Session["ReportSQL"] = "Select * FROM Deposit where userid = '" + gUser(this).UserId + "' and Amount is not null and Notes like 'Sanctuary Payment%' order by Added desc";
                Session["ReportColumns"] = "TXID,Added,Amount,Height,Notes";
                Session["ReportName"] = "My Fractional Sanctuary Transaction History (Ordered by Most Recent Descending)";
            }
            else
            {
                MsgBox("Error", "Sorry, this type of report does not exist.", this);
                return;
            }
        }

        public static string GetReportHeader(DataTable dt, string sCols)
        {
            string html = "<tr>";
            string[] vCols = sCols.Split(",");
            
            for (int i = 0; i < vCols.Length; i++)
            {
                string sColName = vCols[i];
                string sTH = "<th>" + sColName + "</th>";
                html += sTH;
            }
            html += "</tr>";
            return html;
        }

        protected string GetReport()
        {
            string sql = Session["ReportSQL"].ToNonNullString();
            string sName = Request.QueryString["Name"] ?? "";
            DataTable dt = gData.GetDataTable(sql);
            string sCols = Session["ReportColumns"].ToNonNullString();
            string[] vCols = sCols.Split(",");
            string sHTML = "<table class=saved>";
            sHTML += GetReportHeader(dt, sCols);    
            int nRowsPerPage = 15;
            int nRowsConsumed = 0;
            int nPageNo = (int)GetDouble(Request.QueryString["pag"] ?? "");
            int nStartRow = nPageNo * nRowsPerPage;
            int nEndRow = nStartRow + nRowsPerPage - 1;
            double nTotalPages = (int)Math.Ceiling((double)(dt.Rows.Count / nRowsPerPage)) + 1;
            for (int y = nStartRow; y <= nEndRow && y < dt.Rows.Count; y++)
            {
                string sRow = "<tr>";
                for (int i = 0; i < vCols.Length; i++)
                {
                    string sValue = dt.Rows[y][vCols[i]].ToNonNullString();
                    if (sValue.Contains("https://"))
                    {
                        sValue = "<a href='" + sValue + "' target=_blank>View</a>";
                    }
                    string sMoniker = "";
                    string sEndMoniker = "";
                    if (vCols[i] == "TXID")
                    {
                        sMoniker = "<small><nobr>";
                        sEndMoniker = "</nobr></small>";
                    }
                    sRow += "<td>" + sMoniker +  sValue + sEndMoniker + "</td>";
                }
                sRow += "</tr>";
                sHTML += sRow;

                nRowsConsumed++;
                if (nRowsConsumed > nRowsPerPage)
                    break;
            }
            sHTML += "</table>";
            string sURL = "Report?name=" + sName;
            sHTML += GetPagControl(sURL, nPageNo, (int)nTotalPages);
            if (dt.Rows.Count == 0)
            {
                sHTML += "<div>You have no sponsored orphans.</div>";
            }
            return sHTML;
        }
    }
}