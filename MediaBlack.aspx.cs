using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.StringExtension;

namespace Saved
{
    public partial class MediaBlack : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
         
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

        public string GetUserName(string userid)
        {
            if (userid == "")
                return "";
            string sql = "Select * from USERS where id = '" + BMS.PurifySQL(userid,100) + "'";
            string username = gData.GetScalarString2(sql, "username");
            return username;
        }

        public string GetMediaCategory()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            if (category == null || category == "")
                return "";
            category = category.Substring(0, 1).ToUpper() + category.Substring(1, category.Length - 1);
            return category;
        }
        public string GetMedia()
        {
            string category = Request.QueryString["category"].ToNonNullString();
            string mediaid = Request.QueryString["mediaid"].ToNonNullString();
            double dLimited = GetDouble(Request.QueryString["limit"].ToNonNullString());

            
            string sql = "Select * from Rapture where category=@cat order by added desc";
            if (mediaid != "")
            {
                sql = "Update Rapture set ViewCount=isnull(viewcount,0)+1 where id='" + BMS.PurifySQL(mediaid, 100) + "'";
                gData.Exec(sql);
                sql = "Select * from Rapture Where ID='" + Saved.Code.BMS.PurifySQL(mediaid, 64) + "'";
            }
            string html = "";
           
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@cat", category);
            command.Parameters.AddWithValue("@id", mediaid);

            DataTable dt = gData.GetDataTable(command);
            
            bool fAdmin = gUser(this).Admin;
            bool fRelink = dt.Rows.Count > 1;

            string sTable = "<table cellpadding=20px cellspacing=20px>";
            html += sTable;
            double nLimit = 0;
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                nLimit++;
                if (dLimited > 0 && nLimit > dLimited)
                    break;
                string sNotes = GetNotesHTML(dt.Rows[y]["Notes"].ToString());
                if (sNotes.Length > 256) 
                    sNotes = sNotes.Substring(0, 256);
                string sEditURL = "<a href=Markup.aspx?type=Rapture&id=" + dt.Rows[y]["id"].ToString() + ">Edit</a>";

                string sAnchor = "<div><a href=Media.aspx?mediaid=" + dt.Rows[y]["id"].ToString() + ">";
                string sUserID = dt.Rows[y]["userid"].ToString();
                string sUserName = GetUserName(sUserID);
                double nViewCount = GetDouble(dt.Rows[y]["ViewCount"].ToNonNullString());


                string sDiv = "<tr><td width=30%>";

                if (fRelink)
                    sDiv += sAnchor;

                string sAutoPlay = !fRelink ? "autostart autoplay controls playsinline preload='auto'" : "preload='metadata'";

                sDiv += "<video class='connect-bg' width='400' height='340' " + sAutoPlay + " style='background-color:black'>";

                string sLoc = !fRelink ? "" : "#t=25";
                string sPoster = dt.Rows[y]["thumbnail"].ToString();
                sDiv += "<source src='" + dt.Rows[y]["URL"].ToString() + sLoc + "' type='video/mp4' poster='" + sPoster + "'></video>";
                if (fRelink)
                    sDiv += "</a></div>";

                sDiv += "<td style='padding:20px;font-size:16px;' width=70%>&nbsp;&nbsp;" + sNotes + "<br></td>";
                
                
                if (fAdmin)
                    sDiv += "<td>" + sEditURL + "</td>";

                sDiv += "</tr>";
                html += sDiv;

            }
            html += "</table>";
            return html;
        }
    }
}