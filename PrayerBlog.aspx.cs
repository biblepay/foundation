﻿using Saved.Code;
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
    public partial class PrayerBlog : Page
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

        protected string GetPrayerBlogs()
        {

            int nHeight = Common.GetHeight();

            string sql = "Select * from PrayerRequest Inner Join Users on Users.ID = PrayerRequest.UserID order by PrayerRequest.Added desc";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th width=20%>User</th><th width=20%>Added<th width=50%>Subject";

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string sUserName = NotNull(s.Props.UserName);
                if (sUserName == "")
                    sUserName = "N/A";
                string sAnchor = "<a href='PrayerView.aspx?id=" + s.Props.id.ToString() + "'>";

                string div = sAnchor + "<tr><td>" + GetAvatar(s.Props.Picture) + "&nbsp;" 
                    + sUserName + "</td>" + GetTd(dt.Rows[y], "Added", sAnchor) 
                    + GetTd(dt.Rows[y],"subject", sAnchor) + "</tr>";
                html += div + "\r\n";

            }
            html += "</table>";
            return html;
        }
    }
}