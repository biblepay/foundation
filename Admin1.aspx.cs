using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
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
        

        protected void btnMailLetter_Click(object sender, EventArgs e)
        {
            string sXML = "<txid>9a5d2e4957e33ba35447e18d76c5bf7cfc4ade54ee98ee8ef52c81c29653b593</txid><feeaddress>yLKSrCjLQFsfVgX8RjdctZ797d54atPjnV</feeaddress>";
            double nAmt = BMS.VerifyServicePayment(sXML);
            return;
        }

        protected void btnCampaign_Click(object sender, EventArgs e)
        {
            // Reserved Example : string sSig = SignChrome("bbp_privkey", "msg_sig", true);
            return;
        }
        
        protected void btnRemoveBounce_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            UICommon.SendMassDailyTweetReport();
            return;
        }

        protected void btnDSQL_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                Log("DSQL CBNA");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }  
        }

        public static void SubmitPart(string sURL)
        {
        }

        protected void btnBlockChair_Click(object sender, EventArgs e)
        {
            // The following lines are test cases for currencies I found in the chain (used for TrustWallet development... to be announced around June 2021)

            //BTC++ string sAddress10 = "bc1qnx5ef7u3pnq8p05d63j7p6wgksxre3ay6kwah3";            SimpleUTXO u10 = QueryUTO("BTC", sAddress10, .46540400);
            //DOGE++             string sAddress10 = "DNDBchzgXgHEGZo8HNCbSxmqUntzoTZsk5";             SimpleUTXO u10 = QeryUTXO("DOGE", sAddress10, 1500.84221);
            //DASH++             string sAddress10 = "XrLYjwHxfFXcxygqLFbULVzGKT8f791DyP";             SimpleUTXO u10 = QeryUTXO("DASH", sAddress10, 17.99864417);
            //LTC++ string sAddress10 = "MHwp3Wp2iAbwZNorqeUPhrp5MiNsBFXURM";            SimpleUTXO u10 = QueryTXO("LTC", sAddress10, 9.99819143);
            //ETH++             string sAddress10 = "0x95Dc21040641BfEC3a9CC641047F154bc0bf10D0";           2.9908712887372113
            //XLM   GC2TACPHEEUVLPCKK6P7WH3KCJRDBJ34TQSZYEECPAO2HGT54BMGCP6N  5.2434241
            //XRP rJ1adrpGS3xsnQMb9Cw54tWJVFPuSdZHK 69.999856
            // Integration phase

            double nPrice1 = BMS.GetPriceQuote("XRP");
            double nPrice2 = BMS.GetPriceQuote("XLM");

            string sAddress10 = "0x95Dc21040641BfEC3a9CC641047F154bc0bf10D0";
            SimpleUTXO u10 = QueryUTXO("ETH", sAddress10, 2.98961128873721, 0);
            string sAddress2 = "r38WkhGgffX15bzqUbkKbMJCum7XPtkKZU";
            QueryUTXOs("XRP", sAddress2);

            SimpleUTXO u1 = QueryUTXO("XRP", sAddress2, 69.999856, 0);
            string sAddress = "GA6AJKQRXT3TKJIASIA2DEN3GIMHADCC5UMPQY3HCRGDBOOF7YOE5IB2";
            SimpleUTXO u = QueryUTXO("XLM", sAddress, 103842.1599069,0);
            string sAddress3 = "DJiaxWByoQASvhGPjnY6rxCqJkJxVvU41c";
            SimpleUTXO u5 = QueryUTXO("DOGE", sAddress3, 777,0);
            string sAddress4 = "0xaFe8C2709541E72F245e0DA0035f52DE5bdF3ee5";
            SimpleUTXO u6 = QueryUTXO("ETH", sAddress4, 0,0);
            string sAddress5 = "1Hz96kJKF2HLPGY15JWLB5m9qGNxvt8tHJ";
            SimpleUTXO u7 = QueryUTXO("BTC", sAddress5, 0,0);
            string sAddress6 = "XjsyPuaU6hVS63AVsZVjTYMkqYDYAcE3dp";
            SimpleUTXO u8 = QueryUTXO("DASH", sAddress6, 0,0);
        }
        
        protected void btnConvert_Click(object sender, EventArgs e)
        {
            string test = Saved.Code.PoolCommon.GetChartOfSancs();
            // Upgrade san.biblepay images to uplink images
            /*
            string sql = "Select id,url from  SponsoredOrphan where url like '%san.biblepay%' ";
            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string bio = dt.Rows[i]["url"].ToString();
                string sURL = Uplink.Replicate(bio);
                sql = "Update SponsoredOrphan set url='" + sURL + "' where id = '" + dt.Rows[i]["id"].ToString() + "'";
                gData.Exec(sql);
            }
            lblStatus.Text = "Updated " + DateTime.Now.ToString();
            */

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            if (!gUser(this).Admin)
            {
                Log("Clicked save from non admin");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            StringBuilder sb = new StringBuilder();

            if (FileUpload1.HasFile)
            {
                try
                {
                    sb.AppendFormat(" Uploading file: {0}", FileUpload1.FileName);
                    FileUpload1.SaveAs("c:\\" + FileUpload1.FileName);
                    //Showing the file information
                    sb.AppendFormat("<br/> Save As: {0}", FileUpload1.PostedFile.FileName);
                    sb.AppendFormat("<br/> File type: {0}", FileUpload1.PostedFile.ContentType);
                    sb.AppendFormat("<br/> File length: {0}", FileUpload1.PostedFile.ContentLength);
                    sb.AppendFormat("<br/> File name: {0}", FileUpload1.PostedFile.FileName);
                }
                catch (Exception ex)
                {
                    sb.Append("<br/> Error <br/>");
                    sb.AppendFormat("Unable to save file <br/> {0}", ex.Message);
                }
            }
            else
            {
                lblmessage.Text = sb.ToString();
            }
        }
        protected void btnPDF_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            // Pull json from HTTP response
            string sURL = "http://192.168.0.85:12000/rest/dsqlquery/";
            string sData = BMS.ExecMVCCommand(sURL);
            dynamic oJson = JsonConvert.DeserializeObject<dynamic>(sData);
            foreach (var j in oJson)
            {
                string sKey = j.Name;
                string sValue = j.Value;
            }
        }
    }
}