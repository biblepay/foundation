using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Server : Page
    {
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
            else if (sAction == "DOGE_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.DOGE_PRICE_QUOTE();
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