using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.Fastly;

namespace Saved
{
    public partial class Server : Page
    {

        private string SerializeUTXO(SimpleUTXO u)
        {
            string s = "<record><utxo><txid>" + u.TXID + "</txid><trace>1</trace><ordinal>" + u.Ordinal.ToString() + "</ordinal><amount>" + u.Amount.ToString() 
                + "</amount><ticker>" + u.Ticker + "</ticker><address>" + u.Address + "</address><height>" + u.Height.ToString() + "</height><account>" + u.Account + "</account></utxo></record>";
            return s;
        }

        public static string Coalesce(string a, string b)
        {
            if (a != null && a != "")
                return a;
            return b;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            string sAction = Request.QueryString["action"].ToNonNullString();
            Log("SERVER::" + sAction);

            if (sAction == "BBP_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.BBP_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "QUERY_UTXO")
            {
                Response.Write("<eof>");
                Response.End();
            }
            else if (sAction == "QUERY_UTXOS")
            {
                Response.Write("<eof>");
                Response.End();
                return;

                string sXML = Request.Headers["Action"].ToNonNullString();
                string sAddress = ExtractXML(sXML, "<address>", "</address>").ToString();
                string sTicker = ExtractXML(sXML, "<ticker>", "</ticker>").ToString();
                int nUTXOTime = (int)GetDouble(ExtractXML(sXML, "<timestamp>", "</timestamp>").ToString());

                List<SimpleUTXO> l = QueryUTXOs(sTicker, sAddress, nUTXOTime);
                string sReply = "";
                for (int i = 0; i < l.Count; i++)
                {
                    SimpleUTXO u = l[i];
                    sReply += SerializeUTXO(u);
                }
                sReply += "<eof>";

                //Log("Query LISTOF(UTXO) " + sXML + " == REPLY == " + sReply);
                Response.Write(sReply);
                Response.End();
            }
            else if (sAction == "PUBKEYDERIVE")
            {
                string sMinerID = Request.Headers["MinerID"].ToNonNullString();
                string sHash = GetSha256HashI(sMinerID);
                KeyType k = DeriveNewKey(sHash);
                string sReply = k.PubKey + "|" + k.PrivKey + "|";
                Response.Write(sReply);
                Response.End();
            }
            else if (sAction == "QUERYADDRESSBALANCE")
            {
                string sAddress = Request.Headers["Address"].ToNonNullString();
                string sAddress2 = Request.QueryString["Address"].ToNonNullString();
                double b = QueryAddressBalance(Coalesce(sAddress, sAddress2));
                string sReply = b.ToString();
                Log("QUERYADDRBALANCE " + Coalesce(sAddress, sAddress2) + " REPLY " + sReply);
                Response.Write(sReply);
                Response.End();
            }
            else if (sAction == "QUERYROKUBALANCE")
            {
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                KeyType k = DeriveRokuKeypair(sHWID);
                double b = QueryAddressBalance(k.PubKey);
                string sReply = b.ToString();
                Response.Write(sReply);
                Response.End();
            }
            else if (sAction == "LISTROKUORPHANNFTS")
            {
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                string data = Common.ListRokuNFTS(sHWID, false);
                Response.Write(data);
                Response.End();
            }
            else if (sAction == "LISTMYROKUORPHANNFTS")
            {
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                string data = Common.ListRokuNFTS(sHWID, true);
                Response.Write(data);
                Response.End();
            }
            else if (sAction == "QUERYROKUADDRESS")
            {
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                KeyType k = DeriveRokuKeypair(sHWID);
                Response.Write(k.PubKey);
                Response.End();
            }
            else if (sAction == "QUERYROKUPRIVATEKEY")
            {
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                KeyType k = DeriveRokuKeypair(sHWID);
                Response.Write(k.PrivKey);
                Response.End();
            }
            else if (sAction == "BUYNFT")
            {
                // This lets a Roku TV viewer sponsor an orphan:
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                string sAmt = Request.QueryString["amount"].ToNonNullString();
                string sData = Request.Headers["base64data"].ToNonNullString();
                string sDecoded = Base64Decode(sData);
                string sLastOwner = ExtractXML(sDecoded, "<lastcpk>", "</lastcpk>").ToString();
                double dAmt = GetDouble(sAmt);
                if (dAmt > 0 && sHWID != "" && sLastOwner != "")
                {
                    KeyType k = DeriveRokuKeypair(sHWID);
                    DACResult r = CreateFundingTransaction(dAmt, sLastOwner, k.PrivKey, sDecoded, true);
                    string sResult = r.sTXID.ToNonNullString() + "|" + r.sResult.ToNonNullString() + "|" + r.sError.ToNonNullString();
                    Log("SPENDING : " + sResult + ", Enc=" + sData + ", Data=" + sDecoded);
                    string sDesc = ExtractXML(sDecoded, "<description>", "</description>").ToString();
                    string sLo = ExtractXML(sDecoded, "<loqualityurl>", "</loqualityurl>").ToString();
                    double nPrice = GetDouble(ExtractXML(sDecoded, "<buyitnowamount>", "</buyitnowamount>"));
                    NotifyOfRokuSale(sDesc, "rob@biblepay.org", r.sTXID, true, sLo, nPrice);
                    Response.Write(sResult);
                    Response.End();
                }
                else
                {
                    Response.Write("Invalid Funding Transaction");
                    Response.End();
                }
            }
            else if (sAction == "SERIALIZENFT")
            {
                string sNFTID = Request.Headers["nftid"].ToNonNullString();
                if (sNFTID == "")
                {
                    Response.Write("");
                    Response.End();
                }
                string sHWID = Request.Headers["hwid"].ToNonNullString();
                KeyType k = DeriveRokuKeypair(sHWID);
                string sBuyerCPK = k.PubKey;
                string sPayload = PoolCommon.SerializeNFT(sHWID, sNFTID, "BUY");
                Response.Write(sPayload);
                Response.End();
            }
            else if (sAction == "CREATEFUNDINGTRANSACTION")
            {
                string sPrivKey = Request.Headers["PRIVKEY"].ToNonNullString();
                string sToAddress = Request.Headers["TOADDRESS"].ToNonNullString();
                string sAmount = Request.Headers["AMOUNT"].ToNonNullString();
                string sNotes = Request.Headers["NOTES"].ToNonNullString();
                DACResult r = CreateFundingTransaction(GetDouble(sAmount), sToAddress, sPrivKey, sNotes, true);
                string sResult = r.sTXID.ToNonNullString() + "|" + r.sResult.ToNonNullString() + "|" + r.sError.ToNonNullString();
                Response.Write(sResult);
                Response.End();
            }
            else if (sAction == "MAIL")
            {
                string sXML1 = Request.Headers["Action"].ToNonNullString();
                string sXML = Base64Decode(sXML1);
                DirectMailLetter m = new DirectMailLetter();
                string sTo = ExtractXML(sXML, "<to>", "</to>").ToString();
                string sFrom = ExtractXML(sXML, "<from>", "</from>").ToString();
                m.To.Name = ExtractXML(sTo, "<Name>", "</Name>").ToString();
                m.To.AddressLine1 = ExtractXML(sTo, "<AddressLine1>", "</AddressLine1>").ToString();
                m.To.AddressLine2 = ExtractXML(sTo, "<AddressLine2>", "</AddressLine2>").ToString();
                m.To.City = ExtractXML(sTo, "<City>", "</City>").ToString();
                m.To.State = ExtractXML(sTo, "<State>", "</State>").ToString();
                m.To.Zip = ExtractXML(sTo, "<Zip>", "</Zip>").ToString();

                m.From.Name = ExtractXML(sFrom, "<Name>", "</Name>").ToString();
                m.From.AddressLine1 = ExtractXML(sFrom, "<AddressLine1>", "</AddressLine1>").ToString();
                m.From.AddressLine2 = ExtractXML(sFrom, "<AddressLine2>", "</AddressLine2>").ToString();
                m.From.City = ExtractXML(sFrom, "<City>", "</City>").ToString();
                m.From.State = ExtractXML(sFrom, "<State>", "</State>").ToString();
                m.From.Zip = ExtractXML(sFrom, "<Zip>", "</Zip>").ToString();

                m.Medium = "Letter";
                m.Size = "8.5x14";
                m.DryRun = GetDouble(ExtractXML(sXML, "<dryrun>", "</dryrun>").ToString()) == 1;

                // Check to see if they paid for this  (requires <txid> and <toaddress>)
                double nPaidBBP = 0;
                if (m.DryRun == false)
                {
                    for (int iSleep = 0; iSleep < 10; iSleep++)
                    {
                        nPaidBBP = BMS.VerifyServicePayment(sXML);
                        if (nPaidBBP > 0)
                            break;
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                double nAmtPaidUSD = GetUSDAmountFromBBP(nPaidBBP);
                Log("Mailing " + sXML + ", PAID=" + nPaidBBP.ToString() + ", amtusd = " + nAmtPaidUSD.ToString() + ", dryrun=" + m.DryRun.ToString());
                // ************************************ DRY RUN ? ************************************************
                if (m.DryRun == false && nAmtPaidUSD < .51)
                {
                    m.DryRun = true;
                }
                m.PostalClass = "First Class";
                m.Template = ExtractXML(sXML, "<Template>", "</Template>").ToNonNullString().ToLower();
                m.Data = "ea659d20-6031-4f23-abab-3fe39abf381f"; // Easter template
                string[] vCard = m.Template.Split(" ");
                if (vCard.Length < 1)
                {
                    Response.Write("<EOF><HTML>");
                    return;
                }
                m.VariablePayload.ImageURL = "https://foundation.biblepay.org/Uploads/DM/" + vCard[0] + ".jpg";
                m.VariablePayload.OpeningSalutation = ExtractXML(sTo, "<OpeningSalutation>", "</OpeningSalutation>").ToString();
                m.VariablePayload.Paragraph1 = ExtractXML(sXML, "<paragraph1>", "</paragraph1>").ToString();
                m.VariablePayload.Paragraph2 = ExtractXML(sXML, "<paragraph2>", "</paragraph2>").ToString();
                m.VariablePayload.ClosingSalutation = ExtractXML(sTo, "<ClosingSalutation>", "</ClosingSalutation>").ToString();
                m.VariablePayload.FirstName = m.To.Name;
                m.VariablePayload.SenderName = m.From.Name;
                m.VariablePayload.SenderCompany = "Bible Pay";
                Log("Create Greeting Card " + m.VariablePayload.ImageURL + ";" + m.Template);
                m.Description = "tx " + ExtractXML(sXML, "<txid>", "</txid>").ToString();
                string response = MailLetter(m) + "<EOF></HTML>";
                Response.Write(response);
                Response.End();
                return;
            }
            else if (sAction == "statement")
            {
                string sBusinessAddress = Request.QueryString["businessaddress"].ToNonNullString();
                string sCustomerAddress = Request.QueryString["customeraddress"].ToNonNullString();
                int nStart = (int)GetDouble(Request.QueryString["starttime"].ToNonNullString());
                int nEnd = (int)GetDouble(Request.QueryString["endtime"].ToNonNullString());
                if (nEnd < nStart)
                {
                    nStart = 0;
                    nEnd = 0;
                }
                dynamic oJson2 = PoolCommon.GetStatement(sBusinessAddress, sCustomerAddress, nStart, nEnd);
                if (oJson2 == null)
                {
                    Response.Write("<EOF>");
                    return;
                }
                dynamic oJson = oJson2.Result;
                var jmyc = oJson["Charges"];
                var jmyp = oJson["Payments"];
                string sTable = "<table width=100% style='text-align:left;'>";
                string sCharges = sTable + "<TR><TH>Date</th><th width=50%>Description<th>Amount</th></tr>";
                foreach (var jCharge in jmyc)
                {
                    string sKey = jCharge.Name;
                    string sDesc = jCharge.Value["Description"].Value;
                    var nAmt = jCharge.Value["Amount"].Value;
                    string sFromAddress = jCharge.Value["FromAddress"].Value;
                    string sName = jCharge.Value["Name"].Value;
                    var nTime = jCharge.Value["Time"].Value;
                    string sRow = "<TR><TD>" + UnixTimeStampToDateTime(nTime) + "<TD>" + sDesc + "</TD><TD>" + nAmt.ToString() + "</TR>\r\n";
                    sCharges += sRow;
                }
                sCharges += "</table>";

                string sPays = sTable + "<TR><TH>Date</th><th width=50%>Invoice Number</th><th>Amount</th></tr>";

                foreach (var jPayment in jmyp)
                {
                    string sKey = jPayment.Name;
                    string sDesc = jPayment.Value["Notes"].Value;
                    double nAmt = jPayment.Value["Amount"].Value;
                    string sInvoiceNumber = jPayment.Value["InvoiceNumber"].Value;
                    var nTime = jPayment.Value["Time"].Value;
                    var TXID = jPayment.Value["TXID"].Value;
                    string sRow = "<TR><TD>" + UnixTimeStampToDateTime(nTime) + "<td>" + sInvoiceNumber + "<td>" + nAmt.ToNonNullString() + "</td></tr>\r\n";
                    sPays += sRow;
                }

                sPays += "</table>";

                string sExtra = sTable + "<TR><TH><TH width=50%>TOTALS:</th><th>Amount</th></tr>";
                string sWords = "Prior Charges,Prior Payments,Balance Forward,Current Charges,Current Payments,Current Balance";
                string sRow1 = "<TR><TD><TD>Period Start<TD>" + UnixTimeStampToDateTime(oJson["Period Start"].Value) + "</TR>\r\n";
                sExtra += sRow1;
                string sRow2 = "<TR><TD><TD>Period End<TD>" + UnixTimeStampToDateTime(oJson["Period End"].Value) + "</TR>\r\n";
                sExtra += sRow2;

                string[] vWords = sWords.Split(",");
                for (int i = 0; i < vWords.Length; i++)
                {
                    var nAmt = oJson[vWords[i]].Value;
                    string sRow = "<TR><TD><TD>" + vWords[i] + "<TD>" + nAmt.ToString() + "</TR>\r\n";
                    sExtra += sRow;
                }
                sExtra += "</table>";

                string HTML = UICommon.GetTableBeginning("Statement 04-24-2021 CPKABC123456");
                HTML += sCharges + "<br><br>" + sPays + "<br><br>" + sExtra;
                // To byte array here
                var result = Pdf.From(HTML).Portrait().Content();
                Response.Clear();
                Response.ContentType = "application/pdf";
                string sPeriod = UnixTimeStampToDateTime(oJson["Period Start"].Value).ToString();
                string accName = "Statement - " + sPeriod + ".pdf";
                Response.AddHeader("Content-Disposition", "attachment;filename=" + accName);
                Response.BinaryWrite(result);
                Response.Flush();
                Response.End();
            }
            else if (sAction == "TIP")
            {
                string sToAddress = Request.QueryString["ToAddress"].ToNonNullString();
                bool bValid = PoolCommon.ValidateBiblepayAddress(false,sToAddress);
                double dAmt = Code.Common.GetDouble(Request.QueryString["Amount"].ToNonNullString());
                if (gUser(this).LoggedIn == false)
                {
                    MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                    return;
                }
                double nBalance = DataOps.GetUserBalance(gUser(this).UserId);

                if (dAmt > nBalance)
                {
                    MsgBox("Balance Too Low", "Sorry, unable to tip user because your balance is too low.", this);
                    return;
                }
                if (dAmt < 0 || dAmt > 1000000)
                {
                    MsgBox("Out of Range", "Sorry you must tip between .01 and 1MM BBP.", this);
                    return;
                }

                if (!bValid)
                {
                    MsgBox("Invalid address", "Sorry, the address is invalid.", this);
                    return;
                }
                string txid = Withdraw(gUser(this).UserId, sToAddress, dAmt, "Tip to " + sToAddress);

                if (txid == "")
                {
                    MsgBox("Send Failure", "Sorry, the tip failed. Please contact rob@biblepay.org", this);
                    return;
                }
                else
                {
                    MsgBox("Success!", "You have tipped " + sToAddress + " the amount " + dAmt.ToString() + " BBP.  ", this);
                    return;
                }
            }
            else if (sAction == "XMR_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.XMR_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "DASH_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.DASH_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "LTC_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.LTC_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "ZEC_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.ZEC_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "BCH_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.BCH_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;

            }
            else if (sAction == "XLM_PRICE_QUOTE")
            {
                string sXLM = Saved.Code.BMS.XLM_PRICE_QUOTE();
                Response.Write(sXLM);
                Response.End();
                return;
            }
            else if (sAction == "XRP_PRICE_QUOTE")
            {
                string s1 = Saved.Code.BMS.XRP_PRICE_QUOTE();
                Response.Write(s1);
                Response.End();
                return;
            }
            else if (sAction.Contains("GENERIC_PRICE_QUOTE"))
            {
                string[] vTicker = sAction.Split("_");
                if (vTicker.Length > 2)
                {

                    double nPQ = Saved.Code.BMS.GetPriceQuote(vTicker[3]);
                    string sRes = nPQ.ToString("0." + new string('#', 339));
                    string sResult = "<MIDPOINT>" + sRes + "</MIDPOINT><EOF>";
                    Response.Write(sResult);
                    Response.End();
                    return;
                }
            }
            else if (sAction == "DOGE_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.DOGE_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "ETH_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.ETH_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;

            }
            else if (sAction == "MOBILE_API")
            {
                MobileAPI m = new MobileAPI();

                //string sBPQ = Saved.Code.BMS.BTC_PRICE_QUOTE();
                //string sBBP = Saved.Code.BMS.BBP_PRICE_QUOTE();
                m.BTCUSD = BMS.GetPriceQuote("BTC/USD");
                double nBBPBTC = BMS.GetPriceQuote("BBP/BTC");

                m.BBPUSD = m.BTCUSD * nBBPBTC;
                m.BBPBTC = nBBPBTC.ToString("0." + new string('#', 339));

                String json = Newtonsoft.Json.JsonConvert.SerializeObject(m);


                Response.Write(json);
                Response.End();
                return;
            }
            else if (sAction == "BTC_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.BTC_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "LAST_MANDATORY_VERSION")
            {
                string LMV = Saved.Code.BMS.LAST_MANDATORY_VERSION();
                Response.Write(LMV);
                Response.End();
                return;
            }
            else if (sAction == "KAIROS_PAYMENTS")
            {
                Saved.Code.BMS.KAIROS_PAYMENTS(Response);
                return;
            }
            else if (sAction == "CAMEROON_PAYMENTS")
            {
                Saved.Code.BMS.CAMEROON_PAYMENTS(Response);
                return;
            }
            else if (sAction == "CAMEROON_CHILDREN")
            {
                Saved.Code.BMS.CAMEROON_CHILDREN(Response);
                return;
            }
            else if (sAction == "KAIROS_CHILDREN")
            {
                Saved.Code.BMS.KAIROS_CHILDREN(Response);
                return;
            }
            else if (sAction == "PoolMetrics")
            {
                string XML = BMS.GetPoolMetrics();
                Response.Write(XML);
                Response.End();
                return;
            }
            else if (sAction == "FaucetID")
            {
                string sResult = Saved.Code.BMS.FaucetID(Request);
                Response.Write(sResult);
                Response.End();
                return;
            }
            else if (sAction == "GetUTXO")
            {
                Response.Write("<EOF></HTML>\r\n");
            }
            else if (sAction == "GetUTXOData")
            {
                string sReport = DataOps.GetUTXOReport();
                Response.Write(sReport);
                Response.End();
                return;
            }
            else if (sAction == "TrackDashPay")
            {
                string sResult = Saved.Code.BMS.TrackDashPay(Request);
                Response.Write(sResult);
                Response.End();
                return;
            }
            else if (sAction == "DashPay")
            {
                string sResult = Saved.Code.BMS.DashPay(Request);
                Response.Write(sResult);
                Response.End();
                return;
            }
            else
            {
                Response.Write("<HTML>NOT FOUND</EOF>");
            }
        }

        public struct MobileAPI
        {
            public double BTCUSD;
            public double BBPUSD;
            public string BBPBTC;

        }
    }
}
