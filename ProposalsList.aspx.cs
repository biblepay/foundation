using Microsoft.VisualBasic;
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
    public partial class ProposalsList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected string GetVote(string ID, string sAction)
        {
            string sNarr = "From the RPC console in biblepaycore, enter the following command:<br><br>gobject vote-many " + ID + " funding " + sAction;

            string sJS = "<a onclick='showModalDialog(\"" + "Voting Command" + "\",\"" + sNarr + "\", " + 900.ToString() + ", " + 300.ToString() + ");'>Vote " + sAction + "</a>";
            return sJS;
        }

        protected string GetProposalsList()
        {
            
            string sChain = IsTestNet(this) ? "test" : "main";
            //Proposals.SubmitProposals(IsTestNet(this));

            string sql = "Select * from Proposal WHERE CHAIN='" + sChain + "' and added > getdate()-30  Order by Added desc";
            DataTable dt = gData.GetDataTable2(sql);
            string html = "<table class=saved><tr>"
                +"<th>UserName<th>Expense Type<th>Proposal Name<th>Amount<th>PrepareTXID<th>URL<th>Chain<th>Updated<th>SubmitTXID<th>Vote Yes<th>Vote No</tr>\r\n";

            
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sURLAnchor = "<a href='" + dt.Rows[y]["URL"].ToString() + "' target=_blank>View Proposal</a>";

                string sID = dt.Rows[y]["SubmitTXID"].ToString();
                string div = "<tr>"
                    + "<td>" + dt.Rows[y]["UserName"].ToString()
                    + "<td>" + dt.Rows[y]["ExpenseType"].ToString()
                    + "<td>" + dt.Rows[y]["Name"].ToString()
                    + "<td>" + dt.Rows[y]["Amount"].ToString();
                div += "<td><small>" 
                    + Strings.Mid(ToNonNull(dt.Rows[y]["PrepareTXID"]), 1, 10)
                    + "</small>"
                    + "<td>" + sURLAnchor 
                    + "<td>" + dt.Rows[y]["Chain"].ToString()
                    + "<td>" + dt.Rows[y]["Updated"].ToString()
                + "<td><small>" + dt.Rows[y]["SubmitTXID"].ToString() + "</small>"
                +"<td>" + GetVote(sID, "yes") + "<td>" + GetVote(sID, "no");

                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}