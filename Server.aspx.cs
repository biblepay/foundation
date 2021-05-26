using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Web.UI;
using static Saved.Code.Common;

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

        protected void Page_Load(object sender, EventArgs e)
        {
            
            string sAction = Request.QueryString["action"].ToNonNullString();
            if (sAction == "BBP_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.BBP_PRICE_QUOTE();
                Response.Write(sBPQ);
                Response.End();
                return;
            }
            else if (sAction == "TEST1")
            {

                string t = GetChartOfIndex();
                for (int i = -180; i < 0; i++)
                {
                    StoreQuotes(i);
                }

            }
            else if (sAction == "QUERY_UTXO")
            {
                string sXML= Request.Headers["Action"].ToNonNullString();
                string sAddress = ExtractXML(sXML, "<address>", "</address>").ToString();
                double nAmt = GetDouble(ExtractXML(sXML, "<amount>", "</amount>").ToString());
                string sTicker = ExtractXML(sXML, "<ticker>", "</ticker>").ToString();
                int nUTXOTime = (int)GetDouble(ExtractXML(sXML, "<utxotime>", "</utxotime>").ToString());

                SimpleUTXO u = QueryUTXO(sTicker, sAddress, nAmt, nUTXOTime);
                string sReply = SerializeUTXO(u);
                sReply += "<eof>";
                Log("Query utxo " + sXML + " == REPLY == " + sReply);
                Response.Write(sReply);
                Response.End();
            }
            else if (sAction == "QUERY_UTXOS")
            {
                string sXML = Request.Headers["Action"].ToNonNullString();
                string sAddress = ExtractXML(sXML, "<address>", "</address>").ToString();
                string sTicker = ExtractXML(sXML, "<ticker>", "</ticker>").ToString();
                int nUTXOTime = (int)GetDouble(ExtractXML(sXML, "<utxotime>", "</utxotime>").ToString());
                
                List<SimpleUTXO> l = QueryUTXOs(sTicker, sAddress);
                string sReply = "";
                for (int i = 0; i < l.Count; i++)
                {
                    SimpleUTXO u = l[i];
                    sReply += SerializeUTXO(u);
                }
                sReply += "<eof>";

                Log("Query LISTOF(UTXO) " + sXML + " == REPLY == " + sReply);
                Response.Write(sReply);
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

                if (m.DryRun == true && nAmtPaidUSD < .90)
                {
                    m.DryRun = false;
                }
                // HARVEST MISSION CRITICAL TO DO  -  Change to Actual

                m.DryRun = true;
                // END OF HARVEST

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
                bool bValid = PoolCommon.ValidateBiblepayAddress(sToAddress);
                double dAmt = Code.Common.GetDouble(Request.QueryString["Amount"].ToNonNullString());
                if (gUser(this).LoggedIn == false)
                {
                    MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                    return;
                }
                double  nBalance = DataOps.GetUserBalance(gUser(this).UserId);

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
                    string sRes= nPQ.ToString("0." + new string('#', 339));
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
                /*
                string sTXID = Request.QueryString["hash"].ToNonNullString();
                string[] vHash = sTXID.Split("-");
                if (vHash.Length > 1)
                {
                    double nOrdinal = GetDouble(vHash[1]);
                    string sHash = vHash[0];
                    string sResult = DataOps.GetSingleUTXO("DASH", sHash, (int)nOrdinal);
                    Response.Write(sResult);
                    Response.End();
                    return;
                }
                */
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
    }
}