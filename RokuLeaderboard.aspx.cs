using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class RokuLeaderboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void btnView_Click(object sender, EventArgs e)
        {
           
        }

        private string GetTd(DataRow dr, string colname, string sAnchor)
        {
            string val = dr[colname].ToString();
            string td = "<td>" + sAnchor + val + "</a></td>";
            return td;
        }

        protected string GetLeaderboard()
        {
            string sql = "Select UserName, RandomXBBPAddress bbpaddress, rokuID, Updated, VideoCount, Sanctitude FROM RokuLeaderboard order by sanctitude desc";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th>User Name<th width=20%>BBP Address</th><th>Roku ID<th>Video Count<th>Updated<th>Sanctitude</tr>";
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string bbpaddress = dt.Rows[y]["bbpaddress"].ToString() ?? "";
                string div = "<tr><td>" + dt.Rows[y]["UserName"].ToString()
                    + "<td>" + dt.Rows[y]["bbpaddress"].ToString()
                    + "<td>" + dt.Rows[y]["rokuid"].ToString()
                    + "<td>" + dt.Rows[y]["videocount"].ToString()
                    + "<td>" + dt.Rows[y]["Updated"].ToString()
                    + "<td>" + dt.Rows[y]["sanctitude"].ToString() + "</tr>";
                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}