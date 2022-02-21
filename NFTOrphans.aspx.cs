using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.StringExtension;

namespace Saved
{

    public partial class NFTOrphans : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            string sBuy = Request.QueryString["buy"] ?? "";
            string sID = Request.QueryString["id"] ?? "";
            bool fTestNet = false;
            if (sBuy == "1" && sID.Length > 10)
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("NFT Buy Error", "Sorry, you must log in first to sponsor an NFT.", this);
                }

                Code.PoolCommon.NFT myNFT = GetSpecificNFT(sID, fTestNet);

                DACResult d = BuyNFT1(gUser(this).UserId, sID, myNFT.nBuyItNowAmount, false, fTestNet);
                if (d.sError != "")
                {
                    MsgBox("NFT Sponsorship Error", d.sError, this);
                }
                else
                {
                    MsgBox("Success", "You have sponsored " + myNFT.Name + "!  Please find this orphan record in your biblepaycore home wallet NFT List: "
                        + d.sTXID + ".   Please wait a few blocks for this sponsorship to start.   Thank you for fulfilling James 1:27 with BiblePay!", this);
                }
            }
        }


    }
}