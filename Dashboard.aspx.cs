using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;
using System.IO;
using System.Text;
using static Saved.Code.Common;
using System.Security.Authentication;
using System.Net;
using System.Data.SqlClient;
using System.Diagnostics;
using static Saved.Code.UICommon;

namespace Saved
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!gUser(this).LoggedIn)
            {
                MsgBox("Not Logged In", "Sorry, you must be logged in first.", this);
                return;
            }

        }

        public string GetDashboardLinks()
        {

            // Construct a Salvation gauge

            int nMyCount = 0;
            string html = "<br>My Ministry Dashboard v1.1<p><p>" + RenderGauge(250, "Salvations", nMyCount) + "<p><p>";
            html += "<table>";
            html += "";
            html +="<tr><td><p></td></tr><tr><td><a href=ContactAdd.aspx>Add a Contact</a></td></tr>";
            // List the contacts

            string sql = "Select * from contact where UserId = @userid order by LastName, FirstName";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", gUser(this).UserId);
            DataTable u = gData.GetDataTable(command);
            if (u.Rows.Count > 0)
            {

                html += "<tr><td></td></tr><tr><td>My Salvation Contacts:</td><tr><p><td></td></tr>";

                for (int i = 0; i < u.Rows.Count; i++)
                {
                    string person = NotNull(u.Rows[i]["LastName"]) + ", " + NotNull(u.Rows[i]["FirstName"]);

                    string td = "<tr><td><a href='ContactView.aspx?id=" + u.Rows[i]["id"].ToString() + "'>" + person + "</td><td>" + NotNull(u.Rows[i]["Status"]) + "</td></tr>";
                    html += td;
                }
                
            }
            
            html += "</table>";
            return html;
        }
       
    }
}