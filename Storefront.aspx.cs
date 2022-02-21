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
    public partial class Storefront : Page
    {
        private string _country = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            _country = Request.QueryString["Country"] ?? "";
            if (_country == "")
                _country = "US";


        }


        protected void btnAdd_Click(object sender, EventArgs e)
        {
            DACResult r = ZincOps.Zinc_RealTimeProductQuery(txtAdd.Text, _country);
            if (r.sError != "")
            {
                MsgBox("Error", "We encountered an error while adding this product.  " + r.sError, this);
            }

        }

        protected string GetSaleNarrative()
        {
            double nAmount = GetDouble(GetBMSConfigurationKeyValue("amazonsale"));
            if (nAmount > 0)
            {
                string sNarr = "<font color=red>" + nAmount.ToString() + "% Off Sale!  Hurry before it ends!</font>";
                return sNarr;
            }
            return "";
        }

        protected string GetStorefront()
        {

            string sql = "Select * from Products WHERE deleted=0 order by Title";
            sql = "Select * from Products where deleted=0 Order by Title";

            DataTable dt = gData.GetDataTable2(sql);
            string html = "<table><tr>";
            int iTD = 0;

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                string div = ZincOps.GetAmazonItem(dt.Rows[y], false);
                html += div + "\r\n";
                iTD++;
                if (iTD == 3)
                {
                    iTD = 0;
                    html += "</tr><tr>";
                }

            }
            // Check for quizzes
            html += "</table>";

            return html;
        }
    }
}