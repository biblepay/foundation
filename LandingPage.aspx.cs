using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class LandingPage : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            
            string id = Saved.Code.BMS.PurifySQL(Request.QueryString["id"].ToNonNullString(), 50);
            string action = Saved.Code.BMS.PurifySQL(Request.QueryString["action"].ToNonNullString(), 50);
            string claim = Saved.Code.BMS.PurifySQL(Request.QueryString["claim"].ToNonNullString(), 50);

            if (id != "")
            {
                string sql = "Update Leads set Landed=getdate() where id = '" + id + "'";
                gData.Exec(sql);
            }

            if (action == "unsubscribe")
            {
                string sql = "Delete from Leads where id = '" + id + "'";
                gData.Exec(sql);
                MsgBox("Unsubscribed", "We are sorry to see you go.  We respect your privacy and wish you the richest blessings of Abraham, Isaac & Jacob.  <br><br>You have been unsubscribed from our mailing list.  <br>Thank you.<br>", this);
                return;
            }


            if (claim=="1")
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("Logged Out", "Sorry, you must be logged in to claim the reward.  Please join our forum <a href=https://forum.biblepay.org>from here, then come back and claim the reward</a>.", this);
                    return;
                }
                // Claim
                string sql = "Select count(*) ct from Leads where id='" + GetId() + "' and RewardClaimed is null";
                double dCt = gData.GetScalarDouble(sql, "ct");
                if (dCt == 0)
                {
                    MsgBox("Error", "Sorry, this reward has either been claimed already, or no longer exists.", this);
                    return;

                }
                else
                {
                    // Claim it
                    AdjBalance(Common.nCampaignRewardAmount, gUser(this).UserId.ToString(), "New User Welcome Bonus");
                    sql = "Update Leads set RewardClaimed=getdate() where id='" + GetId() + "'";
                    gData.Exec(sql);

                    MsgBox("Welcome Aboard!", "Welcome aboard!  You have successfully claimed the new user welcome bonus.  "
                        +" Feel free to look around at our gospel videos.  <br><br>Also, please check out our <a href='https://forum.biblepay.org/index.php?topic=517.0'>forum thread here.</a>"
                        +" <br>NOTE:  To withdraw your free BBP coins, you must enable 2FA.  You can see your balance by going to Account | Edit.  Thank you for joining BiblePay!", this);

                    return;

                }

            }
        }


        public string GetId()
        {
            string id = Saved.Code.BMS.PurifySQL(Request.QueryString["id"].ToNonNullString(), 50);
            return id;
        }
    }
}