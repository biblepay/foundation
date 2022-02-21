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


        public static void PayVideos(string sID)
        {
            try
            {
                string sql = "select datediff(second,starttime, watching) span,size/422000 secs, datediff(second,starttime, watching)/((SIZE/422000)+.01) pct,* from tip where paid is null  and userid is not null";
                if (sID != "")
                    sql += " and userid='" + BMS.PurifySQL( sID ,100) + "'";

                DataTable dt1 = gData.GetDataTable2(sql);
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
            DataTable dt = gData.GetDataTable2(sql);
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
                    string sSpendAddress = GetBMSConfigurationKeyValue("fundingaddress_mainnet");
                    string sSpendPrivKey = GetBMSConfigurationKeyValue("fundingkey_mainnet");
                    BiblePayCommon.Common.User u = new BiblePayCommon.Common.User();
                    BiblePayCommon.Common.DACResult r1 = BiblePayDLL.Sidechain.UploadFileTypeBlob(false, sOutFile, u);
                    string sOutURL = r1.Result;
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
        DataTable dt = gData.GetDataTable2(sql);
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

                    string sSpendAddress = GetBMSConfigurationKeyValue("fundingaddress_mainnet");
                    string sSpendPrivKey = GetBMSConfigurationKeyValue("fundingkey_mainnet");
                    BiblePayCommon.Common.User u = new BiblePayCommon.Common.User();

                    BiblePayCommon.Common.DACResult r1 = BiblePayDLL.Sidechain.UploadFileTypeBlob(false, sPath, u);
                    string sURL = r1.Result;
                    
                if (sURL != "")
                {  
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
                    sql = "Update RequestVideo set Status='FAILURE' where id = '" + sID + "'";
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
