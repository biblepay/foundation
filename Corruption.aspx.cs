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
    public partial class Corruption : Page
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

        protected string GetCorruption()
        {
            
            string sql = "Select * From Corruption Order by BlockCount";

            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>Block Count</th><th>XMR Raised<th>Corruption Percentage<th>Pool Name<th>Pool Address</tr>";

            double _height = 0;
            double oldheight = 0;
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                double nXMR = GetDouble(dt.Rows[y]["XMR"].ToString());
                double nXXMR = GetDouble(dt.Rows[y]["XXMR"].ToString());
                double nCorruptionPercentage = 1 - (nXMR / (nXXMR +.0000001));



                string div = "<tr><td>" + dt.Rows[y]["BlockCount"].ToString() 
                    + "<td>" + dt.Rows[y]["XMR"].ToString() 
                    + "<td>" + Math.Round(GetDouble(nCorruptionPercentage.ToString()) * 100, 2) + "%"
                    + "<td>" + dt.Rows[y]["PoolName"].ToString()
                    + "<td>" + dt.Rows[y]["Recipient"].ToString() +"</tr>";
                html += div + "\r\n";
                
                oldheight = _height;

            }
            html += "</table>";

            return html;
        }
    }
}