using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.Common;

namespace Saved.Code
{
    public static class ZincOps
    {


        public static void NotifyOfMissingProduct(string sToEmail, string sProductID)
        { 
            try
            {
                MailAddress r = new MailAddress("rob@saved.one", "The BiblePay Team");
                MailAddress t = new MailAddress(sToEmail, sToEmail);
                MailAddress bcc = new MailAddress("rob@biblepay.org", "Rob Andrews");
                MailMessage m = new MailMessage(r, t);
                m.Bcc.Add(bcc);
                string sql = "Update Products set deleted=1 where product_id='" + sProductID + "'";
                gData.Exec(sql);
                string sql1 = "Select * from products where product_id='" + BMS.PurifySQL(sProductID,100) + "'";
                string sTitle = gData.GetScalarString2(sql1, "Title");
                m.Subject = "[Amazon Storefront] Product " + sProductID + " is no longer available.";
                string sBody = "<br>Dear " + sToEmail + ",<br><br>We encountered an error while processing your order. ";
                sBody += "This product [" + sProductID + "] [" + sTitle + "] is no longer available.  <br><br>We have deleted the product from our database. <br><br> ";
                sBody += "<br><br>Your account has been CREDITED in the amount of the purchase, now your buying power is available again!";
                sBody += "<br><br>Thank you for using Biblepay.  <br><br>Sincerely Yours,<br>The BiblePay Team";
                m.IsBodyHtml = true;
                m.Body = sBody;
                SendMail(m);
            }
            catch(Exception ex)
            {
                Log("NotifyOfMissingProduct::" + ex.Message);
            }
        }

        private static string DEC(string sData)
        {
            string e = BiblePayCommon.Encryption.DecryptAES256(sData, GetBMSConfigurationKeyValue("QEV2"));
            return e;
        }

        public static ZincOps.zinc_address GetDeliveryAddress(string id)
        {
            string sql = "Select * from AddressBook Where id='" + BMS.PurifySQL(id, 200) + "'";
            DataTable dt = gData.GetDataTable2(sql);
            ZincOps.zinc_address z = new ZincOps.zinc_address();
            if (dt.Rows.Count < 1)
                return z;
            z.last_name = DEC(dt.Rows[0]["lastname"].ToNonNullString());
            z.first_name = DEC(dt.Rows[0]["firstname"].ToNonNullString());
            z.address_line1 = DEC(dt.Rows[0]["addressLine1"].ToNonNullString());
            z.address_line2 = DEC(dt.Rows[0]["addressLine2"].ToNonNullString());
            z.city = DEC(dt.Rows[0]["city"].ToNonNullString());
            z.state = DEC(dt.Rows[0]["state"].ToNonNullString());
            z.zip_code = DEC(dt.Rows[0]["postalCode"].ToNonNullString());
            return z;
        }

        public struct zinc_product_item
        {
            public string product_id;
            public int quantity;
        };
        public struct zinc_address
        {
            public string first_name;
            public             string last_name;
            public             string address_line1;
            public             string address_line2;
            public             string zip_code;
            public             string city;
            public             string state;
            public             string country;
            public             string phone_number;
        };
        public struct zinc_shipping_info
        {
            public string order_by;
            public             int max_days;
            public             int max_price;
        };
        public struct zinc_payment_method
        {
            public string name_on_card;
            public             string number;
            public             string security_code;
            public             int expiration_month;
            public             int expiration_year;
            public             bool use_gift;
        };
        public struct zinc_retailer_credentials
        {
            public             string email;
            public             string password;
            public             string totp_2fa_key;
        };
        public struct zinc_webhooks
        {
            public string request_succeeded;
            public             string request_failed;
            public             string tracking_obtained;
        };
        public struct zinc_client_notes
        {
            public string our_internal_order_id;
            public             string any_other_field;
        };
        public struct ZincOrder
        {
            public string retailer;
            public             List<zinc_product_item> products;
            public             int max_price;
            public             zinc_address shipping_address;
            public             bool is_gift;
            public             string gift_message;

            //public             zinc_shipping_info shipping;

            public             zinc_payment_method payment_method;
            public             zinc_address billing_address;
            public             zinc_retailer_credentials retailer_credentials;
            public             zinc_webhooks webhooks;
            public             zinc_client_notes client_notes;
            public string shipping_method;
        };


        public static string GetAmazonItem(DataRow dr, bool fBuying)
        {
            int h = 250;
            int w = 350;
            string sButton = "<input type='button' onclick=\"location.href='BuyItem?buyid="
                    + dr["id"].ToString() + "'\" id='buy" + dr["id"] + "' value='Buy It Now' />";

            string sPreview = "<a id='a1' href='" + dr["OriginalURL"].ToString() + "' target='_blank'><input type='button' id='prv" + dr["id"] + "' value='Preview' /></a>";
            double nSaleAmount = GetDouble(GetBMSConfigurationKeyValue("amazonsale"));
            double nBBP = GetBBPAmountDouble(GetDouble(dr["price"].ToString()) / 100, nSaleAmount);

            string sUSD = DoFormat(GetDouble(dr["price"].ToString()) / 100, nSaleAmount);

            string sBuyItNowPrice = "$" + sUSD + " • " + nBBP.ToString() + " BBP";

            string sAsset = "<img style='height:" + h.ToString() + "px;width:" + w.ToString() + "px;' src='"
            + dr["image"].ToString() + "'/>";
            string sScrollY = "overflow-y:auto;";
            string s1 = "<td style='padding:12px;border:1px solid white' cellpadding=12 cellspacing=12>"
                + "<b>" + dr["product_id"].ToString() + "</b><br>" + sAsset
                + "<br><div style='height:" + (h / 3).ToString() + "px;width:" + w.ToString() + "px;" + sScrollY + "'><font style='font-size:11px;'>"
                + dr["Title"] + "</font></div><br><small><font color=green>" + sBuyItNowPrice + "</small>";

            if (fBuying)
            {
                s1 += "</td>";
            }
            else
            {
                s1 += "<br>" + sButton + "&nbsp;" + sPreview + "</td>";
            }
            return s1;
        }



        public static Chilkat.Rest ConnectToZinc()
        {
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            string sURL = "api.zinc.io"; 
            success = rest.Connect(sURL, port, bTls, bAutoReconnect);
            if (success != true)
            {
                Log("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Log(rest.LastErrorText);
                return null;
            }
            
            string sKey = GetBMSConfigurationKeyValue("zincprod");
            rest.SetAuthBasic(sKey, "");
            return rest;
        }

        public static void CacheProduct(dynamic o, string sOriginalURL, string sCountry)
        {
            if (sOriginalURL == "")
                return;
            if (o["product_id"] == null)
                return;
            if (o["product_id"].Value.Length < 3)
                return;

            string sql1 = "Select count(*) ct from Products where product_id='" + o["product_id"].Value + "'";
            double dCt = gData.GetScalarDouble(sql1, "ct");
            if (dCt > 0)
            {
                return;
            }
            string sql = "Delete from Products where product_id='" + o["product_id"].Value + "'"
                +"\r\nInsert Into Products (id, added, deleted, product_id, title, image, brand, price, stars, shipping, originalURL, Country) values "
             + "(newid(), getdate(), '0', @productid, @title,@image,@brand,@price,@stars,@shipping, @originalURL, @Country)";

            try
            {
                bool fPrime = o["prime"].Value;
                if (!fPrime)
                    return;

                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@productid", o["product_id"].Value);
                command.Parameters.AddWithValue("@title", o["title"].Value);
                command.Parameters.AddWithValue("@image", o["image"].Value);
                command.Parameters.AddWithValue("@brand", o["brand"].Value);
                command.Parameters.AddWithValue("@price", o["price"].Value);
                command.Parameters.AddWithValue("@stars", o["stars"].Value);
                command.Parameters.AddWithValue("@originalURL", sOriginalURL);
                command.Parameters.AddWithValue("@country", sCountry);

                command.Parameters.AddWithValue("@shipping", 0);
                gData.ExecCmd(command, false, false, true);
            }catch(Exception ex)
            {
                Log("CacheProduct::" + ex.Message);
            }
        }

        public static string GetJsonValue(dynamic oJson, string RootField, string sSubField)
        {

            if (oJson == null)
                return "";

            if (sSubField == "")
            {
                if (oJson[RootField] == null)
                    return "";
                string o3 = oJson[RootField].Value ?? "";
                return o3;
            }

            dynamic o1 = oJson[RootField];
            if (o1 == null)
                return "";
            string o2 = (o1[0][sSubField].Value ?? "").ToString();

            return o2;
        }
        public static DACResult Zinc_QueryOrderStatus(string sZincID)
        {
            DACResult r = new DACResult();
            r.sError = "";
            try
            {
                Chilkat.Rest rest = ConnectToZinc();
                string jsonOrder = "";
                string sResponse = rest.FullRequestString("GET", "/v1/orders/" + sZincID, jsonOrder);
                if (rest.LastMethodSuccess != true)
                {
                    Log(rest.LastErrorText);
                    r.sError = "Unable to interface with AMAZON.";
                    return r;
                }
                dynamic oJson = JsonConvert.DeserializeObject<dynamic>(sResponse);
                // Tracking
                string sDeliveryDate = GetJsonValue(oJson, "delivery_dates", "date");
                string sTrackingURL = GetJsonValue(oJson, "merchant_order_ids", "tracking_url");
                if (sTrackingURL != "")
                {
                    sTrackingURL = sZincID;
                }


                string sCode = GetJsonValue(oJson, "code", "");

                string msg = GetJsonValue(oJson, "message", "");
                msg = msg.Replace("'", "");

                string sStatus = "";

                if (sCode == "aborted_request")
                {
                    sStatus = "COMPLETED";
                    sTrackingURL = sCode;
                }
                else if (msg == "One of the products you selected is unavailable.")
                {
                    sStatus = "COMPLETED";
                    sTrackingURL = "CUSTOMER REFUNDED";
                    // Credit the user the amount
                    string sMySql = "Select * from Orders where ZincID = '" + BMS.PurifySQL(sZincID,40) + "'";
                    DataTable dtRefunded = gData.GetDataTable2(sMySql);
                    if (dtRefunded.Rows.Count > 0)
                    {
                        double nPriceBBP = GetDouble(dtRefunded.Rows[0]["bbpprice"]);
                        string sSql2 = "Select product_id from products where id='" + dtRefunded.Rows[0]["productid"].ToString() + "'";
                        string sProdID = gData.GetScalarString2(sSql2, "product_id");
                        string sNotes = "Full refund for unavailable product for product id " + sProdID;
                        DataOps.AdjBalance(1 * nPriceBBP, DataOps.GetUserRecord(dtRefunded.Rows[0]["UserId"].ToString()).UserId.ToString(), sNotes);
                        NotifyOfMissingProduct(DataOps.GetUserRecord(dtRefunded.Rows[0]["UserId"].ToString()).EmailAddress, sProdID);

                    }
                }
                else if (sTrackingURL != "")
                {
                    sStatus = "OUT_FOR_DELIVERY";
                    if (sDeliveryDate != "")
                    {
                        System.TimeSpan diffResult = System.DateTime.Now - Convert.ToDateTime(sDeliveryDate);
                        if (diffResult.TotalHours > 1)
                        {
                            sStatus = "COMPLETED";

                        }
                    }

                }


                if (r.sResult != "" || true)
                {
                    string sql = "Update Orders Set Updated=getdate(),DeliveryDate='" + sDeliveryDate + "', TrackingNumber='" + sTrackingURL + "',Message='" 
                        + msg + "',Status = '" + sStatus + "' where ZincID = '" + sZincID + "' and status <> 'COMPLETED'";
                    gData.Exec(sql);

                }

                return r;
            }
            catch (Exception ex)
            {
                r.sError = "Unable to find product.";
                return r;
            }
        }
        
        public static DACResult Zinc_RealTimeProductQuery(string myItem, string sCountry)
        {
            DACResult r = new DACResult();
            r.sError = "";
            try
            {
                Chilkat.Rest rest = ConnectToZinc();
                string jsonOrder = "";
                string[] q = myItem.Split("/");
                string sOriginalURL = "";
                string sSearchProductID = "";
                for (int i = 0; i < q.Length; i++)
                {
                    sOriginalURL += q[i] + "/";
                    if (q[i].Length == 10)
                    {
                        sSearchProductID = q[i];
                        break;
                    }
                }
                string sRetailer = "amazon";
                if (sCountry == "UK")
                {
                    sRetailer = "amazon_uk";
                }
                else
                {
                    sRetailer = "amazon";
                }
                string sResponse = rest.FullRequestString("GET", "/v1/search?query=" + HttpUtility.UrlEncode(sSearchProductID) + "&retailer=" + sRetailer, jsonOrder);
                if (rest.LastMethodSuccess != true)
                {
                    Log(rest.LastErrorText);
                    r.sError = "Unable to interface with AMAZON.";
                    return r;
                }
                dynamic oJson = JsonConvert.DeserializeObject<dynamic>(sResponse);
                if (oJson["results"] == null)
                {
                    r.sError = "Unable to find item";
                    return r;
                }
                
                foreach (var j in oJson["results"])
                {
                    // if its not prime or its pantry; give them a nice error
                    bool fFresh = j["fresh"].Value;
                    bool fPrime = j["prime"].Value;
                    bool fPantry = j["pantry"].Value;
                    if (fFresh || fPantry)
                    {
                        r.sError = "Sorry, we are not interfacing with Fresh or Pantry items yet. ";
                        return r;
                    }
                    if (!fPrime)
                    {
                        r.sError = "Sorry, this item is not amazon prime.  Please, only add Amazon Prime items so we can offer free shipping. ";
                        return r;
                    }
                    CacheProduct(j, sOriginalURL, sCountry);
                    r.sResult = sOriginalURL;
                    return r;
                }
                r.sResult = sOriginalURL;
                return r;
            }
            catch(Exception ex)
            {
                r.sError = "Unable to find product.";
                return r;
            }
        }

        public static string CountryToRetailer(string sCountry)
        {
            string sRetailer = "";
            if (sCountry == "US")
            {
                sRetailer = "amazon";
            }
            else if (sCountry == "UK")
            {
                sRetailer = "amazon_uk";
            }
            else
            {
                sRetailer = "amazon";
            }
            return sRetailer;
        }
        public static DACResult Zinc_CreateOrder(zinc_address toShippingAddress, double nMaxPriceUSD, string sAmzProductId, string sOrderID)
        {
            DACResult r = new DACResult();
            return r;
        }

    }
}
