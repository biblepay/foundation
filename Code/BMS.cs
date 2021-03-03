using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using static Saved.Code.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Saved.Code.DataOps;

namespace Saved.Code
{
    public static class BMS
    {

        public static string ExtractXML(string sData, string sStartKey, string sEndKey)
        {
            if (sData == null)
                return string.Empty;

            int iPos1 = (sData.IndexOf(sStartKey, 0) + 1);
            if (iPos1 < 1)
                return string.Empty;

            iPos1 = (iPos1 + sStartKey.Length);
            int iPos2 = (sData.IndexOf(sEndKey, (iPos1 - 1)) + 1);
            if ((iPos2 == 0))
            {
                return String.Empty;
            }
            string sOut = sData.Substring((iPos1 - 1), (iPos2 - iPos1));
            return sOut;
        }

        public static int UnixTimestampHiResolution(DateTime dt)
        {
            DateTime dt2 = Convert.ToDateTime(new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second));
            Int32 unixTimestamp = (Int32)(dt2.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        public static int UnixTimeStampFromFile(string sPath)
        {
            try
            {
                FileInfo fi = new FileInfo(sPath);
                return UnixTimestampHiResolution(fi.LastWriteTimeUtc);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }



        public static string GetWebJsonApi(string url, string header, string value)
        {
            try
            {
                // Use this to automatically deflate or un-gzip a response stream
                Uri address = new Uri(url);
                System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(address);
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.Headers.Add("Content-Encoding", "utf-8");
                if (header != "")
                {
                    request.Headers.Add(header, value);
                }
                Encoding asciiEncoding = Encoding.ASCII;
                request.Method = "GET";
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (System.Net.WebResponse response = request.GetResponse())
                {
                    System.IO.StreamReader sr =
                        new System.IO.StreamReader(
                            response.GetResponseStream());
                    return sr.ReadToEnd();
                }
            }catch(Exception ex)
            {
                return "";
            }
        }
    
        
        public static void GetMoneroHashRate(out int nBlocks, out double nHashRate)
        {
            string url = "https://minexmr.com/api/pool/stats";
            string sData = GetWebJsonApi(url, "", "");
            if (sData != "")
            { //pool.stats.miners
                JObject oData = JObject.Parse(sData);
                JArray j1 = (JArray)oData["pool"]["blocks"];
                try
                {
                    nBlocks = j1.Count;
                    nHashRate = GetDouble(oData["pool"]["hashrate"]) / 1000000;
                    return;
                }catch(Exception ex)
                {
                    Log("GMHRA:" + ex.Message);
                }
            }
            nHashRate = 0;
            nBlocks = 0;
        }

        public static string ExecMVCCommand(string URL, int iTimeout = 30)
        {
            BiblePayClient wc = new BiblePayClient();
            try
            {
                wc.SetTimeout(iTimeout);
                string d = wc.FetchObject(URL).ToString();
                return d;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exec MVC Failed for " + URL);

                return "";
            }
        }

        public static void CacheQuote(string ticker, string sPrice)
        {
            string sql = "Delete from System where SystemKey = 'PRICE_" + ticker + "'";
            gData.Exec(sql);
            sql = "Insert into System (id,systemkey,value,updated) values (newid(),'PRICE_" + ticker + "','" + sPrice + "',getdate())";
            gData.Exec(sql);
        }

        public static double GetCachedQuote(string ticker, out int age)
        {
            string sql = "Select updated,Value from System where systemkey='PRICE_" + ticker + "'";
            DataTable dt = gData.GetDataTable(sql, false);
            if (dt.Rows.Count < 1)
            {
                age = 0;
                return 0;
            }
            double d1 = GetDouble(dt.Rows[0]["Value"]);
            string s1 = dt.Rows[0]["Updated"].ToString();
            TimeSpan vTime = DateTime.Now - Convert.ToDateTime(s1);
            age = (int)vTime.TotalSeconds;
            return d1;
        }

        public static double GetPriceQuote(string ticker, int nAssessmentType = 0)
        {
            int age = 0;
            double dCachedQuote = GetCachedQuote(ticker, out age);
            if (dCachedQuote > 0 && age < (60 * 60 * 1))
                return dCachedQuote;

            string sURL = "https://www.southxchange.com/api/price/" + ticker;
            string sData = "";

            try
            {
                sData = ExecMVCCommand(sURL);
            }
            catch (Exception ex)
            {
                Log("BAD PRICE ERROR" + ex.Message);

            }
            string bid = ExtractXML(sData, "Bid\":", ",").ToString();
            string ask = ExtractXML(sData, "Ask\":", ",").ToString();
            double dbid = GetDouble(bid);
            double dask = GetDouble(ask);
            double dTotal = dbid + dask;

            double dmid = dTotal / 2;

            if (nAssessmentType == 1)
                dmid = dbid;
            if (dmid > 0)
            {
                CacheQuote(ticker, dmid.ToString("0." + new string('#', 339)));
            }
            else
            {
                return dCachedQuote;
            }
            return dmid;
        }


        public static string LAST_MANDATORY_VERSION()
        {
            double nVersion = 1507;
            string sResult = "<VERSION>" + nVersion.ToString() + "</VERSION><EOF></HTML>";
            return sResult;
        }
        public static string BTC_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("BTC/USD");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string DASH_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("DASH/BTC");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string DOGE_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("DOGE/BTC");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string LTC_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("LTC/BTC");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string XMR_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("XMR/BTC");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string BBP_PRICE_QUOTE()
        {
            try
            {
                double dPrice = GetPriceQuote("BBP/BTC");
                string sPrice = dPrice.ToString("0." + new string('#', 339));
                string sResult = "<MIDPOINT>" + sPrice + "</MIDPOINT><EOF>";
                return sResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static byte[] StrToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static string GetPOOMPayments(string sCharityName)
        {
            object[] oParams = new object[3];
            oParams[0] = "poom_payments";
            oParams[1] = sCharityName;
            oParams[2] = "XML";

            NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

            dynamic oOut = n.SendCommand("exec", oParams);
            string oResult = oOut.Result["payments"].Value;

            double total = 0;
            string sOut = "Child ID,CPK,Amount_USD,Amount,Block #,TXID\r\n";
            string[] vData = oResult.Split(new string[] { "<row>" }, StringSplitOptions.None);
            for (int i = 0; i < vData.Length; i++)
            {
                string childid = ExtractXML(vData[i], "<childid>", "</childid>").ToString();
                string cpk = ExtractXML(vData[i], "<cpk>", "</cpk>").ToString();
                string blockno = ExtractXML(vData[i], "<block>", "</block>").ToString();
                string samount = ExtractXML(vData[i], "<amount>", "</amount>").ToString();
                string samount_usd = ExtractXML(vData[i], "<amount_usd>", "</amount_usd>").ToString();
                string txid = ExtractXML(vData[i], "<txid>", "</txid>").ToString();
                total += GetDouble(samount);
                double dAmt = GetDouble(samount);
                if (dAmt > 0 && childid != "")
                {
                    string sRow = childid + "," + cpk + "," + samount_usd + "," + samount + "," + blockno + "," + txid + "\r\n";
                    sOut += sRow;
                }
            }
            sOut += "TOTAL:, , ," + total.ToString() + "\r\n";
            return sOut;
        }

        public static void SendBinaryFile(HttpResponse response, string filename, byte[] bytes)
        {
            response.Clear();
            response.Buffer = true;
            response.AddHeader("content-disposition", String.Format("attachment;filename={0}", filename));
            response.ContentType = "application/csv";
            response.BinaryWrite(bytes);
            response.End();
        }
        public static void CAMEROON_PAYMENTS(HttpResponse response)
        {
            string sData = GetPOOMPayments("cameroon-one");
            byte[] bytes = StrToByteArray(sData);
            SendBinaryFile(response, "cameroonpayments.csv", bytes);
        }

        public static void KAIROS_PAYMENTS(HttpResponse response)
        {
            string sData = GetPOOMPayments("kairos");
            byte[] bytes = StrToByteArray(sData);
            SendBinaryFile(response, "kairospayments.csv", bytes);
        }
        private static string Clean(string sKey, string sData, bool fRemoveColons = true)
        {
            sData = sData.Replace(sKey, "");
            sData = sData.Replace("\r\n", "");
            if (fRemoveColons)
                sData = sData.Replace(":", "");
            sData = sData.Replace(",", "");
            sData = sData.Replace("\"", "");
            return sData.Trim();
        }

        public static string GetPOOMChildren(string sCharity)
        {
            object[] oParams = new object[1];
            oParams[0] = "all";
            NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
            dynamic oOut = n.SendCommand("listchildren", oParams);
            string d = ",";
            string sOut = "Child ID,Charity,CPK,Bio URL,Sponsor,Balance\r\n";
            string[] vData = oOut.PreReader.Split(new string[] { "Charity" }, StringSplitOptions.None);
            for (int i = 0; i < vData.Length; i++)
            {
                string[] vChild = vData[i].Split(new string[] { "," }, StringSplitOptions.None);
                if (vChild.Length > 5)
                {
                    string _Charity = Clean("Charity", vChild[0]);
                    if (_Charity.ToLower() == sCharity.ToLower())
                    {
                        string sID = Clean("Child", "Child" + vChild[1]);
                        string sCPK = Clean("CPK", vChild[2]);
                        string sBio = Clean("Biography", vChild[3], false);
                        sBio = sBio.Replace(":https", "https");

                        string sBalance = Clean("Balance", vChild[4]);
                        string sSponsor = Clean("Nickname", vChild[5]);
                        if (sID.Length > 3 && sCPK.Length > 1)
                        {
                            sOut += sID + d + sCharity + d + sCPK + d + sBio + d + sSponsor + d + sBalance + d + "\r\n";
                        }
                    }
                }

            }
            return sOut;
        }


        public static double GetWebResourceSize(string url)
        {
            System.Net.WebClient client = new System.Net.WebClient();
            double dBytes = 0;
            using (var sr = client.OpenRead(url))
            {
                dBytes = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
            }
            return dBytes;
        }
        public static string GetPoolMetrics()
        {
            // API called by poolmetrics.stream
            string sql = "Select sum(Hashrate) hr From Leaderboard";
            double dHR = gData.GetScalarDouble(sql, "hr");
            sql = "Select count(bbpaddress) ct from Leaderboard";
            double dMinerCt = gData.GetScalarDouble(sql, "ct");
            double dWC = PoolCommon.dictWorker.Count;
            string XML = "<TOTALHASHRATE>" + dHR.ToString() + "</TOTALHASHRATE><WORKERS>" + dWC.ToString() + "</WORKERS><MINERS>" + dMinerCt.ToString() + "</MINERS>";
            return XML;
        }
        public static void CAMEROON_CHILDREN(HttpResponse response)
        {
            string sData = GetPOOMChildren("cameroon-one");
            byte[] bytes = StrToByteArray(sData);
            SendBinaryFile(response, "cameroonchildren.csv", bytes);
        }
        public static void KAIROS_CHILDREN(HttpResponse response)
        {
            string sData = GetPOOMChildren("kairos");
            byte[] bytes = StrToByteArray(sData);
            SendBinaryFile(response, "kairoschildren.csv", bytes);
        }

        // Todo: Ensure the DashPay Receive Address is correct
        public static string DashPayReceiveAddress = "BMnDFXTo4mwi4QCuYwA6yotSMTvRbaD5QF";
        public struct InstantLock
        {
            public double height;
            public double confirms;
            public bool instantlock;
            public double amount;
            public string txid;
            public string dashtxid;
            public string dashaddress;
            public double dashamount;
            public double usdamount;
            public string cpk;
            public string status;
            public string updated;
        }

        public static void SendDashPayRefund(ref InstantLock ix)
        {
            InstantLock IX2 = GetInstantLock(ix.txid, ix.dashaddress, ix.cpk);
            if (!IX2.instantlock)
            {
                ix.status = "WAITING_FOR_CONFIRMATIONS_INSTANTLOCK_FAILED";
            }
            else
            {
                ix.dashtxid = IssueIX(ix.cpk, ix.amount, "BBP", ix.txid);
                if (ix.dashtxid != "" && ix.dashtxid.Length > 24)
                {
                    ix.status = "REFUNDED";
                }
                else
                {
                    ix.status = "UNSENT";
                }
            }
            string sql = "Update dashpay set UPDATED=getdate(),dashtxid='" + ix.dashtxid + "',status='"
                + ix.status + "' where bbptxid = '" + ix.txid + "'";
            gData.Exec(sql);
        }

        public static string PurifySQL(string value, double maxlength)
        {
            if (value == null)
                return "";
            if (value.IndexOf("'") > 0)
                value = "";
            if (value.IndexOf("--") > 0)
                value = "";
            if (value.IndexOf("/*") > 0)
                value = "";
            if (value.IndexOf("*/") > 0)
                value = "";
            if (value.ToLower().IndexOf("xp_") > 0)
                value = "";
            if (value.IndexOf(";") > 0)
                value = "";
            if (value.ToLower().IndexOf("drop ") > 0)
                value = "";
            if (value.Length > maxlength)
                value = "";
            return value;
        }


        public static void SendDashPay(ref InstantLock ix)
        {
            InstantLock IX2 = GetInstantLock(ix.txid, ix.dashaddress, ix.cpk);
            if (!IX2.instantlock)
            {
                ix.status = "WAITING_FOR_CONFIRMATIONS_INSTANTLOCK_FAILED";
            }
            else
            {
                ix.dashtxid = IssueIX(ix.dashaddress, ix.dashamount, "DASH", ix.txid);
                if (ix.dashtxid != "" && ix.dashtxid.Length > 24)
                {
                    ix.status = "COMPLETE";
                }
                else
                {
                    ix.status = "UNSENT";
                }
            }
            string sql = "Update dashpay set UPDATED=getdate(),dashtxid='" + ix.dashtxid + "',status='"
                + ix.status + "' where bbptxid = '" + ix.txid + "'";
            gData.Exec(sql);
        }

        public static UTXO GetTxOut(string sTicker, string sTXID, int iOrdinal)
        {
            UTXO n = new UTXO();
            try
            {
                NBitcoin.RPC.RPCClient c = GetRPCClient(sTicker);
                object[] oParams = new object[2];
                oParams[0] = sTXID;
                oParams[1] = iOrdinal;
                n.nAmount = 0;
                n.Found = false;
                dynamic oOut = c.SendCommand("gettxout", oParams);
                n.TXID = sTXID;
                n.nOrdinal = iOrdinal;

                n.Address = "";
                n.nAmount = 0;

                if (oOut.Result == null)
                {
                    // Spent
                    
                    n.Spent = true;
                    n.Found = true;
                }
                else
                {
                    n.Address = oOut.Result["scriptPubKey"]["addresses"][0];
                    n.nAmount = oOut.Result["value"];
                    n.Spent = false;
                    n.Found = true;
                }
                return n;
            }
            catch(Exception ex)
            {
                Log("Cant find the UTXO::" + sTicker + "-" + sTXID + "::" + ex.Message);
            }
            return n;
        }
        public static int GetBlockAge(string sTicker)
        {
            NBitcoin.RPC.RPCClient c = GetRPCClient(sTicker);

            string sHash = c.GetBestBlockHash().ToString();
            object[] oParams = new object[1];
            oParams[0] = sHash;
            dynamic oOut = c.SendCommand("getblock", oParams);

            int utcTime = UnixTimestampHiResolution(DateTime.UtcNow);
            int nBlockTime = oOut.Result["time"];
            double nDifficulty = GetDouble(oOut.Result["difficulty"]);


            int nElapsed = utcTime - nBlockTime;
            Log(" ticker " + sTicker + " elapsed " + nElapsed.ToString());

            if (sTicker == "BBP" && nElapsed > (60 * 60 * 2))
                return 99999999;
            if (sTicker == "DASH" && nElapsed > (60 * 60 * 1))
                return 999999999;
            if (sTicker == "BBP" && nDifficulty < .10)
                return 999999999;
            if (sTicker == "DASH" && nDifficulty < 1000000)
                return 999999999;
            return 1;
        }
        
        public static InstantLock GetDbInstantLock(string sTXID)
        {
            string sql = "Select * from dashpay where bbptxid = '" + sTXID + "'";
            DataTable dt = gData.GetDataTable(sql);
            InstantLock ix = new InstantLock();
            if (dt.Rows.Count > 0)
            {
                ix.dashaddress = dt.Rows[0]["dashaddress"].ToString();
                ix.dashamount = GetDouble(dt.Rows[0]["dashamount"].ToString());
                ix.usdamount = GetDouble(dt.Rows[0]["usdamount"].ToString());
                ix.amount = GetDouble(dt.Rows[0]["bbpamount"].ToString());
                ix.txid = sTXID;
                ix.status = dt.Rows[0]["status"].ToString();
                ix.updated = dt.Rows[0]["updated"].ToString();
                ix.dashtxid = dt.Rows[0]["dashtxid"].ToString();
                return ix;
            }
            return ix;
        }
        public static string FaucetID(HttpRequest Request)
        {
            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();
            sIP = sIP.Replace("::ffff:", "");
            string sData = Request.Headers["Action"].ToNonNullString();
            string sCPK = ExtractXML(sData, "<cpk>", "</cpk>");
            string s1 = ExtractXML(sData, "<s1>", "</s1>");
            string sIP2 = sIP.Replace(".", "-");
            string sResp = sCPK + "-" + s1 + "-" + sIP2;
            string sResp2 = "<response>" + sResp + "</response><EOF></HTML>";
            return sResp2;
        }

        public static bool CheckDashAddress(string dash)
        {

            string sInPath = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\dash2.log";
            System.IO.StreamReader fileIn = new System.IO.StreamReader(sInPath);

            string sLine = "";
            int i = 0;
            while ((sLine = fileIn.ReadLine()) != null)
            {
                if (sLine == dash)
                {
                    fileIn.Close();
                    return true;
                }
                i++;
            }
            fileIn.Close();
            return false;

        }



public static string CheckReward(HttpRequest Request)
        {
            //std::string sXML = "<non_bbp_pubkey>" + sPubKey + "</non_bbp_pubkey><non_bbp_keytype>" + RoundToString(nKeyType, 0) 
            //+ "</non_bbp_keytype><bbp_pubkey>" + myAddress + "</bbp_pubkey>";
            string sForum = "https://forum.biblepay.org/index.php?topic=517.new#new";
            string sWeb = "https://www.biblepay.org";
            string sEaster = "https://forum.biblepay.org/index.php?topic=747.new#new";
            string sNarr = "Thank you for your interest in BiblePay.  We are a blockchain that supports over 90 orphans paid for by generous donations, our sanctuaries, and 10% of our mining emissions.  ";
            sNarr += "Please consider donating to our Orphan Foundation, by clicking Send Money and checking the Orphan Donation checkbox.  ";
            sNarr += "Also, please join our Forum and let us know you are a new member here " + sForum + ".   ";
            sNarr += "Please see our website at " + sWeb + ".  Finally, if you have any questions about our Easter Egg program please post here " + sEaster + ".  Thank you for using BiblePay and God bless you!";

            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();
            sIP = sIP.Replace("::ffff:", "");
            string sData = Request.Headers["Action"].ToNonNullString();
            string nba = ExtractXML(sData, "<non_bbp_pubkey>", "</non_bbp_pubkey>");
            string bbpa = ExtractXML(sData, "<bbp_pubkey>", "</bbp_pubkey>");
            string ssig = ExtractXML(sData, "<non_bbp_signature>", "</non_bbp_signature>");

            bool fVerified = Fastly.VerifySignature(nba, "bbp", ssig);
            double nAmount = 0;
            string sOutcome = "";
            if (!fVerified)
            {
                nAmount = 0;
                sOutcome = "Sorry, the Non biblepay signature is not valid.  Please try copying the signature with https://foundation.biblepay.org/Scratchpad";
            }
            else
            {
                string sIP2 = sIP.Replace(".", "-");
                string sql = "Select count(*) ct from Campaign where ip='" + PurifySQL(sIP2, 50) 
                    + "' or nonbbpaddress='" + PurifySQL(nba, 100) + "' or bbpaddress='" + PurifySQL(bbpa, 100) + "'";
                double dCt = gData.GetScalarDouble(sql, "ct");

                if (dCt > 0)
                {
                    nAmount = 0;
                    sOutcome = "Sorry, this campaign is limited to one claim per non-biblepay user. ";
                }
                else
                {
                    bool won = CheckDashAddress(nba);
                    double d1 = 0;
                    if (won)
                    {
                        Random r = new Random();
                        int rInt = r.Next(0, 20000);

                        d1 = 5000 + rInt;
                        rInt = r.Next(0, 100);
                        if (rInt == 50)
                            d1 = 1000000;
                    }
                    //                    double d1 = gData.GetScalarDou
                    if (d1 == 0)
                    {
                        nAmount = 0;
                        sOutcome = "Sorry, this non-biblepay address is not a winner.  Please try again during our next campaign.";
                    }
                    else
                    {
                        sql = "Insert into Campaign (id, bbpaddress,ip,nonbbpaddress,claimed,amount) values (newid(), '" 
                            + PurifySQL(bbpa, 100) + "','" + sIP2 + "','" + PurifySQL(nba, 100) + "',getdate(),'" + nAmount.ToString() + "')";
                        //                        sql = "Update Campaign set bbpaddress='" + PurifySQL(bbpa, 100) + "',ip='"                             + sIP2 + "', Claimed=getdate() where nonbbpaddress='" + PurifySQL(nba, 100) + "'";
                        gData.Exec(sql);
                        nAmount = d1;
                        sOutcome = "Congratulations!  You found " + nAmount.ToString() + " BBP!  The reward has been sent to your wallet address " + bbpa + "!";
                        Log(sOutcome);
                        Withdraw("", bbpa, nAmount, "");
                    }
                }
            }
            // Return response
            string sResp2 = "<amount>" + nAmount.ToString()
                + "</amount><outcome>" + sOutcome + "</outcome><narr>" + sNarr + "</narr><EOF></HTML>";
            return sResp2;
            
        }


        public static string CheckNewUserReward(HttpRequest Request)
        {
            string sForum = "https://forum.biblepay.org/index.php?topic=517.new#new";
            string sWeb = "https://www.biblepay.org";
            string sEaster = "https://forum.biblepay.org/index.php?topic=747.new#new";
            string sNarr = "Thank you for your interest in BiblePay.  We are a blockchain that supports over 90 orphans paid for by generous donations, our sanctuaries, and 10% of our mining emissions.  ";
            sNarr += "Please consider donating to our Orphan Foundation, by clicking Send Money and checking the Orphan Donation checkbox.  ";
            sNarr += "Also, please join our Forum and let us know you are a new member here " + sForum + ".   ";
            sNarr += "Please see our website at " + sWeb + ".  Finally, if you have any questions please post here " + sEaster + ".  Thank you for using BiblePay and God bless you!";

            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();
            sIP = sIP.Replace("::ffff:", "");
            string sData = Request.Headers["Action"].ToNonNullString();
            string bbp_reward_address = ExtractXML(sData, "<bbp_pubkey>", "</bbp_pubkey>");
            string email = ExtractXML(sData, "<email>", "</email>");

            double nAmount = 0;
            string sOutcome = "";
            string sIP2 = sIP.Replace(".", "-");
            string sql = "Select count(*) ct from Campaign where ip='" + PurifySQL(sIP2, 50)
                    + "' or nonbbpaddress='" + PurifySQL(email, 100) + "' or bbpaddress='" + PurifySQL(bbp_reward_address, 100) + "'";
            double dCt = gData.GetScalarDouble(sql, "ct");
            sql = "Select count(*) ct from Leads where rewardclaimed is null and Email='" + PurifySQL(email, 256) + "' ";
            double dEmailCt = gData.GetScalarDouble(sql, "ct");
            if (dCt == 0 && dEmailCt == 0)
            {
                //maybe google
                string status = CheckEmail(email, "google", "", "");
                if (status == "deliverable")
                {
                    dEmailCt = 1;
                }
                
            }
            if (dCt > 0)
                {
                    nAmount = 0;
                    sOutcome = "Sorry, this campaign is limited to one claim per non-biblepay user. ";
                }
                else if (dEmailCt == 0)
                {
                    nAmount = 0;
                    sOutcome = "Sorry, this user is not part of the new user campaign. ";
                }
            else
            {
                bool won = true;
                    double d1 = 0;
                    if (won)
                    {
                        Random r = new Random();
                        double dMinimumReward = GetDouble(GetBMSConfigurationKeyValue("minnewuserreward"));

                        int rInt = r.Next((int)dMinimumReward, (int)dMinimumReward*2);
                        d1 = 1 + rInt;
                        // Jackpot winner: (1 in 100):
                        rInt = r.Next(0, 100);
                        if (rInt == 50)
                            d1 = 1000000;
                    }
                    if (d1 == 0)
                    {
                        nAmount = 0;
                        sOutcome = "Sorry, this non-biblepay address is not a winner.  Please try again during our next campaign.";
                    }
                    else
                    {
                        sql = "Insert into Campaign (id, bbpaddress,ip,nonbbpaddress,claimed,amount) values (newid(), '"
                            + PurifySQL(bbp_reward_address, 100) + "','" + sIP2 + "','" + PurifySQL(email, 256) + "',getdate(),'" + nAmount.ToString() + "')";
                        //                        sql = "Update Campaign set bbpaddress='" + PurifySQL(bbpa, 100) + "',ip='"                             + sIP2 + "', Claimed=getdate() where nonbbpaddress='" + PurifySQL(nba, 100) + "'";
                        gData.Exec(sql);
                        sql = "Update leads set RewardClaimed=getdate() where email='" + PurifySQL(email, 256) + "'";
                        gData.Exec(sql);
                        nAmount = d1;
                        sOutcome = "Congratulations!  You found " + nAmount.ToString() + " BBP!  The reward has been sent to your wallet address " + bbp_reward_address + "!";
                        Log(sOutcome);
                        Withdraw("", bbp_reward_address, nAmount, "");
                    }
                }
            
            // Return response
            string sResp2 = "<amount>" + nAmount.ToString()
                + "</amount><outcome>" + sOutcome + "</outcome><narr>" + sNarr + "</narr><EOF></HTML>";
            return sResp2;

        }

        public static string TrackDashPay(HttpRequest Request)
        {
            try
            {
                string sql = "Select * from dashpay where STATUS not in ('COMPLETE','FAILED','SENDING','REFUNDED')";
                DataTable dt = gData.GetDataTable(sql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string txid = dt.Rows[i]["bbptxid"].ToString();
                    InstantLock ixDB = GetDbInstantLock(txid);
                    if (ixDB.dashamount > 0)
                    {
                        SendDashPayRefund(ref ixDB);
                    }
                }

                string sData = Request.Headers["Action"].ToNonNullString();
                string sTXID = ExtractXML(sData, "<txid>", "</txid>");
                sTXID = PurifySQL(sTXID, 100);
                InstantLock ix = GetDbInstantLock(sTXID);
                if (ix.status == null)
                    ix.status = "";
                if (ix.updated == null)
                    ix.updated = "";

                if (ix.status == null || ix.status == "")
                    ix.status = "TXID not found.";
                string sResponse = "<response>" + ix.status + "</response><updated>" + ix.updated.ToString() + "</updated><dashtxid>"
                    + ix.dashtxid + "</dashtxid><EOF></HTML>\r\n";
                return sResponse;
            }
            catch (Exception ex)
            {
                Log("TrackDashPay::" + ex.Message);
                return ("<response>Error 802::Unable to track.</response><EOF></HTML>");
            }
        }
        public static bool ValidateAddress(string sAddress, string sTicker)
        {
            try
            {
                NBitcoin.RPC.RPCClient nClient = GetRPCClient(sTicker);

                object[] oParams = new object[1];
                oParams[0] = sAddress;
                dynamic oOut = nClient.SendCommand("validateaddress", oParams);
                bool fValid = oOut.Result["isvalid"];
                return fValid;
            }
            catch (Exception ex)
            {
                Log("Unable to validate address::" + ex.Message);
                return false;
            }
        }

        public static string GetDashPayHealthStatus()
        {
            // Check the height, lastblock time, dashpay height, dashpay lastblocktime.  Make a call if health or not
            bool fHealth = true;
            try
            {
                NBitcoin.RPC.RPCClient nLocal = WebRPC.GetLocalRPCClient();
                int nBlocks = nLocal.GetBlockCount();
                int nBBPAge = GetBlockAge("BBP");
                int nDashAge = GetBlockAge("DASH");
                if (nBBPAge != 1 || nDashAge != 1)
                    fHealth = false;
                Log(" bbpage " + nBBPAge.ToString() + ", dashage " + nDashAge.ToString() + ", blocks " + nBlocks.ToString());


                if (fHealth == true)
                {
                    return "<health>UP</health><EOF></HTML>\r\n";
                }
                else
                {
                    return "<health>DOWN</health><EOF></HTML>\r\n";
                }
            }
            catch (Exception ex)
            {

                Log("getdashpayhealthstatus " + ex.Message);
                return "<health>DOWN</health><EOF></HTML>\r\n";
            }
        }

        public static double GetTxVoutCount(string sTXID)
        {
            try
            {
                NBitcoin.RPC.RPCClient nLocal = WebRPC.GetLocalRPCClient();
                object[] oParams = new object[2];
                oParams[0] = sTXID;
                oParams[1] = 1;
                dynamic oOut = nLocal.SendCommand("getrawtransaction", oParams);
                InstantLock IX = new InstantLock();
                //confirmations, height, instantlock
                return oOut.Result["vout"].Count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static InstantLock GetInstantLock(string sTXID, string sDashAddress, string sCPK)
        {
            try
            {
                InstantLock IX = new InstantLock();

                // Give it up to 60 seconds before we look at it.
                int nTime = UnixTimestampHiResolution(DateTime.Now);

                for (int i = 0; i < 60; i++)
                {
                    int nElapsed = UnixTimestampHiResolution(DateTime.Now) - nTime;
                    if (nElapsed > 60)
                        break;
                    double nVoutCount = GetTxVoutCount(sTXID);
                    if (nVoutCount > 0)
                        break;
                    System.Threading.Thread.Sleep(2000);
                }

                NBitcoin.RPC.RPCClient nLocal = WebRPC.GetLocalRPCClient();
                object[] oParams = new object[2];
                oParams[0] = sTXID;
                oParams[1] = 1;
                double nTotal = 0;
                dynamic oOut = nLocal.SendCommand("getrawtransaction", oParams);
                //confirmations, height, instantlock
                for (int i = 0; i < oOut.Result["vout"].Count; i++)
                {
                    string sAddress = oOut.Result["vout"][i]["scriptPubKey"]["addresses"][0];
                    double nAmount = oOut.Result["vout"][i]["value"];
                    if (sAddress == DashPayReceiveAddress)
                    {
                        nTotal += nAmount;
                    }
                }
                IX.height = GetDouble(oOut.Result["height"]);
                IX.confirms = GetDouble(oOut.Result["confirmations"]);
                IX.txid = sTXID;
                IX.cpk = sCPK;
                IX.dashaddress = sDashAddress;
                IX.instantlock = oOut.Result["instantlock"];
                if (IX.confirms > 1)
                    IX.instantlock = true;
                IX.instantlock = true;
                IX.amount = nTotal;
                return IX;
            }
            catch (Exception ex)
            {
                Log("Unable to GetInstantLock " + ex.Message);
                InstantLock IX = new InstantLock();
                return IX;
            }
        }
        public static void UpdateDashPay(InstantLock ix)
        {
            try
            {
                string sql = "Update dashpay set UPDATED=getdate(),status='" + ix.status + "' where bbptxid='" + ix.txid + "'";
                gData.Exec(sql);
            }
            catch (Exception ex)
            {
                Log("Unable To update DashPay::" + ex.Message);
            }
        }
        public static NBitcoin.RPC.RPCClient GetDashPayRPCClient()
        {
            NBitcoin.RPC.RPCCredentialString r = new NBitcoin.RPC.RPCCredentialString();
            System.Net.NetworkCredential t = new System.Net.NetworkCredential(GetBMSConfigurationKeyValue("DashPayRPCUser"),
                GetBMSConfigurationKeyValue("DashPayRPCPassword"));
            r.UserPassword = t;
            string sHost = GetBMSConfigurationKeyValue("DashPayRPCHost");
            NBitcoin.RPC.RPCClient n = new NBitcoin.RPC.RPCClient(r, sHost, NBitcoin.Network.BiblepayMain);
            return n;
        }

        public static NBitcoin.RPC.RPCClient GetRPCClient(string sTicker)
        {
            NBitcoin.RPC.RPCClient nClient;

            if (sTicker == "BBP")
            {
                nClient = WebRPC.GetLocalRPCClient();
            }
            else
            {
                nClient = GetDashPayRPCClient();
            }
            return nClient;
        }

        public static double PersistDashPay(InstantLock ix)
        {
            string sql = "SELECT count(*) ct from dashpay where bbptxid='" + PurifySQL(ix.txid, 64) + "'";
            double nCount = gData.GetScalarDouble(sql, "ct");
            if (nCount > 0)
                return -1;
            sql = "Insert into dashpay (id, DashAddress, CPK, Added, USDAmount, BBPAmount, DashAmount, Status, BBPTXID, DASHTXID) values "
                + "(newid(), '" + ix.dashaddress + "','" + ix.cpk + "',getdate(), '" + ix.usdamount.ToString() + "','" + ix.amount.ToString() + "','"
                + ix.dashamount.ToString() + "','" + ix.status + "','" + PurifySQL(ix.txid, 64) + "','" + ix.dashtxid + "')";
            gData.Exec(sql);
            return 1;
        }

        // For promotional campaigns
        public static System.Collections.Generic.List<string> GetTopDashAddresses()
        {
            List<string> t = new List<string>();
            NBitcoin.RPC.RPCClient nDash = GetRPCClient("dash");
            int nHeight = nDash.GetBlockCount();
            nHeight = 705784;
            for (int h = nHeight-1; h > 1; h--)
            {

                object[] oParams = new object[1];
                oParams[0] = h;

                Console.WriteLine(h.ToString());
                Debug.WriteLine(h.ToString());

                dynamic o1 = nDash.SendCommand("getblockhash", oParams);
                string sHash = o1.Result.ToString();

                oParams = new object[1];
                oParams[0] = sHash;

                dynamic oBlock = nDash.SendCommand("getblock", oParams);
                // loop through each transaction
                //try

                for (int i = 1; i < oBlock.Result["tx"].Count; i++)
                {
                    dynamic oTx = oBlock.Result["tx"][i];
                    string txid = oTx.ToString();
                    object[] oTxParams = new object[2];
                    oTxParams[0] = txid;
                    oTxParams[1] = 1;
                    dynamic oTx1 = nDash.SendCommand("getrawtransaction", oTxParams);

                    for (int j = 0; j < oTx1.Result["vout"].Count; j++)
                    {
                        try
                        {
                            dynamic oTxOut = oTx1.Result["vout"][j];
                            double nAmt = oTxOut["value"];
                            string sAddress = oTxOut["scriptPubKey"]["addresses"][0].ToString();
                            DashLog(sAddress);
                        }
                        catch (Exception ex)
                        {
                            //opreturn nulldata
                        }

                    }
                }
            }
            return t;
        }

        public static string IssueIX(string sAddress, double nAmount, string sTicker, string bbptxid)
        {
            try
            {
                if (nAmount == 0)
                    return "";
                NBitcoin.RPC.RPCClient nLocal = GetRPCClient(sTicker);
                object[] oParams = new object[6];
                //sendtoaddress BMnDFXTo4mwi4QCuYwA6yotSMTvRbaD5QF 1.01 ix ix false true
                oParams[0] = sAddress;
                string sAmt = nAmount.ToString();
                if (sAmt.Substring(0, 1) == ".")
                {
                    sAmt = "0" + nAmount.ToString();
                }
                oParams[1] = sAmt;
                oParams[2] = bbptxid;
                oParams[3] = bbptxid;
                oParams[4] = false; // Dont subtract fee from amount
                oParams[5] = true; // Use IX
                dynamic oOut = nLocal.SendCommand("sendtoaddress", oParams);
                string sTXID = oOut.Result.ToString();
                return sTXID;
            }
            catch (Exception ex)
            {
                Log("Unable to issueIX :: " + ex.Message);
                return "";
            }
        }

        public static double GetBalance(string sTicker)
        {
            try
            {
                NBitcoin.RPC.RPCClient n1 = GetRPCClient(sTicker);
                Money m1 = n1.GetBalance();
                double m2 = GetDouble(m1.ToString());
                return m2;
            }
            catch (Exception ex)
            {
                Log("GetBalanceRPC::" + ex.Message);
                return 0;
            }
        }
        
        public static string DashPay(HttpRequest Request)
        {
            // Verify Dash is synced, BiblePay is synced, Dash height matches dash bx height, biblepay matches biblepay bx height,
            // BBP IX is received, IX is locked, record is new, gateway is not locked, process refunds
            // otherwise process the send.
            // Verify bank account is not empty in Dash, throw error.

            try
            {
                double dBBPPrice = GetPriceQuote("BBP/BTC", 1);
                double dDashPrice = GetPriceQuote("DASH/BTC");
                double dBitcoinUSD = GetPriceQuote("BTC/USD");
                double dDashUSD = dDashPrice * dBitcoinUSD * .98;
                double dBBPUSD = dBBPPrice * dBitcoinUSD;

                if (dBBPUSD < .0002)
                {
                    return "<error>BBP Price too low (BBP/USD < .0002 BBP/USD limit).  Feature disabled temporarily.</error></HTML><EOF>\r\n";
                }
                if (dDashUSD < 1)
                {
                    return "<error>DASH Price too low.  Feature Disabled.</error></HTML><EOF>\r\n";
                }

                bool fDiagnostics = false;
                string sError = "";
                string sData = Request.Headers["Action"].ToNonNullString();
                Log("DashPay::Data " + sData);

                string sTXID = ExtractXML(sData, "<txid>", "</txid>");
                string sDashAddress = ExtractXML(sData, "<dashaddress>", "</dashaddress>");
                string sCPK = ExtractXML(sData, "<cpk>", "</cpk>");
                double dDashAmount = GetDouble(ExtractXML(sData, "<dashamount>", "</dashamount>"));
                double dExpectedBBPAmount = (dDashAmount * dDashUSD) / dBBPUSD;
                double dUSDAmt = dDashUSD * dDashAmount;

                bool fBBPValid = ValidateAddress(sCPK, "BBP");
                bool fDashValid = ValidateAddress(sDashAddress, "DASH");
                if (!fDiagnostics)
                {
                    if (!fBBPValid)
                        return "<error>BiblePay Address [" + sCPK + "] is invalid.</error></HTML><EOF>\r\n";
                    if (!fDashValid)
                        return "<error>Dash Address is invalid.</error></HTML><EOF>\r\n";
                }

                if (fDiagnostics)
                    sTXID = "c6da32d83e3880d836f2a992b7545caa4da6d486f8f891948592282936ae1f73";

                //////////////////////////////////////// DRY RUN MODE ////////////////////////////////////////////////
                if (sTXID == "")
                {
                    // Dry run mode
                    // Verify the bank account balance in Dash
                    double nDashBalance = GetBalance("DASH");
                    if (nDashBalance < dDashAmount)
                    {
                        return "<error>Error 801: Dashpay is down.</error></HTML><EOF>\r\n";
                    }
                    double nBBPAmount = GetDouble(ExtractXML(sData, "<bbpamount>", "</bbpamount>"));
                    if (nBBPAmount < (dExpectedBBPAmount * .90) || nBBPAmount > (dExpectedBBPAmount * 1.50))
                    {
                        return "<error>The amount of BBP sent for collateral (" + nBBPAmount.ToString() + ") does not cover the amount of DASH requested.  Expected amount: " + dExpectedBBPAmount.ToString() + ".</error></HTML><EOF>\r\n";
                    }
                    if (nBBPAmount < 100)
                    {
                        return "<error>BBP amount too low. (1)</error></HTML><EOF>\r\n";
                    }
                    if (dUSDAmt < 1 || dUSDAmt > 26)
                    {
                        return "<error>Error 804: USD spend amount must be between $1.00 and $26.00.</error></HTML><EOF>\r\n";
                    }
                    return GetDashPayHealthStatus();
                }

                InstantLock ix = GetInstantLock(sTXID, sDashAddress, sCPK);
                ix.dashamount = dDashAmount;
                ix.usdamount = dUSDAmt;

                // An IX has been relayed:

                double nPersisted = PersistDashPay(ix);
                if (nPersisted == -1)
                {
                    // We've already seen this txid; maybe they are trying a double spend... Reject
                    sError = "<error>DUPLICATE_TXID</error><EOF></HTML>\r\n";
                    return sError;
                }
                if (dUSDAmt < 1 || dUSDAmt > 26)
                {
                    return "<error>Error 804: USD spend amount must be between $1.00 and $26.00.</error></HTML><EOF>\r\n";
                }

                // This is the first time we have seen this transaction.  If it is locked, we can relay it.
                if (ix.amount < (dExpectedBBPAmount * .90) || ix.amount > (dExpectedBBPAmount * 1.50))
                {
                    ix.status = "REFUND";
                    UpdateDashPay(ix);
                    return "<response>PROCESSING_REFUND</response><error>The amount of BBP sent for collateral does not cover the amount of DASH requested.</error></HTML><EOF>\r\n";
                }

                if (nPersisted == 1)
                {
                    if (ix.instantlock == true)
                    {
                        // Forward the Tx through Dash, record the Dash TXID, return the dash TXID
                        ix.status = "SENDING";
                        UpdateDashPay(ix);
                        SendDashPay(ref ix);

                        string sStatus = "<response>" + ix.dashtxid + "</response><error></error></HTML><EOF>\r\n";
                        Log("DashPay::SENT " + ix.dashtxid);

                        return sStatus;
                    }
                    else
                    {
                        // Give them a token, update status to Waiting_for_confirmations
                        ix.status = "WAITING_FOR_CONFIRMATIONS";
                        UpdateDashPay(ix);
                        string sStatus1 = "<response>" + ix.txid
                            + "</response><warning>Your transaction failed to IX lock, but is being processed.  Please type 'trackdashpay bbp_txid' to monitor your dash transaction.  "
                            + " It will be delivered after 1 more block confirmation. </warning><error></error></HTML><EOF>\r\n";

                        return sStatus1;
                    }
                }
                else
                {
                    return "<error>FAILURE::DashPay has failed.  Please forward the BBP txid to rob@biblepay.org with this message.</error><EOF></HTML>\r\n";
                }
            }
            catch (Exception ex)
            {
                Log("DashPay has failed with a catastrophe::Message" + ex.Message);
                return "<error>Error 803: IX Lock failed.  Please type 'trackdashpay bbp_txid' to track the transaction manually.</error><EOF></HTML>\r\n";
            }

        }


        public static void LaunchInterfaceWithWCG()
        {
            string sXMLPath = GetFolderWWCerts("wcgrac.xml");
            int nStored = UnixTimeStampFromFile(sXMLPath);
            if (nStored < 0)
                nStored = 0;
            int myUnix = UnixTimestampHiResolution(DateTime.UtcNow);

            int nElapsed = myUnix - nStored;
            int mySpan = (60 * 60 * 8);

            if (nElapsed > mySpan)
            {
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("Elapsed " + nElapsed.ToString() + ", UTCNOW: " + DateTime.UtcNow.ToString());

                    string sMsg = "v2.5 PORT=Elpased for WCG, storedvalue: " + nStored.ToString() + ", elapsed: " +
                        nElapsed.ToString() + ", CurTime " + myUnix.ToString() + ", mySpan " + mySpan.ToString() + ", myElapsed " + nElapsed.ToString();
                    Console.WriteLine(sMsg);
                    System.Threading.Thread.Sleep(5000);

                    BOINC b = new BOINC();
                    b.InterfaceWithWCG();
                }
            }
        }


    }


    public class BOINC
    {
        // This class interfaces with BOINCs servers to gather CPID RAC for WCG.
        // In the future, we have a couple ideas that could completely remove Oracle risk.
        // Today, we trust the RAC posted by WCG (World Community Grid), on the basis that IBM hosts the data, and we have never discovered an attack vector or fraud in WCG.
        // Our mitigation plan is if fraud is ever discovered by a CPID, we will shut down our WCG credit collector and fall back to POBH and regroup.
        // During the regroup process, we will implement one of the plans that removes oracle risk.
        // Also, if the internet goes down while BiblePay is rewarding PODC rewards, our system is resilient enough to recover by falling back to POBH.
        public BOINC()
        {
            // Initialize boinc vectors
        }

        public static void Decompress(System.IO.FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);

                        Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
                    }
                }
            }
        }

        public static void BBPCopyFile(string sOldPath, string sNewPath)
        {
            if (!File.Exists(sOldPath))
            {
                return;
            }
            if (File.Exists(sNewPath))
                File.Delete(sNewPath);
            File.Copy(sOldPath, sNewPath);
        }
       
        
        public void InterfaceWithWCG()
        {
            try
            {
                BiblePayClient wc = new BiblePayClient();
                wc.SetTimeout(10 * 60);
                string sURL = "https://download.worldcommunitygrid.org/boinc/stats/user.gz";
                string sPath = GetFolderWWCerts("wcg.gz");
                wc.DownloadFile(sURL, sPath);
                FileInfo fi = new FileInfo(sPath);
                Decompress(fi);

                // Filter file down to what we need
                string sDecPath = GetFolderWWCerts("wcg");
                System.IO.StreamReader file = new System.IO.StreamReader(sDecPath);
                string sLine = string.Empty;
                string sData = "";

                string sXMLPath = GetFolderWWCerts("wcgrac.xml");

                string sXMLPath_NEW = GetFolderWWCerts("wcgrac_new.xml");

                string sGZPath = GetFolderWWCerts("wcgrac.xml.gz");

                System.IO.StreamWriter sw = new System.IO.StreamWriter(sXMLPath_NEW);

                int iRC = 0;

                while ((sLine = file.ReadLine()) != null)
                {
                    sData += sLine + "\r\n";
                    if (sLine.Contains("</user>"))
                    {
                        //Process this user record
                        double dTeam = GetDouble(ExtractXML(sData, "<teamid>", "</teamid>"));
                        double dRac = GetDouble(ExtractXML(sData, "<expavg_credit>", "</expavg_credit>"));
                        string sCPID = ExtractXML(sData, "<cpid>", "</cpid>").ToString();
                        double nID = GetDouble(ExtractXML(sData, "<id>", "</id>"));
                        //30513 == grc, 35006 = bbp
                        if (dTeam == 35006)
                        {
                            sw.Write(sData);
                            iRC++;
                        }
                        else if (dRac > 90)
                        {
                            // if not in BBP & GRC, we just store:  CPID, ID, RAC (expavg_credit)
                            sData = "<user><id>" + nID.ToString() + "</id>" + "<expavg_credit>"
                                + Math.Round(dRac, 2).ToString() + "</expavg_credit><cpid>" + sCPID + "</cpid>";
                            // If whitelisted:
                            if (dTeam == 35006 || dTeam == 30513)
                            {
                                sData += "<teamid>" + dTeam.ToString() + "</teamid>";
                            }
                            sData += "</user>";
                            sw.Write(sData);
                            iRC++;
                        }


                        sData = "";
                    }

                }

                // Add the boinchash
                string sBoincHash = "\r\n<boinchash>" + GetSha256Hash(iRC.ToString()) + "</boinchash>";
                sw.Write(sBoincHash);
                // Pad
                for (int i = 0; i < 70; i++)
                {
                    sw.Write("<padding>" + i.ToString() + "</padding>");
                }

                file.Close(); // the source file that was decrompressed
                sw.Write("\r\n<EOF></HTML>\r\n");
                char c = (char)32;

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");

                sw.Write(new String(c, 65535));
                sw.Write("\r\n");



                sw.Close(); // the wcgrac.xml file

                // Reserved for future GZ encoding.
                /*
                byte[] inputBytes = System.IO.File.ReadAllBytes(sXMLPath);
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                        gZipStream.Write(inputBytes, 0, inputBytes.Length);

                    var outputBytes = outputStream.ToArray();
                    // Write GZ data to file
                    using (var fs1 = new FileStream(sGZPath, FileMode.Create, FileAccess.Write))
                    {
                        fs1.Write(outputBytes, 0, outputBytes.Length);
                        byte[] oBytes = Encoding.UTF8.GetBytes("\r\n<EOF></HTML>\r\n");
                        fs1.Write(oBytes);

                        fs1.Close();
                    }

                }
                */
                
                Console.WriteLine("Researcher count: " + iRC.ToString());
                if (iRC > 10000)
                {
                    // Move the new file in place
                    BBPCopyFile(sXMLPath, sXMLPath + ".bak");
                    BBPCopyFile(sXMLPath_NEW, sXMLPath);
                }
            }
            catch (Exception ex)
            {
                Log("InterfaceWithWCG::" + ex.Message);
            }
        }
    }
    

    public class BiblePayClient : System.Net.WebClient
    {
        static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }


        private int DEFAULT_TIMEOUT = 30000;

        public object FetchObject(string URL)
        {
            object o = this.DownloadString(URL);
            return o;
        }

        public void SetTimeout(int iTimeout)
        {
            DEFAULT_TIMEOUT = iTimeout * 1000;
        }
        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateCertificate);
            System.Net.WebRequest w = base.GetWebRequest(uri);
            w.Timeout = DEFAULT_TIMEOUT;
            return w;
        }
    }
}
