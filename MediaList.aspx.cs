using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class MediaList : Page
    {

        public string GetMediaList()
        {
            string sql = "Select count(id) ct,max(id) id,Category From Rapture group by Category Order by Category";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=80%>Category</th></tr>";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                double dVidCt = GetDouble(dt.Rows[y]["ct"]);
                string sAnchor = "<a href='Media.aspx?category=" + s.Props.Category.ToString() + "'>" + s.Props.Category.ToString() + "</a>";
                if (dVidCt == 1)
                {
                    sAnchor = "<a href='Media.aspx?mediaid=" + dt.Rows[y]["id"].ToString() + "'>" + s.Props.Category.ToString() + "</a>";
                }
                string div = "<tr><td>" + sAnchor + "</td></tr>";
                html += div + "\r\n";

            }
            html += "</table>";
            return html;
        }
         
        protected void Page_Load(object sender, EventArgs e)
        {
            

        }
    }
}