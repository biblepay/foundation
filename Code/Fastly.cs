using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Saved.Code.Common;
using static Saved.Code.Utils;
using static Saved.Code.WebServices;

namespace Saved.Code
{
    public static class Fastly
    {
        public static void SyncFastlyNicknames()
        {
            try
            {
                string path = GetFolderUnchained("nicknames.dat");
                DateTime diMod = System.IO.File.GetLastWriteTime(path);
                TimeSpan tsElapsed = System.DateTime.Now - diMod;
                if (dicNicknames.Count() == 0)
                    MemorizeNickNames();

                if (tsElapsed.TotalSeconds < (60 * 30))
                    return;

                NBitcoin.RPC.RPCClient c = WebRPC.GetLocalRPCClient();
                object[] oParams = new object[2];
                oParams[0] = "cpk";
                oParams[1] = "9999999";

                dynamic j = c.SendCommand("datalist", oParams);
                JObject j1 = j.Result;
                JArray ja = (JArray)j.Result.ChildrenTokens;
                string data = "";

                foreach (var jcpk in j1)
                {
                    string skey = jcpk.Key;
                    string sValue = jcpk.Value.ToString();
                    string sType = GetEle(skey, "[-]", 0);
                    string sPriKey = GetEle(skey, "[-]", 1);
                    string sCPK = GetEle(sValue, "|", 0);
                    string sNN = GetEle(sValue, "|", 1);
                    if (sType == "CPK" && sCPK != "" && sNN != "")
                    {

                        string sRow = sType + "|" + sCPK + "|" + sNN + "\r\n";
                        data += sRow;

                    }
                }

                Unchained.WriteToFile(path, data);
                MemorizeNickNames();
            }
            catch (Exception ex)
            {
                Log("SyncFastlyNicknames " + ex.Message);
            }
        }

        public struct FastlyUser
        {
            public string CPK;
            public double balance;
            public double requests;
            public double bytesconsumed;
            public string nickname;
        }

        public static void SerializeFastlyUser(FastlyUser f)
        {
            string path = Common.GetFolderUnchained("CPK") + "\\" + f.CPK + "_balance.json";
            String json = Newtonsoft.Json.JsonConvert.SerializeObject(f);
            Unchained.WriteToFile(path, json);
        }


        public static void UpdFastlyBalance(string sKey, double nSize, double nCharge)
        {
            string sql = "Update fastlybalance set requests=isnull(requests,0)+1, balance=isnull(balance,0) + "
                    + nCharge.ToString() + ",bytesconsumed=isnull(bytesconsumed,0) + " + nSize.ToString() + ",updated=getdate() where cpk='" + BMS.PurifySQL(sKey, 200) + "'";
            gData.Exec(sql);

        }
        public static void ProcessFastlyInvoices()
        {
            /*
             * 1.  For every non-processed item in the request log, loop through it
             * 2. Move unspent utxos into another table
             * 3. Update debit/credit balances
             * 4.  Mark items paid
             * 5.  Delete items from request log that are old
             */

            try
            {
                string sql = "Select * from FastlyRequestLog where processed is null order by timestamp";
                DataTable dt = gData.GetDataTable(sql, false);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string sURL = dt.Rows[i]["url"].ToString();
                    double nSize = GetDouble(dt.Rows[i]["ResponseBodySize"].ToString());
                    string sAgent = dt.Rows[i]["Agent"].ToString();
                    string sKey = ExtractXML(sAgent, "<key>", "</key>").ToString();
                    string sTXID = ExtractXML(sAgent, "<tx>", "</tx>").ToString();
                    string sSig = ExtractXML(sAgent, "<sig>", "</sig>").ToString();
                    string sID = dt.Rows[i]["id"].ToString();
                    sql = "Update FastlyRequestLog set processed=getdate() where id = '" + sID + "'";
                    gData.Exec(sql);
                    if (sKey != "" && sSig != "")
                    {
                        sql = "Select count(*) ct from fastlybalance where cpk='" + BMS.PurifySQL(sKey, 200) + "'";
                        double nCt = gData.GetScalarDouble(sql, "ct");
                        if (nCt == 0)
                        {
                            sql = "Insert into FastlyBalance (id,cpk) values (newid(), '" + BMS.PurifySQL(sKey, 200) + "')";
                            gData.Exec(sql);
                        }
                        // adjust balance
                        double nCharge = nSize * .00001;
                        if (nCharge < 1)
                            nCharge = 1;

                        UpdFastlyBalance(sKey, nSize, nCharge);
                        // Update the json file
                        sql = "Select * from fastlybalance where cpk='" + BMS.PurifySQL(sKey, 200) + "'";
                        DataTable dt1 = gData.GetDataTable(sql);
                        if (dt1.Rows.Count > 0)
                        {
                            FastlyUser f = new FastlyUser();
                            f.balance = GetDouble(dt1.Rows[0]["balance"]);
                            f.bytesconsumed = GetDouble(dt1.Rows[0]["bytesconsumed"]);
                            f.CPK = sKey;
                            if (dicNicknames.ContainsKey(sKey))
                                f.nickname = dicNicknames[sKey];

                            SerializeFastlyUser(f);

                            if (f.balance > 100 && sTXID != "")
                            {
                                sql = "Select count(*) ct from fastlypayments where txhex='" + BMS.PurifySQL(sTXID, 2000) + "'";
                                double dCT = gData.GetScalarDouble(sql, "ct");
                                Log(sql);

                                if (dCT == 0)
                                {
                                    string txid = WebRPC.SendRawTx(sTXID);
                                    sql = "Insert into FastlyPayments (id,added,txid,amount,txhex) values (newid(),getdate(),'" + txid + "',80,'" + BMS.PurifySQL(sTXID, 2000) + "')";
                                    gData.Exec(sql);
                                    if (txid.Length > 20)
                                    {
                                        // Todo - call out to see what the transaction amount is worth.
                                        UpdFastlyBalance(sKey, 0, -80.05);
                                        Log("lowering " + sKey + " by 80");
                                    }
                                    else
                                    {
                                        Log("got back" + txid);
                                    }

                                }
                            }
                        }
                    }
                    if (i == 0)
                    {
                        sql = "Delete from FastlyRequestLog where timestamp < getdate()-7";
                        gData.Exec(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ProcessFastlyBalances: " + ex.Message);
            }
        }

        public static string SV(string URL)
        {
            URL += "?token=" + SignVideoURL();
            return URL;
        }

        public static string SignVideoURL()
        {
            string sKey2 = GetBMSConfigurationKeyValue("fastlysigner");
            sKey2 = sKey2.Replace("[equal]", "=");
            int nExpiry = 60 * 60 * 8;
            string sTS = (UnixTimeStamp() + nExpiry).ToString();
            string sTest3 = Base64Sha1HashWithKey(sTS, sKey2);
            string sFullKey = sTS + "_" + sTest3;
            return sFullKey;
        }
        public static string Base64Sha1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        public static string Sha1Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        public static string SignChrome(string sPrivKey, string sMessage, bool fProd)
        {
            if (sPrivKey == null || sMessage == String.Empty || sMessage == null)
                return string.Empty;

            BitcoinSecret bsSec;
            if (fProd)
            {
                bsSec = Network.BiblepayMain.CreateBitcoinSecret(sPrivKey);
            }
            else
            {
                bsSec = Network.BiblepayTest.CreateBitcoinSecret(sPrivKey);
            }
            string sSig = bsSec.PrivateKey.SignMessage(sMessage);
            string sPK = bsSec.GetAddress().ToString();
            var fSuc = VerifySignature(sPK, sMessage, sSig);
            return sSig;
        }

        public static string Sign(string sPrivKey, string sMessage, bool fProd)
        {
            if (sPrivKey == null || sMessage == String.Empty || sMessage == null)
                return string.Empty;

            BitcoinSecret bsSec;
            if (fProd)
            {
                bsSec = Network.BiblepayMain.CreateBitcoinSecret(sPrivKey);
            }
            else
            {
                bsSec = Network.BiblepayTest.CreateBitcoinSecret(sPrivKey);
            }
            string sSig = bsSec.PrivateKey.SignMessage(sMessage);
            string sPK = bsSec.GetAddress().ToString();
            var fSuc = VerifySignature(sPK, sMessage, sSig);
            return sSig;
        }

        public static bool VerifySignature(string BBPAddress, string sMessage, string sSig)
        {
            if (BBPAddress == null || sSig == String.Empty)
                return false;
            try
            {
                // Determine the network:
                BitcoinPubKeyAddress bpk;
                if (BBPAddress.StartsWith("y"))
                {
                    bpk = new BitcoinPubKeyAddress(BBPAddress, Network.BiblepayTest);
                }
                else if (BBPAddress.StartsWith("X"))
                {
                    bpk = new BitcoinPubKeyAddress(BBPAddress, Network.DashMain);
                }
                else
                {
                    bpk = new BitcoinPubKeyAddress(BBPAddress, Network.BiblepayMain);
                }

                bool b1 = bpk.VerifyMessage(sMessage, sSig);
                return b1;
            }
            catch (Exception ex)
            {
                Log("VerifySignature::" + ex.Message + " for key " + BBPAddress);
                return false;
            }
        }

        public static void ProcessFastly(string sData)
        {
            string sIndData = "";
            try
            {
                sData = sData.Replace("}\r\n{", "}<newline>{");
                sData = sData.Replace("}\r{", "}<newline>{");
                sData = sData.Replace("}\n{", "}<newline>{");
                // {   "timestamp":"2021 - 02 - 17T04: 13:21",   "time_elapsed":388856,   "is_tls":true,   "client_ip":"40.77.188.62",   "geo_city":"chicago",   "geo_country_code":"US",   "request":"GET",   "host":"(null)",   "url":" / Rapture2 / 700 - 764046584.mp4",   "request_referer":"",   "request_user_agent":"Mozilla / 5.0(Windows NT 6.1; WOW64) AppleWebKit / 534 + (KHTML, like Gecko) BingPreview / 1.0b",   "request_accept_language":"",   "request_accept_charset":"",   "cache_status":"PASS" }{ "timestamp":"2021-02-17T04:13:18",   "time_elapsed":3858590,   "is_tls":true,   "client_ip":"40.77.188.62",   "geo_city":"chicago",   "geo_country_code":"US",   "request":"GET",   "host":"(null)",   "url":"/Rapture2/700-764046584.mp4",   "request_referer":"",   "request_user_agent":"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534+ (KHTML, like Gecko) BingPreview/1.0b",   "request_accept_language":"",   "request_accept_charset":"",   "cache_status":"PASS" }            { "timestamp":"2021-02-17T04:13:22",   "time_elapsed":392418,   "is_tls":true,   "client_ip":"40.77.188.47",   "geo_city":"chicago",   "geo_country_code":"US",   "request":"GET",   "host":"(null)",   "url":"/Rapture2/7001216792637.mp4",   "request_referer":"",   "request_user_agent":"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534+ (KHTML, like Gecko) BingPreview/1.0b",   "request_accept_language":"",   "request_accept_charset":"",   "cache_status":"PASS" 
                string[] vData = sData.Split("<newline>");
                for (int i = 0; i < vData.Length; i++)
                {
                    if (vData[i].Length > 10)
                    {
                        sIndData = vData[i];
                        JObject oData = JObject.Parse(sIndData);
                        String timestamp = oData["timestamp"].ToString();
                        string IP = oData["client_ip"].ToNonNullString();
                        string sCity = oData["geo_city"].ToNonNullString();
                        string sCountry = oData["geo_country_code"].ToNonNullString();
                        string sURL = oData["url"].ToNonNullString();
                        string sAgent = oData["request_user_agent"].ToNonNullString();
                        double nRespBodySz = GetDouble(oData["response_body_size"].ToNonNullString());
                        if (IP != "" && sURL != "")
                        {
                            // check sig

                            string sKey = ExtractXML(sAgent, "<key>", "</key>").ToString();
                            string sSig = ExtractXML(sAgent, "<sig>", "</sig>").ToString();
                            double dSigValid = 0;
                            if (sKey != "" && sSig != "")
                            {
                                BitcoinPubKeyAddress bpk;
                                bpk = new BitcoinPubKeyAddress(sKey, Network.BiblepayMain);
                                dSigValid = bpk.VerifyMessage("hello1", sSig, false) ? 1 : 0;
                            }

                            string sql = "Insert into FastlyRequestLog (id, timestamp, IP, City, Country, URL, Agent, ResponseBodySize, SigValid) values (newid(), @timestamp, @IP, @city, @country, @URL, @agent, @rbs, @sigvalid)";
                            SqlCommand command = new SqlCommand(sql);
                            command.Parameters.AddWithValue("@timestamp", timestamp);
                            command.Parameters.AddWithValue("@IP", IP);
                            command.Parameters.AddWithValue("@city", sCity);
                            command.Parameters.AddWithValue("@country", sCountry);
                            command.Parameters.AddWithValue("@URL", sURL);
                            command.Parameters.AddWithValue("@agent", sAgent);
                            command.Parameters.AddWithValue("@rbs", nRespBodySz);
                            command.Parameters.AddWithValue("@sigvalid", dSigValid);

                            gData.ExecCmd(command);
                        }
                    }
                }
                SyncFastlyNicknames();
                ProcessFastlyInvoices();
            }
            catch (Exception ex)
            {
                Log("FastlyIns::" + ex.Message + ":: " + sData + ":: Forensics :: " + sIndData);
            }

        }

    }
}