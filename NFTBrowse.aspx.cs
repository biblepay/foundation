using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.StringExtension;

namespace Saved
{

    public partial class NFTBrowse : Page
    {
        protected void chkDigital_Changed(object sender, EventArgs e)
        {

        }
        protected void chkTweet_Changed(object sender, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            bool fTestNet = false;

            if (Session["chkDigital"] == null)
            {
                // first time
                Session["chkDigital"] = "1";
                Session["chkSocial"] = "1";
            }

            if (IsPostBack)
            {
                Session["chkDigital"] = chkDigital.Checked ? "1" : "0";
                Session["chkSocial"] = chkSocial.Checked ? "1" : "0";
            }
            chkDigital.Checked = SessionToBool(Session,"chkDigital");
            chkSocial.Checked = SessionToBool(Session,"chkSocial");
            

            string sType = Request.QueryString["type"] ?? "";
            if (sType != "")
            {
                if (sType == "orphan" || sType == "goods")
                {
                    Session["NFTQueryType"] = sType;
                    Response.Redirect("NFTBrowse");
                }
                else
                {
                    MsgBox("Error", "No such nft type.", this);
                }
            }
            string sBuy = Request.QueryString["buy"] ?? "";
            string sBid = Request.QueryString["bid"] ?? "";
            string sID = Request.QueryString["id"] ?? "";
            if (sBid == "1" && sID.Length > 10)
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("NFT Bid Error", "Sorry, you must log in first to bid on an NFT.", this);
                }
                

                double nOffer = GetDouble(Request.QueryString["amount"] ?? "");

                DACResult d = BuyNFT1(gUser(this).UserId, sID, nOffer, true, fTestNet);
                if (d.sError != "")
                { 
                    MsgBox("NFT Bid Error", d.sError, this);
                }
                else
                {
                    MsgBox("Success", "You have bidded " + nOffer.ToString() + " BBP on this NFT.", this);
                }
            }

            if (sBuy == "1" && sID.Length > 10)
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("NFT Buy Error", "Sorry, you must log in first to buy an NFT.", this);
                }

                Code.PoolCommon.NFT myNFT = GetSpecificNFT(sID, fTestNet);

                DACResult d = BuyNFT1(gUser(this).UserId, sID, myNFT.nBuyItNowAmount, false, fTestNet);
                bool fOrphan = myNFT.Type.ToLower().Contains("orphan");

                if (d.sError != "")
                {
                    MsgBox("NFT Buy Error", d.sError, this);
                }
                else
                {
                    string sNarr = fOrphan ? "You have successfully sponsored this Orphan!" : "You are now the proud new owner of an NFT.";
                    MsgBox("Success", sNarr + "<br><br> Please see your biblepaycore home wallet NFT list to find " 
                        + d.sTXID + ".  <br><br> Please wait a few blocks for the ownership to be transferred.  <br><br>You can also view your NFT <a href='NFTList'>here.</a>", this);
                }

            }

        }
    }
}