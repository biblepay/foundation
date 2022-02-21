using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.Fastly;

namespace Saved
{
    public partial class Admin1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
            {
                CoerceUser(Session);
            }

            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }
        }
        


        protected void btnConvert_Click(object sender, EventArgs e)
        {
            string test = Saved.Code.PoolCommon.GetChartOfSancs();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                Log("Clicked save from non admin");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }
            return;
        }

        private void ResubmitZinc(string oldid)
        {
            string sql = "Select * from Orders where id = '" + BMS.PurifySQL(oldid,20) + "'";
            DataTable oldOrder = gData.GetDataTable2(sql);
            if (oldOrder.Rows.Count < 1)
                return;

            sql = "Select * from products where id='" + BMS.PurifySQL(oldOrder.Rows[0]["productid"].ToString(),40) + "'";
            DataTable dtProd = gData.GetDataTable2(sql);
            if (dtProd.Rows.Count < 1)
                return;

            ZincOps.zinc_address zTo = ZincOps.GetDeliveryAddress(oldOrder.Rows[0]["addressbookid"].ToString());
            string sProductGuid = oldOrder.Rows[0]["productid"].ToNonNullString();
            string sProductID = dtProd.Rows[0]["product_id"].ToNonNullString();
            string sOrderID = Guid.NewGuid().ToString();
            double nMaxPrice = 25;
            DACResult r = ZincOps.Zinc_CreateOrder(zTo, nMaxPrice, sProductID, sOrderID);
            if (r.sError != "")
            {
                MsgBox("Buying Error", "Sorry, the purchase Failed.  Exception: " + r.sError + ".  You have not been charged.  ", this);
            }
            else
            {
                double dPriceUSD = GetDouble(dtProd.Rows[0]["price"]) / 100;

                string sNotes = "Store purchase: " + dtProd.Rows[0]["Title"] + ", Item: "
                    + dtProd.Rows[0]["product_id"] + ", Amount: $" + DoFormat(dPriceUSD);
                double nPriceBBP = GetDouble(oldOrder.Rows[0]["bbpprice"]);

                string sStatus = "PROCESSING";
                string sql1 = "Insert into Orders (id, retailer, productid, addressbookid, status, added, updated, notes, zincid, userid, bbpprice) values ('"
                    + sOrderID + "','AMAZON','"
                    + BMS.PurifySQL(sProductGuid, 256) + "','"
                    + BMS.PurifySQL(oldOrder.Rows[0]["addressbookid"].ToString(), 100) + "','"
                    + sStatus + "',getdate(),getdate(),null,'" + r.sResult
                    + "','" + gUser(this).UserId.ToString() + "','" + nPriceBBP.ToString() + "')";
                gData.Exec(sql1);
            }
        }
        protected void btnPDF_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }
        }
    }
}