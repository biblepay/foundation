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
    public partial class BlockHistory : Page
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

        public static int lastBH = 0;
        public static string lastReport = "";
        protected string GetBH()
        {
            int elapsed = UnixTimeStamp() - lastBH;
            if (elapsed < (60 * 17))
            {
                return lastReport;
            }
            int nHeight = _pool._template.height;
            lastBH = UnixTimeStamp();
            string sql = "Select Height, bbpaddress, percentage, reward, subsidy, txid from Share (nolock) where subsidy > 1 and reward > .01 and updated > getdate()-2 and height > " 
                + (nHeight-(300)).ToString() + "order by height desc, bbpaddress";

            
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>Height</th><th>BBP Address<th>Percentage<th>Reward<th>Block Subsidy<th>TXID</tr>";

            double _height = 0;
            double oldheight = 0;
            for (int y = 0; y < dt.Rows.Count; y++)
            {

                _height = GetDouble(dt.Rows[y]["height"]);
                if (oldheight > 0 && _height != oldheight)
                {
                    html += "<tr style='background-color:white;'><td style='background-color:white;' colspan = 6><hr></td></tr>";
                }

                string div = "<tr><td>" + dt.Rows[y]["height"].ToString() 
                    + "<td>" + dt.Rows[y]["bbpaddress"].ToString() 
                    + "<td>" + Math.Round(GetDouble(dt.Rows[y]["percentage"].ToString()) * 100, 2) + "%"
                    + "<td>" + dt.Rows[y]["reward"].ToString()
                    + "<td>" + dt.Rows[y]["subsidy"].ToString() 
                    + "<td><small><nobr>" + dt.Rows[y]["TXID"].ToString() +    "</nobr></small></tr>";
                html += div + "\r\n";
                
                oldheight = _height;

            }
            html += "</table>";
            lastReport = html;
            return html;
        }
    }
}