using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Web.UI;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Accountability : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string sYear = Request.QueryString["year"] ?? "";
            string sType = Request.QueryString["type"] ?? "";
            if (sType != "")
            {
                GenerateCharityReport(sType);
                return;
            }
            if (sYear != "")
            {
                if (sYear == "total")
                {
                    GenerateTotalReport();
                    return;
                }
                GenerateAccountingReport((int)GetDouble(sYear));
                return;
            }
        }

        public void GenerateAccountingReport(int nYear)
        {
            string sql = "SELECT * FROM (select id,added as a1, FORMAT (added, 'MMMM yyyy') as Added,'DR' as Type,Amount,Charity, '' as Notes from expense "
                + " union all  select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type,Amount, Charity, Notes from Revenue ) b where year(b.a1)='" 
                + nYear.ToString() + "' order by a1 ";
            string html = UICommon.GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            Response.Clear();
            Response.ContentType = "application/pdf";
            string accName = "BiblePay Accounting Year " + nYear.ToString() + ".pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + accName);
            Response.BinaryWrite(result);
            Response.Flush();
            Response.End();
        }

        public void GenerateTotalReport()
        {
            string sql = "SELECT newid(),sum(amount) Amount,'' Notes,added,Type, 'Various' Charity, max(Dt1) FROM( "
                + "select id, added as a1, FORMAT(added, 'MMMM yyyy') as Added, 'DR' as Type, Amount, Charity, '' as Notes, added as dt1 from expense"
                + "    union all"
                + " select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type, Amount, Charity, Notes, added as dt1 from Revenue"
                + "  ) b group by added, Type  order by max(dt1)";

            string html = UICommon.GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            Response.Clear();
            Response.ContentType = "application/pdf";
            string accName = "BiblePay All Time.pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + accName);
            Response.BinaryWrite(result);
            Response.Flush();
            Response.End();
        }

        public void GenerateCharityReport(string sCharity)
        {
            string sql = "SELECT orphanexpense.id,orphanexpense.Amount,orphanexpense.Notes,orphanexpense.Added,orphanexpense.Charity,orphanexpense.ChildID,orphanexpense.Balance,sponsoredOrphan.Name "
                + " from orphanexpense inner join sponsoredorphan on sponsoredorphan.childid=orphanexpense.childid where sponsoredorphan.charity = '" 
                + BMS.PurifySQL(sCharity, 50) + "' order by added";

            string html = UICommon.GetCharityTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            Response.Clear();
            Response.ContentType = "application/pdf";
            string accName = "Charity Report - " + sCharity + ".pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + accName);
            Response.BinaryWrite(result);
            Response.Flush();
            Response.End();
        }


        public string GetPDFList()
        {

            string html = "<table>";
            for (int year = 2017; year <= DateTime.Now.Year; year++)
            {
                string row = "<tr><td><a href=Accountability.aspx?year=" + year.ToString() + ">Accounting Year " + year.ToString() + "</a></td></tr>\r\n";
                html += row;
            }
            html += "<tr><td><a href=Accountability.aspx?year=total>Grand Total (All Time)</a></td></tr>\r\n";
            html += "</table>";

            html += "<br><table><tr><td><a href=Accountability?type=cameroon-one>Cameroon-One Report</a></td></tr>\r\n";
            html += "<tr><td><a href=Accountability?type=kairos>Kairos Report</a></td></tr>\r\n";
            html += "</table>";



            return html;
        }
    }
}