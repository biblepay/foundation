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
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class BuyItem : Page
    {

        private string DEC(string sData)
        {
            string e = BiblePayCommon.Encryption.DecryptAES256(sData, GetBMSConfigurationKeyValue("QEV2"));
            return e;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Debugger.IsAttached)
                CoerceUser(Session);



            if (!gUser(this).LoggedIn)
            {
                MsgBox("Error", "Sorry, you must be logged in.", this);
            }

            string sID = Request.QueryString["buyid"].ToNonNullString();
            if (sID == "")
            {
                MsgBox("Error", "Item not specified.", this);
            }

            //Populate ddDeliveryAddress
            string sql = "Select * from AddressBook where userid='" + BMS.PurifySQL(gUser(this).UserId.ToString(),20) + "'";
            DataTable dt = gData.GetDataTable2(sql);
            if (dt.Rows.Count < 1)
            {
                MsgBox("Error", "Sorry, you must have at least 1 delivery address stored in your Address Book.  Click <a href=AddressBook>here</a> to add an address book entry.", this);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ListItem j = new ListItem();
                j.Text = DEC(dt.Rows[i]["LastName"].ToString())
                    + ", " + DEC(dt.Rows[i]["FirstName"].ToString())
                    + ", " + DEC(dt.Rows[i]["AddressLine1"].ToString())
                    + ", " + DEC(dt.Rows[i]["City"].ToString())
                    + ", " + DEC(dt.Rows[i]["State"].ToString())
                    + ", " + DEC(dt.Rows[i]["PostalCode"].ToString());
                j.Value = dt.Rows[i]["id"].ToString();
                ddDeliveryAddress.Items.Add(j);
            }
        }

        public string GetAmzItem(bool fBuying)
        {
            string sID = Request.QueryString["buyid"].ToNonNullString();
            string sql = "Select * from Products Where id='" + BMS.PurifySQL(sID, 200) + "' and deleted=0";
            DataTable dt = gData.GetDataTable2(sql);
            if (dt.Rows.Count < 1)
                return "";
            string div = ZincOps.GetAmazonItem(dt.Rows[0], fBuying);
            return div;
        }

        protected void btnBuy_Click(object sender, EventArgs e)
        {

            string sID = Request.QueryString["buyid"].ToNonNullString();
            string sql = "Select * from Products Where id='" + BMS.PurifySQL(sID, 200) + "' and deleted=0";
            DataTable dt = gData.GetDataTable2(sql);
            if (dt.Rows.Count < 1)
            {
                MsgBox("Error", "Item no longer available.", this);
            }
            double dPriceUSD = GetDouble(dt.Rows[0]["Price"].ToString()) / 100;
            double nSaleAmount = GetDouble(GetBMSConfigurationKeyValue("amazonsale"));

            double nPriceBBP = GetBBPAmountDouble(dPriceUSD, nSaleAmount);

            if (dPriceUSD < 1)
            {
                MsgBox("Error", "This items price is not correct.  ", this);
            }
            
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            if (gUser(this).TwoFactorAuthorized == false || gUser(this).Require2FA != 1)
            {
                MsgBox("Two Factor Not Enabled", "Sorry, you cannot spend unless you enable two factor authorization.  Please go to the Account Edit page to enable 2FA. ", this);
                return;
            }

            // Amazon Promotion $5
            double nUnlockAmazon = gData.GetScalarDoubleFromObject("Users", "UnlockAmazon", gUser(this).UserId);
            double nMax = GetBBPAmountDouble(5);

            double nBal = DataOps.GetUserBalance(gUser(this).UserId.ToString());

            if (nUnlockAmazon == 1 && (nPriceBBP <= nMax || nBal+nMax >= nPriceBBP))
            {
                string sql2 = "Update Users set unlockamazon=2 where id = '" + gUser(this).UserId.ToString() + "'";
                gData.Exec(sql2);
                // End of promotion
                if (nPriceBBP < nMax)
                    nMax = nPriceBBP;
                string sNarr = "Promotional Store Credit";
                DataOps.AdjBalance(nMax + 100, gUser(this).UserId.ToString(), sNarr);
                Log("Giving away promotional store credit to " + gUser(this).EmailAddress);
            }

            if (nBal == 0 || nBal < 1 || nPriceBBP > nBal || nPriceBBP > 20000000)
            {
                MsgBox("Insufficient Funds", "Sorry, the amount requested exceeds your balance.", this);
                return;
            }

            // Buy then adjust
            string deliveryid = ddDeliveryAddress.SelectedValue;

            ZincOps.zinc_address zTo = ZincOps.GetDeliveryAddress(deliveryid);
            string sProductID = dt.Rows[0]["product_id"].ToNonNullString();
            if (sProductID == "")
            {
                MsgBox("Error", "Unable to find item.", this);
            }
            string sOrderID = Guid.NewGuid().ToString();
            double nMaxPrice = Math.Round(dPriceUSD + 5, 0);

            

            DACResult r = ZincOps.Zinc_CreateOrder(zTo, nMaxPrice, sProductID, sOrderID);
            if (r.sError != "")
            {
                MsgBox("Buying Error", "Sorry, the purchase Failed.  Exception: " + r.sError + ".  You have not been charged.  ", this);
            }
            else
            {
                string sNotes = "Store purchase: " + dt.Rows[0]["Title"] + ", Item: " + dt.Rows[0]["product_id"] + ", Amount: $" + DoFormat(dPriceUSD);

                DataOps.AdjBalance(-1 * nPriceBBP, gUser(this).UserId.ToString(), sNotes);
                string sStatus = "PROCESSING";
                string sql1 = "Insert into Orders (id, retailer, productid, addressbookid, status, added, updated, notes, zincid, userid, bbpprice) values ('"
                    + sOrderID + "','AMAZON','" 
                    + BMS.PurifySQL(sID, 256) + "','" 
                    + BMS.PurifySQL(ddDeliveryAddress.SelectedValue.ToString(), 100) + "','" 
                    + sStatus + "',getdate(),getdate(),null,'" + r.sResult + "','" + gUser(this).UserId.ToString() + "','" + nPriceBBP.ToString() + "')";
                gData.Exec(sql1);

                MsgBox("Success", "You have successfully purchased the item [" + dt.Rows[0]["Title"] 
                    + "].  To track this order, simply navigate to <a href='MyOrders'>My Orders</a>.  Thank you for shopping with BiblePay.  ", this);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Storefront");
        }
    }
}