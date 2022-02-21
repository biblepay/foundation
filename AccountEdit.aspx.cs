﻿using Google.Authenticator;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.DataOps;

namespace Saved
{
    public partial class AccountEdit : Page
    {
        public static string _id = null;
        public static string _picture = null;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (true)
            {
                if (!gUser(this).LoggedIn)
                {
                    MsgBox("Restricted", "Sorry, you must log in first to access account edit.", this);
                    return;
                }
            }

            txtChain.Text = GetDouble(Session["ChainTestNet"]) == 1 ? "TESTNET" : "MAINNET";
            txtTwoFactorEnabled.Text = gUser(this).Require2FA == 1 ? "Enabled" : "Disabled";
            txtMyBalance.Text = GetUserBalance(this).ToString();
            string sql = "Select * from users where username=@username";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@username", gUser(this).UserName);
            DataRow dr1 = gData.GetScalarRow(command);
            imgQrCode.Visible = false;
            if (dr1 != null)
            {
                if (!this.IsPostBack)
                {
                    txtRandomXAddress.Text = dr1["RandomXBBPAddress"].ToString();
                    txtCPKAddress.Text = dr1["CPKAddress"].ToString();
                    txtCPKAddressTestnet.Text = dr1["CPKAddressTestNet"].ToString();
                    txtUserName.Text = dr1["UserName"].ToString();
                    txtEmailAddress.Text = dr1["EmailAddress"].ToString();
                    //txtForumRewardsAddress.Text = dr1["ForumRewardsAddress"].ToString();
                    chkUnsubscribeDailyDigest.Checked = GetDouble(dr1["unsubscribedailydigest"]) == 1 ? true : false;
                    chkUnsubscribe.Checked = GetDouble(dr1["Unsubscribe"]) == 1 ? true : false;
                }
                _id = dr1["id"].ToString();
                _picture = NotNull(dr1["picture"]);
            }

            if (_picture == "" || _picture == null)
            {
                _picture = "<img src=/Images/emptyavatar.png width=250 height=250 />";
            }
           
        }
        public string GetPictureLegacy()
        {
            string p = "<img src='" + _picture + "' width=256 height=256 />";
            return p;
        }
        
        protected void btnValidateTwoFactor_Click(object sender, EventArgs e)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string pin = txttwofactorcode.Text;
            if (pin == "")
            {
                MsgBox("Pin Empty", "Sorry, the pin is empty.  Unable to test the code.  Please click back and try again. ", this);

            }
            bool fPassed = tfa.ValidateTwoFactorPIN(gUser(this).UserId, pin);
            string sNarr = fPassed ? "Success.  <br>Your Two-factor authentication code has been set successfully and verified.  <br><br>Next time you log in you will be required to paste the PIN number in the 2FA box.  <br><br>Thank you.  " : "Failure!  The 2FA code does not work.  Please click back and generate a new code and try again.  ";
            string sSucNar = fPassed ? "Success" : "Fail";
            if (fPassed && gUser(this).UserName.Length > 1 && gUser(this).UserName != "Guest" && gUser(this).UserId.Length > 10)
            {
                string sql = "Update Users set twofactor=1 where id=@id";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@id", gUser(this).UserId);
                gData.ExecCmd(command);
                User g1 = (User)Session["CurrentUser"];
                g1.Require2FA = 1;
                g1.TwoFactorAuthorized = true;
                Session["CurrentUser"] = g1;
                MsgBox(sSucNar, sNarr, this);
            }
        }
        protected void btnSwitchToTestNet_Click(object sender, EventArgs e)
        {
            Session["ChainTestNet"] = "1";
            txtChain.Text = GetDouble(Session["ChainTestNet"]) == 1 ? "TESTNET" : "MAINNET";
        }

        protected void btnSwitchToMainNet_Click(object sender, EventArgs e)
        {
            Session["ChainTestNet"] = "0";
            txtChain.Text = GetDouble(Session["ChainTestNet"]) == 1 ? "TESTNET" : "MAINNET";
        }



        protected void btnSetTwoFactor_Click(object sender, EventArgs e)
        {
            // Generate the 2fa
 
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            if (gUser(this).UserName == "" || gUser(this).UserId.Length < 20)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            string title = gUser(this).UserName.Replace(" ", "_");
            var setupInfo = tfa.GenerateSetupCode("The BiblePay Foundation - " + GetBMSConfigurationKeyValue("PoolDNS"), title, gUser(this).UserId, false, 100);

            this.imgQrCode.ImageUrl = setupInfo.QrCodeSetupImageUrl;
            this.imgQrCode.Visible = true;
            lblQR.Visible = true;
            lblQR.Text = "Manual Entry: " + setupInfo.ManualEntryKey;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            string sql = "Update Users set UnsubscribeDailyDigest=@unsubscribedailydigest,cpkaddresstestnet=@cpktestnet,cpkaddress=@cpk,randomxbbpaddress=@rx,unsubscribe=@unsubscribe,updated=getdate() where id=@id";

            SqlCommand command = new SqlCommand(sql);
            /*
            if (!IsEmailValid(txtEmailAddress.Text))
            {
                MsgBox("E-Mail invalid", "Sorry, the e-mail address is invalid, please try again.", this);
                return;
            }
            */
            command.Parameters.AddWithValue("@rx", txtRandomXAddress.Text);
            command.Parameters.AddWithValue("@cpk", txtCPKAddress.Text);
            command.Parameters.AddWithValue("@cpktestnet", txtCPKAddressTestnet.Text);
            command.Parameters.AddWithValue("@id", _id);
            object unsubscribe = chkUnsubscribe.Checked ? (object)"1" : DBNull.Value;
            object unsubscribeDailyDigest = chkUnsubscribeDailyDigest.Checked ? (object)"1" : DBNull.Value;
            command.Parameters.AddWithValue("@unsubscribe", unsubscribe);
            command.Parameters.AddWithValue("@unsubscribedailydigest", unsubscribeDailyDigest);
            gData.ExecCmd(command);
            User gu =(User)Session["CurrentUser"];
            gu.RandomXBBPAddress = txtRandomXAddress.Text;
            gu.CPKAddress = txtCPKAddress.Text;
            gu.CPKAddressTestNet = txtCPKAddressTestnet.Text;
            Session["CurrentUser"] = gu;
            MsgBox("Account Updated", "Your Account has been updated.", this);
        }
        
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            /*
            string sql = "update Users set passwordhash=@pw where id=@id";
            if (!IsPasswordStrong(txtPassword.Text))
            {
                MsgBox("Minimum Password Requirements Failed", 
                    "Sorry, your password must meet these minimum guidelines: " + Common.GetPWNarr() + ".", this);
                return;
            }
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@pw", GetSha256Hash(txtPassword.Text));
            command.Parameters.AddWithValue("@id", _id);
            gData.ExecCmd(command);
            MsgBox("Password Changed", "Your Password has been changed.", this);
            */
        }

    }
}