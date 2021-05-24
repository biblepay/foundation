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
    public partial class NFTList : Page
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

        protected string GetMyNFTs(Page p)
        {
            string sql = "Select * from MyNFT Where userid='" + gUser(p).UserId.ToString() + "' order by added";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>Added<th>BBP Address<th>Amount<th>Lo Quality URL<th>Hi Quality URL</tr>";
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string bbpaddress = dt.Rows[y]["bbpaddress"].ToString() ?? "";
                string sLoQualityURL = "<a href='" + dt.Rows[y]["loqualityurl"].ToString() + "'>Low Quality URL</a>";
                string sHiQualityURL = "<a href='" + dt.Rows[y]["hiqualityurl"].ToString() + "'>Hi Quality URL</a>";

                string div = "<tr>"
                    + "<td>" + dt.Rows[y]["added"].ToString()
                    + "<td>" + bbpaddress
                    + "<td>" + dt.Rows[y]["amount"].ToString()
                    + "<td>" + sLoQualityURL
                    + "<td>" + sHiQualityURL
                    + "</tr>";
                html += div + "\r\n";

            }
            html += "</table>";

            return html;
        }
    }
}