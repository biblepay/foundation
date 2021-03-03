using Google.Authenticator;
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
using static Saved.Code.WebRPC;

namespace Saved.Code
{

    

    public static class Common
    {
        public static Data gData = new Data(Data.SecurityType.REQ_SA);

        public static Pool _pool = null;
        public static XMRPool _xmrpool = null;
        public static double nCampaignRewardAmount = 10000;

        private static string sCachedHomePath = string.Empty;




        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static string GetLongSiteName(Page p)
        {
            return GetBMSConfigurationKeyValue("longsitename");
        }

        
        public static void CoerceUser(HttpSessionState Session)
        {
             User u = new User();
             u.UserName = GetBMSConfigurationKeyValue("administratorusername");
             u.LoggedIn = true;
             u.TwoFactorAuthorized = true;
             u.Require2FA = 1;
             DataOps.PersistUser(ref u);
             Session["CurrentUser"] = u;
             StoreCookie("CurrentUser", u.UserName);
        }

        public static bool runCmd(string path, string command)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = @"" + command;
            cmdsi.WorkingDirectory = path;
            Process cmd = Process.Start(cmdsi);
            int nTimeout = 9000;
            bool fSuccess =             cmd.WaitForExit(nTimeout); //wait indefinitely for the associated process to exit.
            return fSuccess;
        }
        public static string run_cmd(string sFileName,string args)
        {
            string result = " ";
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = sFileName;
                start.Arguments = string.Format("{0}", args);
                start.UseShellExecute = false;
                start.CreateNoWindow = false;
                start.RedirectStandardOutput = true;
                start.WorkingDirectory = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\";

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }

                Log("run_cmd result " + result);
                return result;
            }
            catch(Exception ex)
            {
                Log("Run cmd failed " + ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public static int B2N(bool bIn)
        {
            return bIn ? 1 : 0;
        }

        public static string FleeceCommas(string data)
        {
            string data1 = "";
            bool insidestr = false;
            for (int i = 0; i < data.Length; i++)
            {
                string ch = data.Substring(i, 1);
                if (ch == "\"")
                {
                    insidestr = !insidestr;
                }

                if (insidestr && ch == ",")
                    ch = "`";

                if (ch != "" && ch != "\"")
                {
                    data1 += ch;
                }
            }
            return data1;
        }

        public static string GetRewardMoniker()
        {
            double dreward = GetDouble(GetBMSConfigurationKeyValue("minnewuserreward"));
            double dmax = 1000000;
            string s = "between " + String.Format("{0:n0}", dreward) + " and " + String.Format("{0:n0}", dmax) + " BBP";
            return s;
        }
        public static string CheckEmail(string sEmail, string sCompany, string sTitle, string sName)
        {
            string sql = "Select count(*) ct from Leads where email='" + sEmail + "'";
            double dCt = gData.GetScalarDouble(sql, "ct");
            if (dCt == 0)
            {
                string sID = System.Guid.NewGuid().ToString();

                sql = "Insert into Leads (id, company, email, title, name, added) values ('" + sID + "', '" + sCompany + "','" + sEmail + "','" + sTitle + "','" + sName + "',getdate())";
                gData.Exec(sql);
                string status = WebServices.VerifyEmailAddress(sEmail, sID);
                return status;
            }

            return "";
        }
        
        public static string CommitToObjectStorage7(string sPath)
        {
            // Commit to STORJ network, and to VultrObjects for redundancy (and speed)
            // Storj uses a satellite uplink (defined here): https://documentation.tardigrade.io/api-reference/uplink-cli
            string s3cmd = "\\s3cmd-master\\s3cmd";
            string sFileName = System.IO.Path.GetFileName(sPath);
            string s3args = "put -P " + sPath + " s3://san1/" + sFileName + " --config=c:\\inetpub\\wwwroot\\vultrobjectsconfig.conf --no-check-certificate";
            string res = run_cmd(s3cmd, s3args);
            bool fSuc = res.Contains("ewr1.vultrobjects.com");
            string result = fSuc ? "//san1/" + sFileName : "";
            return result;
        }
        public static string Base64Sha1HashWithKey(string input, string sKey)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] messageBytes = encoding.GetBytes(input);
            byte[] key = Convert.FromBase64String(sKey);
            using (var hmacsha1 = new HMACSHA1(key))
            {
                var hash = hmacsha1.ComputeHash(messageBytes);
                string interim = "0x" + string.Concat(hash.Select(b => b.ToString("x2")));
                string base64 = Base64Encode(interim);

                return base64;
            }
        }

        public static string Withdraw(string sUserId, string toAddress, double nReq, string sNotes)
        {
            bool fGood = ValidateBiblepayAddress(toAddress);
            if (!fGood)
                return "";

            List<Payment> p = new List<Payment>();
            Payment p1 = new Payment();
            p1.bbpaddress = toAddress;
            p1.amount = nReq;
            p.Add(p1);

            string poolAccount = GetBMSConfigurationKeyValue("PoolPayAccount");
            string txid = SendMany(p, poolAccount, sNotes);
            if (txid.Length > 20 && sUserId != "")
                DataOps.AdjBalance2(-1 * nReq, sUserId, sNotes, txid);

            return txid;
        }


        public static string ReplaceURLs(string s)
        {
            s = s.Replace("\r\n", " <br><br>");

            string[] vWords = s.Split(" ");
            string sOut = "";
            for (int i = 0; i < vWords.Length; i++)
            {
                string v = vWords[i];
                if (v.Contains("https://"))
                {
                    v = v.Replace("<br>", "");

                    v = "<a target='_blank' href='" + v + "'><b>Link</b></a>";
                }
                sOut += v + " ";
            }
            return sOut;
        }
        public static string GetGlobalAlert(Page p)
        {
            string sql = "Select top 1 * from Tweet where added > getdate()-.5";
            DataTable dt1 = gData.GetDataTable(sql);
            if (dt1.Rows.Count > 0)
            {
                string sID = dt1.Rows[0]["id"].ToString();
                if (p.Session["Tweet" + sID] == null)
                {
                    string sAlertNarr = dt1.Rows[0]["Subject"].ToString();
                    if (sAlertNarr == "")
                        return "";

                    string sRedir = "TweetView?id=" + dt1.Rows[0]["id"].ToString();
                    string sAlert = "<div id=\"divAlert\" style=\"text-align:left;padding-left:250px;background-color:yellow;color:black;\">"
                    + "<span><a href=" + sRedir + ">" + sAlertNarr + "</a></span></div>";
                    return sAlert;
                }
            }
            return "";
        }


        public static double BBP_BTC = 0;
        public static double BTC_USD = 0;
        public static double BBP_USD = 0;

        public static void UpdateBBPPrices()
        {
            BBP_BTC = BMS.GetPriceQuote("BBP/BTC", 1);
            BTC_USD = BMS.GetPriceQuote("BTC/USD");
            BBP_USD = BBP_BTC * BTC_USD;
        }

        public static double GetBBPAmountDouble(double nUSD)
        {
            if (BBP_USD == 0)
                UpdateBBPPrices();

            if (BBP_USD < .000001)
                return 0;
            return Math.Round(nUSD / BBP_USD, 2);
        }

        public static string GetBBPAmount(double nUSD)
        {
            if (BBP_USD < .000001)
                return "";
            string sOut = Math.Round(nUSD / BBP_USD, 2) + " BBP";
            return sOut;
        }
        
        public static string GetBaseHomeFolder()
        {
            string sHomePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
             Environment.OSVersion.Platform == PlatformID.MacOSX) ? Environment.GetEnvironmentVariable("HOME")
                 : Environment.ExpandEnvironmentVariables("%APPDATA%");
            sHomePath = "c:\\inetpub\\wwwroot\\";

            return sHomePath;
        }
        
        public static bool ToBool(object c)
        {
            if (c == null || c==DBNull.Value) return false;
            return Convert.ToBoolean(c);
        }

        public static bool Login(string UserName, string pin, HttpSessionState h, string extra)
        {
            string sql = "Select * from Users where username=@username";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@username", UserName);
            DataRow d1 = gData.GetScalarRow(command);
            if (d1 != null)
            {
                User u = new User();
                u.UserId = d1["id"].ToString();
                u.Admin = ToBool(d1["admin"]);
                u.UserName = d1["UserName"].ToString() ?? "";
                u.AvatarURL = d1["Picture"].ToString() ?? "";
                u.Require2FA = GetDouble(d1["twofactor"].ToString());
                u.RandomXBBPAddress = d1["RandomXBBPAddress"].ToNonNullString();
                u.EmailAddress = d1["EmailAddress"].ToNonNullString();
                u.Banned = ToBool(d1["Banned"]);
                if (u.Banned)
                {
                    u.LoggedIn = false;
                    h["CurrentUser"] = u;
                    return false;
                }

                StoreCookie("CurrentUser", UserName);

                if (u.Require2FA != 1 || pin == "")
                {
                    u.LoggedIn = true;
                    u.TwoFactorAuthorized = false;
                    h["CurrentUser"] = u;
                    return true;
                }
                else 
                {
                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    bool fPassed = tfa.ValidateTwoFactorPIN(u.UserId, pin);
                    if (fPassed)
                    {
                        u.TwoFactorAuthorized = true;
                        u.LoggedIn = true;
                        h["CurrentUser"] = u;
                        return true;
                    }
                    return false;
                }
            }
            else
            {

                h["CurrentUser"] = null;
                return false;
            }
        }

        public static string DoFormat(double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);
            return s;
        }

        public static void MsgBox(string sTitle, string sBody, System.Web.UI.Page p)
        {
            p.Session["MSGBOX_TITLE"] = sTitle;
            p.Session["MSGBOX_BODY"] = sBody;
            p.Response.Redirect("MessagePage.aspx");
        }

        public static double GetOrphanFracSancPercentage()
        {
            double nBBPEPD = 0;
            double nOCPD = 0;
            double nESTROI = GetEstimatedHODL(false, 0, out nBBPEPD, out nOCPD);
            double nBBPPCT = nOCPD / (nBBPEPD + .01);
            return nBBPPCT;
        }

        public static double GetEstimatedHODL(bool fWithCompounding, double nBP, out double nBBPEarningsPerDay, out double nOrphanChargesPerDay)
        {
            string sql = "select sum(amount)/7/4500001*365 amt from sanctuaryPayment where added > getdate()-7";
            sql = "select sum(amount)/7 amt from sanctuaryPayment where added > getdate()-7";
            nBBPEarningsPerDay = Math.Round(gData.GetScalarDouble(sql, "amt"), 2);
            double nMonthlyOrphanCharges = GetDouble(GetBMSConfigurationKeyValue("CameroonOneMonthlyCharge"));
            nOrphanChargesPerDay = Math.Round(GetBBPAmountDouble(nMonthlyOrphanCharges / 30.01), 2);
            double nDailyEarnings = nBBPEarningsPerDay - nOrphanChargesPerDay;
            double nROI = Math.Round(nDailyEarnings / 4500001 * 365, 2);
            if (fWithCompounding)
            {
                nROI = GetCompounded(nROI);
            }
            return nROI;
        }

        public static string NotNull(object o)
        {

            if (o == null || o == DBNull.Value) return "";
            return o.ToString();
        }
        public static string GetSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool IsPasswordStrong(string pw)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");
            var isValidated = hasNumber.IsMatch(pw) && hasUpperChar.IsMatch(pw) && hasMinimum8Chars.IsMatch(pw);
            return isValidated;
        }

        public static bool SendMail(MailMessage message)
        {
            try
            {
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("1", "2"); // Do not change these values, change the config values.
                client.Port = 587;        
                client.EnableSsl = true;  
                client.Host = GetBMSConfigurationKeyValue("smtphost");
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(GetBMSConfigurationKeyValue("smtpuser"), GetBMSConfigurationKeyValue("smtppassword"));
                try
                {
                    client.Send(message);
                    return true;
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(30000);
                    // Time out delay
                    Console.WriteLine("Error in Send email: {0}", e.Message);
                    return false;
                }
                message.Dispose();
            }catch(Exception ex2)
            {
                Log("Cannot send Mail: " + ex2.Message);
            }
            return false;
        }

        public static         int iSent = 0;
        public static bool SendMailSSL(string Body, MailboxAddress maTo, string sSubject)
        {
            try
            {
                MailMessage m1 = new MailMessage();
                m1.IsBodyHtml = true;
                m1.Body = Body;
                m1.From = new MailAddress("rob@saved.one", "BiblePay Team");
                m1.To.Add(new MailAddress(maTo.Address, maTo.Name));
                if (iSent % 1000 == 0)
                {
                    m1.Bcc.Add(new MailAddress("rob@biblepay.org", "Rob Andrews"));
                }
                iSent++;

                m1.Subject = sSubject;
                SendMail(m1);
                return true;
            }
            catch (Exception ex)
            {
                Log("SendMailSSL2::" + ex.Message);
                return false;
            }
        }

        public static string GetPWNarr()
        {
            string sNarr = "must contain: >= 8 characters, have >= 1 uppercase character, have >= 1 number.";
            return sNarr;
        }

        public static bool IsEmailValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static SavedObject RowToObject(DataRow dr)
        {
            SavedObject s = new SavedObject();

            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                s.AddProperty(dr.Table.Columns[i].ColumnName, dr[i]);
            }
            return s;
        }

        public static int UnixTimeStamp(DateTime dt)
        {
            int unixTime = (int)((DateTimeOffset)dt).ToUnixTimeSeconds();
            return unixTime;
        }
        public static int UnixTimeStamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        public static string GetPlatformMoniker()
        {
            string sMoniker = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) ? "LIN" : "WIN";
            return sMoniker;
        }
        public static string GetPathDelimiter()
        {
            string sPathDelimiter = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) ? "/" : "\\";
            return sPathDelimiter;
        }

        public static double GetDouble(object o)
        {
            try
            {
                if (o == null) return 0;
                if (o.ToString() == string.Empty) return 0;
                double d = Convert.ToDouble(o.ToString());
                return d;
            }
            catch (Exception ex)
            {
                // Someone probably entered letters here
                return 0;
            }
        }
        public static string GetFolderUnchained(string sType)
        {
            string sPath = "c:\\inetpub\\wwwroot\\Saved\\Unchained\\" + sType;
            return sPath;
        }
        
        public static string GetFolderUploads(string sType)
        {
            string sPath = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\" + sType;
            return sPath;
        }

        public static string GetFolderWWCerts(string sType)
        {
            string sPath = "c:\\inetpub\\wwwroot\\Saved\\wwwroot\\certs\\" + sType;
            return sPath;
        }

        public static string GetFolder(int iPort, string sType)
        {
            string sHomePath = GetHomeFolder();
            string sPathDelimiter = GetPathDelimiter();
            sHomePath +=  "SAN" + sPathDelimiter;
            string s1 = sHomePath + iPort.ToString();
            if (sType != string.Empty)
                s1 += sPathDelimiter + sType;

            string sSqlPath = Path.Combine(s1);
            if (!Directory.Exists(sSqlPath))
                Directory.CreateDirectory(sSqlPath);
            return sSqlPath;
        }
        public static string GetFolder(int iPort, string sType, string sFileName)
        {
            string sPathDelimiter = GetPathDelimiter();
            string sPath = GetFolder(iPort, sType) + sPathDelimiter + sFileName;
            return sPath;
        }

        private static int iRowModulus = 0;
        private static object cs_log = new object();
        private static string mLastLogData = "";
        public static void Log(string sData, bool fQuiet=false)
        {
            lock (cs_log)
            {
                {
                    try
                    {
                        if (sData == mLastLogData)
                            return;
                        iRowModulus++;
                        if ((fQuiet && iRowModulus % 10 == 0) || (!fQuiet))
                        {
                            mLastLogData = sData;
                            string sPath = GetFolderUploads("foundation.log");
                            System.IO.StreamWriter sw = new System.IO.StreamWriter(sPath, true);
                            string Timestamp = DateTime.Now.ToString();
                            sw.WriteLine(Timestamp + ": " + sData);
                            sw.Close();
                        }
                    }

                    catch (Exception ex)
                    {
                        string sMsg = ex.Message;
                    }
                }
            }

        }

        public static void DashLog(string sData)
        {
                            string sPath = GetFolderUploads("dash.log");
                            System.IO.StreamWriter sw = new System.IO.StreamWriter(sPath, true);
                            sw.WriteLine(sData);
                            sw.Close();
        }

        public static string GetHomeFolder(bool fUseCache = true)
        {
            if (sCachedHomePath != String.Empty && fUseCache)
                return sCachedHomePath;
            string sHomePath = GetBaseHomeFolder();
            string sOverriddenPath = GetExtConfigurationKeyValue(GetBaseHomeFolder() +  "bms.conf", "datadir");
            if (!sOverriddenPath.IsNullOrEmpty())
            {
                sCachedHomePath = sOverriddenPath;
                Log("Overridden Path: " + sOverriddenPath);
                return sOverriddenPath;
            }
            sCachedHomePath = sHomePath;
            return sHomePath;
        }

        public static string GetExtConfigurationKeyValue(string sPath, string _Key)
        {
            if (!File.Exists(sPath))
                return string.Empty;

            string sData = System.IO.File.ReadAllText(sPath);
            string[] vData = sData.Split("\n");
            for (int i = 0; i < vData.Length; i++)
            {
                string sEntry = vData[i];
                sEntry = sEntry.Replace("\r", "");
                string[] vRow = sEntry.Split("=");
                if (vRow.Length >= 2)
                {
                    string sKey = vRow[0];
                    string sValue = vRow[1];
                    if (sKey.ToUpper() == _Key.ToUpper())
                        return sValue;
                }
            }
            return string.Empty;
        }

        public static string GetBMSConfigurationKeyValue(string _Key)
        {
            string sKV = GetExtConfigurationKeyValue(GetBaseHomeFolder() + "bms.conf", _Key);
            return sKV;
        }

        public static object ExtractXML(string sData, string sStartKey, string sEndKey)
        {
            int iPos1 = Strings.InStr(1, sData, sStartKey);
            if (iPos1 == 0) return "";
            iPos1 = iPos1 + Strings.Len(sStartKey);
            int iPos2 = Strings.InStr(iPos1, sData, sEndKey);
            if (iPos2 == 0) return "";
            string sOut = Strings.Mid(sData, iPos1, iPos2 - iPos1);
            return sOut;
        }

        public static string GetPathFromTube(string sURL)
        {
            string sFolderPath = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\";
            sURL += "&";
            string sTube = ExtractXML(sURL, "v=", "&").ToString();
            string[] files = System.IO.Directory.GetFiles(sFolderPath, "*.mp4");
            for (int i = 0; i < files.Length; i++)
            {
                string sPath = files[i];
                if (sPath.Contains(sTube))
                    return sPath;
            }
            return "";
        }
        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
        public static string Left(string source, int iHowMuch)
        {
            if (source.Length < iHowMuch)
                return source;
            return source.Substring(0, iHowMuch);
        }

        public static void FDetect(string sUserName)
        {
            string sCookieHomogenized = GetCookie("fdetect");
            if (!sCookieHomogenized.Contains(sUserName))
            {
                sCookieHomogenized += sUserName + ";";
            }
            StoreCookie("fdetect", sCookieHomogenized);

            string[] vDetect = sCookieHomogenized.Split(";");
            if (vDetect.Length > 2)
            {
                string sql = "Update users set fdetect='" + BMS.PurifySQL(sCookieHomogenized,255) + "' where username='" + sUserName + "'";
                gData.Exec(sql);
            }
        }
        public struct User
        {
            public bool LoggedIn;
            public bool Admin;
            public bool Banned;
            public string EmailAddress;
            public string UserId;
            public bool TwoFactorAuthorized;
            public double Require2FA;
            public string RandomXBBPAddress;
            public string AvatarURL;
            public string UserName;
        }
        public static User gUser(Page p)
        {
            User u = new User();
            if (p.Session["CurrentUser"] == null)
            {
                // Before dropping down to the guest level, attempt to deserialize the cookie.  This will log the user into the forum, but 2FA will not be enabled.
                // This means they cannot withdraw BBP, but they can see tweets and reply to prayers.  This should be less of a nuisance for most users; as then they will be logged in for 7 days at a time on level 1.  
                // But, we will still require them to click Log In with 2FA to withdraw.
                string sCookieUserName = GetCookie("CurrentUser");
                if (sCookieUserName != "" )
                {
                    bool fSuccess = Login(sCookieUserName, "", p.Session, "");
                    if (fSuccess)
                    {
                        FDetect(sCookieUserName);
                        return (User)p.Session["CurrentUser"];
                    }
                }
                User u1 = new User();
                u1.UserName = "Guest";
                u1.AvatarURL = "<img src='https://forum.biblepay.org/Themes/Offside/images/default-avatar.png'>";
                u1.LoggedIn = false;
                p.Session["CurrentUser"] = u1;
                return u1;
            }
            u = (User)p.Session["CurrentUser"];
            string sUN = u.UserName;
            string sAvatar = u.AvatarURL;
            return u;
        }

        public static double GetCompounded(double nROI)
        {
            double nBank = 10000;
            for (int nMonth = 1; nMonth <= 12; nMonth++)
            {
                double nReward = nBank * (nROI / 12);
                nBank += nReward;
            }
            double nCompounded = -1 * Math.Round(1 - (nBank / 10000), 2);
            return nCompounded;
        }

        public static string GetCookie(string sKey)
        {
            try
            {
                HttpCookie _pool = HttpContext.Current.Request.Cookies["credentials_" + sKey];
                if (_pool != null)
                {
                    string sOut = (_pool.Value ?? string.Empty).ToString();
                    string sDeciphered = Base65Decode(sOut);
                    return sDeciphered;
                }
            }catch(Exception ex)
            {

            }
            return "";
        }

        public static string Base65Encode(string sData)
        {
            string s1 = Base64Encode(sData);
            string s2 = s1.Replace("=", "[equal]");
            return s2;
        }

        public static string Base65Decode(string sData)
        {
            string s1 = sData.Replace("[equal]", "=");
            string s2 = Base64Decode(s1);
            return s2;
        }
        public static void StoreCookie(string sKey, string sValue)
        {
            try
            {
                string sEnc = Base65Encode(sValue);
                HttpCookie _pool = new HttpCookie("credentials_" + sKey);
                _pool[sKey] = sEnc;
                _pool.Expires = DateTime.Now.AddDays(7);
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Cookies.Add(_pool);
                }
                HttpContext.Current.Response.Cookies["credentials_" + sKey].Value = sEnc;
            }
            catch (Exception ex)
            {
                string sError = ex.Message;
                Log("Store Cookie: " + sError);
            }
        }



        public static string sScratchpad = "";
        public static void GetVideo(string sURL)
        {
            try
            {
                string vidArgs = sURL + " -w -f mp4 --verbose --write-description --no-check-certificate";
                string res = run_cmd("c:\\inetpub\\wwwroot\\Saved\\bin\\youtube-dl.exe", vidArgs);
            }
            catch(Exception ex)
            {
                Log("GetVideo::" + ex.Message);
            }
        }
    }
}
 