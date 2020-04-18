using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.StringExtension;

namespace Saved
{
    public partial class Media : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Respect the category (Current, Historical, Rapture, Guest-Pastor, etc).
        }

        public string GetNotesHTML(string data)
        {
            string[] vData = data.Split("\r\n");
            if (vData.Length < 2) return data;
            string sTitle = "<h3>" + vData[0] + "</h3><br>";

            for (int i = 1; i < vData.Length; i++)
            {
                sTitle += vData[i] + "<br>";
            }
            sTitle = sTitle.Replace("\r\n", "<br>");
            return sTitle;

        }

        public string GetMedia()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            string sql = "Select * from Rapture where category = @cat order by added desc";
            SqlCommand command = new SqlCommand(sql);

            command.Parameters.AddWithValue("@cat", category);
            DataTable dt = gData.GetDataTable(command);
            
            string html = "";
            
            bool fAdmin = gUser(this).Admin;

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sNotes = GetNotesHTML(dt.Rows[y]["Notes"].ToString());
                if (sNotes.Length > 512) sNotes = sNotes.Substring(0, 512);
                string sEditURL = "<a href=Markup.aspx?type=Rapture&id=" + dt.Rows[y]["id"].ToString() + ">Edit</a>";

                string sDiv = "<table cellpadding=20px cellspacing=20px><tr><td><video class='connect-bg' width=400 height=340 controls preload='metadata' style='background-color:black'>"
                    + "<source src='"
                    + dt.Rows[y]["URL"].ToString()
                    + "#t=7' type='video/mp4'></video><td style='padding:20px;font-size:16px;'>&nbsp;&nbsp;" + sNotes + "</td>";
                if (fAdmin)
                    sDiv += "<td>" + sEditURL + "</td>";

                sDiv += "</table>";
                html += sDiv;

            }
            return html;
        }
    }
}