using Microsoft.VisualBasic;
using MimeKit;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using static Saved.Code.PoolCommon;
using static Saved.Code.Common;
using static Saved.Code.Utils;


namespace Saved.Code
{



    public static class WebServices
    {


        public static Dictionary<string, string> dicNicknames = new Dictionary<string, string>();

        public static void MemorizeNickNames()
        {

            try
            {
                string path = GetFolderUnchained("nicknames.dat");
                string sData = System.IO.File.ReadAllText(path);
                string[] vData = sData.Split("\r\n");
                for (int i = 0; i < vData.Length; i++)
                {
                    string sEntry = vData[i];
                    string sCPK = GetEle(sEntry, "|", 1);
                    string sNN = GetEle(sEntry, "|", 2);
                    if (sCPK != "" && sNN != "")
                    {
                        dicNicknames[sCPK] = sNN;
                    }

                }
            }
            catch (Exception ex)
            {
                Log("MMNA" + ex.Message);
            }
        }

        public static void PayMonthlyOrphanSponsorships()
        {

            try
            {
                UpdateBBPPrices();

                string sql = "select * from SponsoredOrphan where userid is not null and lastpaymentdate < getdate()-30";
                DataTable dt1 = gData.GetDataTable(sql);
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    string userid = dt1.Rows[i]["userid"].ToString();
                    double dAmtGross = GetDouble(dt1.Rows[i]["MonthlyAmount"]);
                    double dMatchPct = GetDouble(dt1.Rows[i]["matchpercentage"]);
                    double dMatchAmt = dAmtGross * dMatchPct;
                    double dAmt = Math.Round(dAmtGross - dMatchAmt, 2);
                    double dBalance = DataOps.GetUserBalance(userid);
                    double dMonthly = GetBBPAmountDouble(dAmt);
                    string id = dt1.Rows[i]["id"].ToString();
                    string sUserId = dt1.Rows[i]["userid"].ToString();
                    string sChildID = dt1.Rows[i]["childid"].ToString();
                    string sLastNotified = dt1.Rows[i]["LastNotified"].ToString();
                    if (sLastNotified == "") sLastNotified = "1-1-1970";
                    TimeSpan tLastNotifyElapsed = DateTime.Now - Convert.ToDateTime(sLastNotified);
                    TimeSpan tElapsed = DateTime.Now - Convert.ToDateTime(dt1.Rows[i]["LastPaymentDate"]);

                    if (dBalance > dMonthly)
                    {
                        // Charge each user for their monthly sponsorship
                        string sql1 = "Update SponsoredOrphan set LastPaymentDate=getdate() where id='" + id.ToString() + "'";
                        gData.Exec(sql1);
                        string sNotes = "Sponsor Payment " + sChildID + " (rebate " + GetBBPAmountDouble(dMatchAmt).ToString() + " bbp)";
                        sql1 = "Insert into SponsoredOrphanPayments (id,childid,amount,added,userid,updated,notes) values (newid(),'" + sChildID + "','"
                            + dMonthly.ToString() + "',getdate(),'" + userid + "',getdate(),'" + sNotes + "')";
                        gData.Exec(sql1);
                        DataOps.AdjBalance(-1 * dMonthly, userid, sNotes);
                    }
                    else
                    {
                        try
                        {
                            // If  the 21 day grace period has already elapsed, move the child back to the pool:
                            if (tElapsed.Days > 51)
                            {
                                Log("Child ID " + sChildID + " is being removed due to non payment.");
                                sql = "Update SponsoredOrphan set LastPaymentDate=null, Userid=null where ID = '" + id + "'";
                                gData.Exec(sql);
                            }
                            else if (tElapsed.Days > 30 && tElapsed.Days < 52 && tLastNotifyElapsed.Hours > 24)
                            {
                                // If they are within the 21 day grace period
                                MailAddress r = new MailAddress("rob@saved.one", "The BiblePay Team");
                                User u = DataOps.GetUserRecord(sUserId);
                                MailAddress t = new MailAddress(u.EmailAddress, u.UserName);
                                MailAddress cc = new MailAddress("rob@biblepay.org", "Rob Andrews");
                                MailMessage m = new MailMessage(r, t);
                                m.Bcc.Add(cc);
                                m.Subject = "Monthly Orphan Sponsorship Payment is Due";
                                string sBody = "Dear " + u.UserName + ",<br><br>We are unable to deduct the monthly amount due of " + dMonthly.ToString() + " BBP for child "
                                    + sChildID + ".  Our records show that it has been " + tElapsed.Days.ToString() + " days since your last payment.  <br><br>  Will you please make a deposit into your Foundation account?  We will try again tomorrow, and then 20 more successive times.  If we cannot successfully charge your account for the sponsorship within the 21 day (grace period) we will automatically cancel your sponsorship and move the child back to the available pool.  <br><br>Thank you for understanding,<br>The BiblePay Team<br>";
                                m.IsBodyHtml = true;
                                m.Body = sBody;
                                bool fSent = SendMail(m);
                                sql = "Update SponsoredOrphan set LastNotified = getdate() where id = '" + id + "'";
                                gData.Exec(sql);
                            }
                        }
                        catch (Exception ex1)
                        {
                            Log("PayMonthlyOrphanSponsorships: " + ex1.Message);
                        }

                    }
                }
            }
            catch (Exception ex2)
            {
                Log("PayMonthlyOrphanSponsorships[2]:" + ex2.Message);
            }
        }


        public static void PayVideos(string sID)
        {
            try
            {
                string sql = "select datediff(second,starttime, watching) span,size/422000 secs, datediff(second,starttime, watching)/((SIZE/422000)+.01) pct,* from tip where paid is null  and userid is not null";
                if (sID != "")
                    sql += " and userid='" + sID + "'";

                DataTable dt1 = gData.GetDataTable(sql);
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    double nPCT = GetDouble(dt1.Rows[i]["Pct"]);
                    double nRew = GetDouble(dt1.Rows[i]["Amount"]);
                    if (nPCT > 1) nPCT = 1;

                    double nAmt = nRew * nPCT;
                    string sCategory = dt1.Rows[i]["Category"].ToString();
                    string sUserID = dt1.Rows[i]["UserID"].ToString();
                    // Reward the user
                    DataOps.AdjBalance(nAmt, sUserID, "Video Reward for " + Math.Round(nPCT * 100, 2).ToString() + "% for [" + sCategory + "]");
                    sql = "Update Tip set Paid = getdate() where id = '" + dt1.Rows[i]["id"].ToString() + "'";
                    gData.Exec(sql);
                }
            }
            catch (Exception ex)
            {
                Log("Unable to pay " + ex.Message);
            }
        }

        public static void ConvertMp4ToJpg(string sURL, string sOutFile)
        {
            try
            {
                //>ffmpeg -ss 45 -i c:\\1.mp4 -vframes 1 -filter "scale=-1:300" thumb1.jpg
                //https://media.biblepay.org/7001248974024.mp4
                string vidArgs = "-ss 45 -i " + sURL + " -vframes 1 -filter scale=-1:300 " + sOutFile;
                string res = run_cmd("c:\\inetpub\\wwwroot\\Saved\\bin\\ffmpeg.exe", vidArgs);
            }
            catch (Exception ex)
            {
                Log("CMP4TJ::" + ex.Message);
            }
        }


        public static void AddThumbnails()
        {
            string sql = "Select * from Rapture where thumbnail is null and url like '%mp4%'";
            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string url = dt.Rows[i]["url"].ToString();
                string sID = dt.Rows[i]["id"].ToString();
                string sUserID = dt.Rows[i]["userid"].ToString();
                string sOutFile = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\Thumbnails\\" + sID + ".jpg";
                ConvertMp4ToJpg(url, sOutFile);
                FileInfo fi = new FileInfo(sOutFile);
                if (fi.Exists)
                {
                    string sOutURL = "https://foundation.biblepay.org/Uploads/Thumbnails/" + sID + ".jpg";
                    sql = "Update Rapture set Thumbnail='" + sOutURL + "' where id = '" + sID + "'";
                    gData.Exec(sql);
                }
            }
        }

    public static void ConvertVideos()
    {
        string sVideoPath = GetBMSConfigurationKeyValue("convertvideospath");
        if (sVideoPath == "")
            return;

        // Convert unconverted RequestVideo to Resilient format
        string sql = "Select * from RequestVideo where status is null";
        DataTable dt = gData.GetDataTable(sql);
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            string url = dt.Rows[i]["url"].ToString();
            string sID = dt.Rows[i]["id"].ToString();
            string sUserID = dt.Rows[i]["userid"].ToString();
            // Convert this particular youtube URL into a rapture video
            // Then store in the rapture table with an uncategorized category
            GetVideo(url);
            string sPath = GetPathFromTube(url);
            // Convert the path to hash
            string sNewFileName = "700" + sPath.GetHashCode().ToString() + ".mp4";
            if (System.IO.File.Exists(sPath))
            {
                System.IO.FileInfo fi = new FileInfo(sPath);
                string sHeading = Left(fi.Name, 100);
                string sNotes = Utils.CleanseHeading(sHeading) + "\r\n\r\n" + Left(DataOps.GetNotes(sPath), 4000);

                string sTargetPath = sVideoPath + "\\" + sNewFileName;
                bool bOK = false;
                try
                {
                    System.IO.File.Copy(sPath, sTargetPath);
                    bOK = true;
                }
                catch (Exception)
                {

                }
                if (bOK)
                {
                    string sURL = "https://san.biblepay.org/Rapture2/" + sNewFileName;
                    sql = "Insert into Rapture (id,added,Notes,URL,FileName,Category,UserID) values (newid(), getdate(), @notes, @url, @filename, @category, @userid)";
                    SqlCommand command = new SqlCommand(sql);
                    command.Parameters.AddWithValue("@notes", sNotes);
                    command.Parameters.AddWithValue("@url", sURL);
                    command.Parameters.AddWithValue("@filename", sNewFileName);
                    command.Parameters.AddWithValue("@category", "Miscellaneous");
                    command.Parameters.AddWithValue("@userid", sUserID);
                    gData.ExecCmd(command);
                    sql = "Update RequestVideo set Status='FILLED' where id = '" + sID + "'";
                    gData.Exec(sql);
                    // Delete the temporary file
                    System.IO.File.Delete(sPath);
                }
                else
                {
                    sql = "Update RequestVideo set Status='AMAZON FAILURE' where id = '" + sID + "'";
                    gData.Exec(sql);
                }
            }
            else
            {
                sql = "Update RequestVideo set Status='FILE DOES NOT EXIST' where id = '" + sID + "'";
                gData.Exec(sql);
            }
        }


    }


    public static string VerifyEmailAddress(string sEmail, string sID)
    {

        if (sEmail == null || sEmail == "")
            return "UNDELIVERABLE";


        string sKey = GetBMSConfigurationKeyValue("thechecker");
        string sURL = "https://api.thechecker.co/v2/verify?email=" + sEmail + "&api_key=" + sKey;
        string sOut = BMS.GetWebJsonApi(sURL, "", "","","");
        if (sOut != "")
        {
            JObject oData = JObject.Parse(sOut);
            string sResult = oData["result"].ToString();
            bool bDisposable = Convert.ToBoolean(oData["disposable"].ToString());
            string reason = oData["reason"].ToString();
            bool bFree = Convert.ToBoolean(oData["free"].ToString());
            bool bAcceptAll = Convert.ToBoolean(oData["accept_all"].ToString());
            if (sID != "")
            {
                string sql = "Update Leads set Verification='" + sResult + "', VerificationReason = '" + reason + "', Free='" + B2N(bFree).ToString()
                    + "',Acceptall='" + B2N(bAcceptAll).ToString() + "',Disposable='" + B2N(bDisposable).ToString() + "' where id = '" + sID + "'";
                gData.Exec(sql);
            }
            return sResult;
        }
        return "";
    }



 }
}
