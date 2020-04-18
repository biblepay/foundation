using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Illustrations : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
        public string GetArticles()
        {
            string type = Request.QueryString["type"] ?? "";
            string prefix = "";
            string table = type == "wiki" ? "wiki" : "illustrations";
            string sql = "Select * from " + table + " order by Name,Description";
            DataTable dt = gData.GetDataTable(sql);
            string html = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sArticle = dt.Rows[i]["Name"].ToString();
                string sDesc = dt.Rows[i]["Description"].ToString();
                string sNarr = sArticle + " - " + sDesc;
                string sURL = "";
                if (type == "wiki")
                {
                    sURL = sArticle;
                    sNarr = sDesc;
                    prefix = "<h3>Wiki Theological Articles</h3><br>&nbsp;<p><p><p><p>";
                }
                else
                {
                    sURL = System.Web.HttpUtility.UrlEncode(dt.Rows[i]["Url"].ToString());
                    prefix = "<h3>All credit for these Illustrations goes to <a href = 'https://www.freebibleimages.org/illustrations/'> Free Bible Images </ a> !</h3><br/>"
                        + "<br/><small><font color=red>NOTE:  After choosing one below, Please click on the \"VIEW SLIDESHOW\" button to see the Narrative along with the images.<br/></small>"
                        + "</font></h3><br>&nbsp;<p><p>";
                }
                string URL2 = "<a href='Viewer.aspx?target=" + sURL + "'>" + sNarr + "</a>";
                string row = URL2 + "<br>";
                html += row;
            }
            string output = prefix + html;
            return output;
        }
    }
}