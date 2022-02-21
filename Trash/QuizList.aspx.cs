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
    public partial class QuizList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        
        public string RenderControlToHtml(Control ControlToRender)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter stWriter = new System.IO.StringWriter(sb);
            System.Web.UI.HtmlTextWriter htmlWriter = new System.Web.UI.HtmlTextWriter(stWriter);
            ControlToRender.RenderControl(htmlWriter);
            return sb.ToString();
        }
        private string GetTd(DataRow dr, string colname, string sAnchor)
        {
            string val = dr[colname].ToString();
            string td = "<td>" + sAnchor + val + "</a></td>";
            return td;
        }

        protected string GetQZ()
        {
            int nHeight = Common.GetHeight();
            string sql = "Select * from Quiz order by Added";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>BBP Address</th><th>Book<th>Solved<th>Reward<th>TXID</tr>";
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string div = "<tr><td>" + dt.Rows[y]["bbpaddress"].ToString()
                    + "<td>" + dt.Rows[y]["Book"].ToString()
                    + "<td>" + dt.Rows[y]["Solved"].ToString()
                    + "<td>" + dt.Rows[y]["Reward"].ToString()
                    + "<td>" + dt.Rows[y]["TXID"].ToString()
                    + "</tr>";
                html += div + "\r\n";

            }
            html += "</table>";

            return html;
        }
    }
}