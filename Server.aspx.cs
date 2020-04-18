using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;

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
            else if (sAction == "DASH_PRICE_QUOTE")
            {
                string sBPQ = Saved.Code.BMS.DASH_PRICE_QUOTE();
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