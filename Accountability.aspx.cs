using OpenHtmlToPdf;
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
            string html = GetTableHTML(sql);
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
            string sql = "SELECT newid(),sum(amount) Amount,'' Notes,max(added) Added,Type,Charity FROM (select id,added as a1, FORMAT (added, 'MMMM yyyy') as Added,'DR' as Type,Amount,Charity, '' as Notes from expense "
                + " union all  select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type,Amount, Charity, Notes from Revenue ) b   group by Type,Charity ";

            string html = GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            Response.Clear();
            Response.ContentType = "application/pdf";
            string accName = "BiblePay All Time.pdf";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + accName);
            Response.BinaryWrite(result);
            Response.Flush();
            Response.End();
        }

        public string GetPDFList()
        {

            string html = "<table>";
            for (int year = 2017; year <= 2020; year++)
            {
                string row = "<tr><td><a href=Accountability.aspx?year=" + year.ToString() + ">Accounting Year " + year.ToString() + "</a></td></tr>\r\n";
                html += row;
            }
            html += "<tr><td><a href=Accountability.aspx?year=total>Grand Total (All Time)</a></td></tr>\r\n";
            html += "</table>";
            return html;
        }
    }
}