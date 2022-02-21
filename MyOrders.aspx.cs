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
    public partial class MyOrders : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void UpdOrders()
        {
            string sql = "Select * from Orders Where status <> 'COMPLETED' and Updated < getdate()-.1";
            DataTable dt = gData.GetDataTable2(sql);
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sZincID = dt.Rows[y]["zincID"].ToNonNullString();
                DACResult r = ZincOps.Zinc_QueryOrderStatus(sZincID);
                string test = "";
            }
        }

        protected string GetOrders()
        {

            UpdOrders();
            string sql = "Select * from Orders Inner Join Products p on p.id = orders.productid inner join AddressBook Ab on ab.Id = orders.addressbookid "
                + " WHERE orders.userid='" + BMS.PurifySQL(gUser(this).UserId.ToString(),50) + "' order by Orders.Added desc";
            DataTable dt = gData.GetDataTable2(sql);
            string html = "<table class=saved><tr><th>ID<th>Retailer</th><th>Item ID<th>Title<th>Price USD<th>Price BBP<th>Ship-To-Address<th>Status<th>Delivery ETA<th>Tracking No<th>Added<th>Updated</tr>\r\n";
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string sAddressBook = "<a href=addressbook?id=" + dt.Rows[y]["AddressBookID"].ToString() + ">"
                    + dt.Rows[y]["LastName"] + ", " + dt.Rows[y]["FirstName"] + ", " 
                    + dt.Rows[y]["AddressLine1"] + ", " + dt.Rows[y]["PostalCode"].ToString() + "</a>";

                double nUSD = GetUSDAmountFromBBP(GetDouble(dt.Rows[y]["bbpprice"]));

                string div = "<tr>"
                    + "<td>" + dt.Rows[y]["ID"].ToString()
                    + "<td>" + dt.Rows[y]["Retailer"].ToString()
                    + "<td>" + dt.Rows[y]["product_id"].ToString()
                    + "<td>" + dt.Rows[y]["Title"].ToString()
                    + "<td>$" + DoFormat(nUSD)
                    + "<td>" + dt.Rows[y]["bbpprice"].ToString() + " BBP"
                    + "<td>" + sAddressBook 
                    + "<td>" + dt.Rows[y]["Status"].ToString()
                    + "<td>" + dt.Rows[y]["DeliveryDate"].ToString()
                    + "<td>" + dt.Rows[y]["TrackingNumber"].ToString()
                    + "<td>" + dt.Rows[y]["Added"].ToString()
                    + "<td>" + dt.Rows[y]["Updated"].ToString();
                html += div + "\r\n";
            }
            // Check for quizzes
            html += "</table>";


            return html;
        }
    }
}