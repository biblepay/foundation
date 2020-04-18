using System;
using System.Data;
using System.Web.UI;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Study : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
        public string GetArticles()
        {
            string sql = "Select * from Articles order by Description";
            DataTable dt = gData.GetDataTable(sql);
            string html = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sArticle = dt.Rows[i]["Name"].ToString();
                string row = "<a href=Viewer.aspx?ref=" + sArticle + ">" + dt.Rows[i]["Description"].ToString() + "</a><br>";
                html += row;
            }
            return html;
        }
    }
}