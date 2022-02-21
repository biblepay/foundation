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
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.Common;

namespace Saved.Code
{
    public static class StripeOps
    {

        public static string ListCards1(bool fTestnet)
        {
            Chilkat.Rest rest = ConnectToLithic(fTestnet);
            string s  = rest.FullRequestString("GET", "/v1/card", "");
            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            dynamic oJson = JsonConvert.DeserializeObject<dynamic>(s);
            foreach (var card in oJson["data"])
            {
                string token = card["token"].Value;
                return token;
            }
            return "";
        }

        public struct cardobjectlithic
        {
            public string memo;
            public string type;
            public int spend_limit;
            public string spend_limit_duration;
        };

        public static Chilkat.Rest ConnectToLithic(bool fTestnet)
        {
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            string sURL = fTestnet ? "sandbox.lithic.com" : "api.lithic.com";
            success = rest.Connect(sURL, port, bTls, bAutoReconnect);
            if (success != true)
            {
                Log("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Log(rest.LastErrorText);
                return null;
            }
            string lithic_testnet = GetBMSConfigurationKeyValue("lithic_testnet");
            string lithic_prod = GetBMSConfigurationKeyValue("lithic_prod");

            string auth = "Authorization: api-key " + lithic_testnet;
            string sKey = fTestnet ? "api-key " + lithic_testnet : "api-key " + lithic_prod;
            rest.AddHeader("Authorization", sKey);
            rest.AddHeader("Content-Type", "application/json");
            return rest;

        }

        public struct panObject
        {
            public string pan;
            public string descriptor;
            public double amount;
        };
        public static void SimulateAuth_Lithic(bool fTestnet)
        {
            Chilkat.Rest rest = ConnectToLithic(fTestnet);
            panObject p = new panObject();
            p.amount = 500;
            p.descriptor = "AMAZON.COM";
            string json = JsonConvert.SerializeObject(p, Formatting.Indented);
            string sResponse = rest.FullRequestString("POST", "/v1/simulate/authorize", json);
            if (rest.LastMethodSuccess != true)
            {
                Log(rest.LastErrorText);
                return;
            }
            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.Load(sResponse);
        }


        public static string GetHMAC(string text, string key)
        {
            key = key ?? "";
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hash);
            }
        }

        public struct embedRequest
        {
            public string embed_request;
            public string hmac;
        };
        public struct embedSubRequest
        {
            public string token;
            public string css;
        };

        public static string Lithic_GetCardEmbedCode(bool fTestnet, string sCardGuid)
        {
            Chilkat.Rest rest = ConnectToLithic(fTestnet);
            embedRequest e = new embedRequest();
            e.embed_request = sCardGuid;
            string lithic_testnet = GetBMSConfigurationKeyValue("lithic_testnet");
            string lithic_prod = GetBMSConfigurationKeyValue("lithic_prod");
            string myKey = fTestnet ? lithic_testnet : lithic_prod;
            embedSubRequest esr = new embedSubRequest();
            esr.token = sCardGuid;
            esr.css = "";
            string embedRequestJson = JsonConvert.SerializeObject(esr);
            e.hmac = GetHMAC(embedRequestJson, myKey);
            string json = JsonConvert.SerializeObject(e, Formatting.Indented);
            string sResponse = rest.FullRequestString("POST", "/v1/embed/card", json);
            if (rest.LastMethodSuccess != true)
            {
                Log(rest.LastErrorText);
                return "";
            }
            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.Load(sResponse);
            return "?";
        }


        public static void CreateCard_Lithic(bool fTestnet)
        {
            Chilkat.Rest rest = ConnectToLithic(fTestnet);
            cardobjectlithic c = new cardobjectlithic();
            c.memo = "Memo1";
            c.spend_limit = 200 * 100;
            c.spend_limit_duration = "TRANSACTION";

            c.type = "MERCHANT_LOCKED";
            string json = JsonConvert.SerializeObject(c, Formatting.Indented);
            string sResponse = rest.FullRequestString("POST", "/v1/card", json);
            if (rest.LastMethodSuccess != true)
            {
                Log(rest.LastErrorText);
                return;
            }
            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.Load(sResponse);
            string mytest = "";
        }

        public static void CreateCardholder()
        {
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            //  URL: https://api.stripe.com/v1/balance
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            success = rest.Connect("api.stripe.com", port, bTls, bAutoReconnect);
            if (success != true)
            {
                Log("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Log(rest.LastErrorText);
                return;
            }
            //rest.SetAuthBasic(stripe_testnet, "");
            rest.AddQueryParam("type", "individual");
            rest.AddQueryParam("name", "John Doe");
            rest.AddQueryParam("billing[address][line1]", "1008 Their Address St.");
            rest.AddQueryParam("billing[address][city]", "City");
            rest.AddQueryParam("billing[address][state]", "St");
            rest.AddQueryParam("billing[address][country]", "US");
            rest.AddQueryParam("billing[address][postal_code]", "Zip");
            string strResponseBody = rest.FullRequestFormUrlEncoded("POST", "/v1/issuing/cardholders");
            if (rest.LastMethodSuccess != true)
            {
                Log(rest.LastErrorText);
                return;
            }

            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.Load(strResponseBody);
        }
        public static void ListAllCardholders()
        {
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            //  URL: https://api.stripe.com/v1/balance
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            success = rest.Connect("api.stripe.com", port, bTls, bAutoReconnect);
            if (success != true)
            {
                Log("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Log(rest.LastErrorText);
                return;
            }
            Chilkat.StringBuilder sbResponseBody = new Chilkat.StringBuilder();
            success = rest.FullRequestNoBodySb("GET", "/v1/issuing/cardholders", sbResponseBody);
            if (success != true)
            {
                Log(rest.LastErrorText);
                return;
            }
            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.LoadSb(sbResponseBody);
            var  o = jsonResponse.StringOf("object");
            var  livemode = jsonResponse.BoolOf("livemode");
        }
    }
}

