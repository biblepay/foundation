using Google.Authenticator;
using Microsoft.VisualBasic;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.PoolCommon;

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

        public static void ClearUser(HttpSessionState s)
        {
            User u = new User();
            u.LoggedIn = false;
            u.UserName = "Guest";
            u.TwoFactorAuthorized = false;
            u.Require2FA = 0;
            s["CurrentUser"] = u;

            StoreCookie("CurrentUser", u.UserName);

        }

        public static void CoerceUser(HttpSessionState Session)
        {
            User u = new User();
            u.UserName = GetBMSConfigurationKeyValue("administratorusername");
            u.LoggedIn = true;
            u.TwoFactorAuthorized = true;
            u.Require2FA = 1;
            Login(u.UserName, "0", Session, "coerce");
            return;
        }

        public static bool runCmd(string path, string command)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = @"" + command;
            cmdsi.WorkingDirectory = path;
            Process cmd = Process.Start(cmdsi);
            int nTimeout = 9000;
            bool fSuccess = cmd.WaitForExit(nTimeout); //wait indefinitely for the associated process to exit.
            return fSuccess;
        }
        public static string run_cmd(string sFileName, string args)
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
            catch (Exception ex)
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

        public static double GetUSDAmountFromBBP(double nBBPAmount)
        {
            if (BBP_USD == 0)
                UpdateBBPPrices();
            if (BBP_USD < .000001)
                return 0;

            return nBBPAmount * BBP_USD;
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
            if (c == null || c == DBNull.Value) return false;
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
                u.CPKAddress = d1["CPKAddress"].ToNonNullString();
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
                    if (extra == "coerce")
                    {
                        u.TwoFactorAuthorized = true;
                        u.LoggedIn = true;
                        h["CurrentUser"] = u;
                        return true;
                    }
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
        public static string GetSha256HashI(string rawData)
        {
            // The I means inverted (IE to match a uint256)
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string GetSha256HashS(string rawData)
        {
            // The I means inverted (IE to match a uint256)
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
            } catch (Exception ex2)
            {
                Log("Cannot send Mail: " + ex2.Message);
            }
            return false;
        }

        public static int iSent = 0;
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
            catch (Exception)
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
            sHomePath += "SAN" + sPathDelimiter;
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
        public static void Log(string sData, bool fQuiet = false)
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
            string sOverriddenPath = GetExtConfigurationKeyValue(GetBaseHomeFolder() + "bms.conf", "datadir");
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
                string sql = "Update users set fdetect='" + BMS.PurifySQL(sCookieHomogenized, 255) + "' where username='" + sUserName + "'";
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
            public string CPKAddress;
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
                if (sCookieUserName != "")
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
            } catch (Exception)
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
            catch (Exception ex)
            {
                Log("GetVideo::" + ex.Message);
            }
        }

        // www.directmailers.com/docs
        public struct DirectMailAddress
        {
            public string Name;
            public string AddressLine1;
            public string AddressLine2;
            public string City;
            public string State;
            public string Zip;

        };

        public struct DirectMailVariable 
        {
            public string FirstName;
            public string SenderName;
            public string SenderCompany;
            public string Paragraph1;
            public string Paragraph2;
            public string ImageURL;
            public string OpeningSalutation;
            public string ClosingSalutation;
        };

        public struct DirectMailLetter
        {
            public string Medium;
            public string Size;
            public bool DryRun;
            public string PostalClass;
            public string Template;
            public DirectMailAddress To;
            public DirectMailAddress From;
            public string Data;
            public string Description;
            public DirectMailVariable VariablePayload;
        }

        public struct DMResponse
        {
            public string RenderedPDF;
        }
        public static string MailLetter(DirectMailLetter letter)
        {
            string username = GetBMSConfigurationKeyValue("DMUSER");
            string password = GetBMSConfigurationKeyValue("DMPASS"); 

            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            string sPass = "Basic " + svcCredentials;
            string sURL = "https://print.directmailers.com/api/v1/letter/";
            string json = JsonConvert.SerializeObject(letter, Formatting.Indented);
            try
            {
                string sResult = BMS.GetWebJsonApi(sURL, "Authorization", sPass, "POST", json);
                if (sResult == "")
                    return "<error>Invalid Address Data</error>";

                DMResponse r = JsonConvert.DeserializeObject<DMResponse>(sResult);
                string sURL1 = r.RenderedPDF;
                return "<pdf>" + sURL1 + "</pdf>";
            }
            catch(Exception ex)
            {
                return "<error>" + ex.Message + "</error>";
            }
        }

        public static Code.PoolCommon.NFT GetSpecificNFT(string hash)
        {
            List<Code.PoolCommon.NFT> n = Saved.Code.PoolCommon.GetNFTList("all");
            for (int i = 0; i < n.Count; i++)
            {
                if (n[i].Hash == hash)
                {
                    return n[i];
                }
            }
            Code.PoolCommon.NFT o = new Code.PoolCommon.NFT();
            return o;
        }

        public static double GetHighBid(string nftid)
        {
            string sql = "Select max(bidamount) a from nftbid where nftid='" + nftid + "'";
            double nAmt = gData.GetScalarDouble(sql, "a");
            return nAmt;
        }

        public struct DACResult
        {
            public string sError;
            public string sResult;
            public string sTXID;
        };

        public static DACResult BuyNFT1(string sUserID, string sID, double nOfferPrice, bool fBidOnly)
        {
            DACResult d = new DACResult();
            d.sError = "";

            if (DataOps.GetUserRecord(sUserID).CPKAddress.ToNonNullString() == "")
            {
                d.sError = "Sorry, you must populate your 'Christian-Public-Key address', inside your Account Settings (go to  Account Edit from the left menu).  <br>You can find this address in your biblepay core home PC wallet in File | Receiving Addresses.  <br>This allows you to transfer ownership of your store bought NFTs to be sent to your home wallet.";
                return d;
            }
                
            Code.PoolCommon.NFT myNFT = GetSpecificNFT(sID);
            if (!myNFT.found)
            {
                d.sError = "Sorry, the NFT cannot be found.";
                return d;
            }

            if (!myNFT.fMarketable || myNFT.fDeleted)
            {
                d.sError = "Sorry, this NFT is not for sale.";
                return d;
            }

            double nMyBal = Saved.Code.BMS.GetBalance("BBP");
            // HARVEST MISSION CRITICAL TODO: FIX THIS ****************************************************** HARVEST ************************************************************************************
            /*
            if (false)
            {
                if (myNFT.nMinimumBidAmount > nMyBal)
                {
                    d.sError = "Sorry, the bid amount requested exceeds your balance.";
                    return d;
                }
            }
            */

            
            if (nOfferPrice < myNFT.nMinimumBidAmount)
            {
                d.sError = "Sorry this NFT has a minimum bid price of " + myNFT.nMinimumBidAmount.ToString();
                return d;
            }

            if (fBidOnly)
            {
                double nHighBid = GetHighBid(myNFT.Hash);

                if (nOfferPrice < nHighBid)
                {
                    d.sError = "Sorry, this NFT has a bid of " + nHighBid.ToString() + ", please bid higher.";
                    return d;
                }
                    
            }

            if (fBidOnly && nOfferPrice >= myNFT.nMinimumBidAmount && myNFT.fMarketable && !myNFT.fDeleted)
            {
                // Just bid
                string sql = "Insert Into nftbid (id, nftid, userid, added, bidamount) values (newid(), @nftid, @userid, getdate(), @amount)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@nftid", myNFT.Hash);
                command.Parameters.AddWithValue("@userid", DataOps.GetUserRecord(sUserID).UserId);
                command.Parameters.AddWithValue("@amount", nOfferPrice);
                gData.ExecCmd(command, false, false, false);
                d.sTXID = myNFT.Hash;
                return d;

            }
            else if (fBidOnly)
            {
                d.sError = "Sorry, the bid failed.  You have not been charged. ";
                return d;
            }
            // BUY

            if (nOfferPrice >= myNFT.nLowestAcceptableAmount && myNFT.fMarketable && !myNFT.fDeleted)
            {
                string sResult2 = Saved.Code.PoolCommon.BuyNFT(myNFT.Hash, DataOps.GetUserRecord(sUserID).CPKAddress, nOfferPrice);
                if (sResult2 == "")
                {
                    d.sTXID = myNFT.Hash;
                    PoolCommon.listBoughtNFT.Add(myNFT.Hash);
                    // Harvest mission critical todo: Charge their account for the NFT here, and add the reference to the Pool Description Deposit TxList
                    // This is also the place where we add a copy of the nft into the mynft table:
                    string sql = "Insert into mynft (id, nftid, userid, added, amount, bbpaddress, loqualityurl, hiqualityurl) values "
                        + "(newid(), @nftid, @userid, getdate(), @amount, @bbpaddress, @loqualityurl, @hiqualityurl)";
                    SqlCommand command = new SqlCommand(sql);
                    command.Parameters.AddWithValue("@nftid", myNFT.Hash);
                    command.Parameters.AddWithValue("@userid", DataOps.GetUserRecord(sUserID).UserId);
                    command.Parameters.AddWithValue("@amount", nOfferPrice);
                    command.Parameters.AddWithValue("@bbpaddress", DataOps.GetUserRecord(sUserID).CPKAddress);
                    command.Parameters.AddWithValue("@loqualityurl", myNFT.LoQualityURL);
                    command.Parameters.AddWithValue("@hiqualityurl", myNFT.HiQualityURL);
                    gData.ExecCmd(command, false, false, false);
                    return d;
                }
                else
                {
                    d.sError = "Sorry, the purchase failed.  You have not been charged. ";
                    return d;
                }
            }
            return d;
        }


        public static bool SessionToBool(HttpSessionState s, string sKey)
        {
            if (s[sKey] == null)
                return false;
            if (s[sKey].ToNonNullString() == "1")
                return true;
            return false;
        }


    public static string GetNFTDisplayList(bool fOrphansOnly, Page h)
    {

            bool fDigital = SessionToBool(h.Session,"chkDigital");
            bool fTweet = SessionToBool(h.Session,"chkSocial");
            string sTypes = "";
            if (fOrphansOnly)
            {
                sTypes = "orphan";
            }
            else
            {
                sTypes += fDigital ? "digital," : "";
                sTypes += fTweet ? "social" : "";
            }
            List<Code.PoolCommon.NFT> n = Saved.Code.PoolCommon.GetNFTList(sTypes);
            string sHTML = "<table><tr>";
            int iTD = 0;
            int nColsPerRow = 3;
            int nObjsPerPage = 4 * nColsPerRow;
            int nPageNo = (int)GetDouble(h.Request.QueryString["pag"] ?? "");
            int nStartRow = nPageNo * nObjsPerPage;
            int nEndRow = nStartRow + nObjsPerPage - 1;
            int nRows = n.Count / nColsPerRow;
            double nTotalPages = (int)Math.Ceiling((double)(nRows / nObjsPerPage)) + 1;

            for (int i = nStartRow; i < nEndRow && i < n.Count; i++)
            {
                // Each Orphan should be a div with their picture in it
                string sURLLow1 = n[i].LoQualityURL;
                string sURLLow = "";
                if (n[i].Type.Contains("social"))
                {
                    sURLLow = FreezerImage(sURLLow1);

                }
                else
                {
                    sURLLow = sURLLow1;
                }
                
                string sURLHi = n[i].HiQualityURL;
                string sName = n[i].Name;
                string sDesc = n[i].Description;
                string sID = n[i].Hash;
                double nBid = GetHighBid(sID);
                bool fOrphan = n[i].Type.Contains("Orphan");
                string sBuyItCaption = fOrphansOnly ? "Sponsor Me for " : "Buy it Price ";

                if (sURLLow.Contains("hrefhttps://"))
                {
                    sURLLow = "https://foundation.biblepay.org/Images/404.png";
                }
                if (n[i].fMarketable)
                {
                    string sButtonCaption = fOrphansOnly ? "Sponsor Now" : "Buy it Now";

                    string sButton = "<input type='button' onclick=\"location.href='NFTBrowse.aspx?buy=1&id=" 
                        + sID + "'\" id='buy" + sID + "' value='" + sButtonCaption + "' />";

                    string sPreviewURL = sURLLow + "?id=" + sID;
                    string sPreviewButton = "<input type='button' onclick=\"window.open('" + sPreviewURL + "');\" value='Preview' />";

                    string sBidButton = "<input type='button' onclick=\"var amt=prompt('Please enter the bid "
                        + "amount you are offering', '0'); location.href='NFTBrowse.aspx?bid=1&id=" 
                        + sID + "&amount='+amt;\" id='bid" + sID + "' value='Make Offer' />";

                    string sBuyItNowPrice = n[i].nBuyItNowAmount.ToString() + " BBP";
                    string sAsset = "<iframe xwidth=95% style='height: 200px;width:300px;' src='" + sURLLow + "'></iframe>";
                    if (sURLLow.Contains(".gif"))
                    {
                        sAsset = "<img style='height:200px;width:300px;' src='" + sURLLow + "'/>";
                    }
                    else if (sURLLow.Contains(".mp4") || sURLLow.Contains(".mp3"))
                    {
                        sAsset = "<video xclass='connect-bg' width='300' height='200' style='background-color:black' controls><source src='" + sURLLow + "' xtype='video/mp4' />        </video>";
                    }
                    //<img style='width:300px;height:250px' src='" + sURL + "'>"
                    string s1 = "<td style='padding:7px;border:1px solid white' cellpadding=7 cellspacing=7>"
                        + "<b>" + sName + "</b><br>" + sAsset
                        + "<br><div style='height:110px;width:300px;'>"
                        + sDesc + "</div><br><small><font color=green>" + sBuyItCaption + " " + sBuyItNowPrice;
                    if (!fOrphansOnly)
                    {
                        s1 += "&nbsp;•&nbsp;High Offer: " + nBid.ToString() + " BBP";
                    }
                    else
                    {
                        s1 += "&nbsp;•&nbsp;<small>" + sID.Substring(0, 8) + "</small>";

                    }

                    s1 += "<br>" + sButton;
                    if (!fOrphansOnly)
                    {
                        s1 += sBidButton;
                    }
                    s1 += "&nbsp;" + sPreviewButton + "</td>";
                    sHTML += s1;
                    iTD++;
                    if (iTD > nColsPerRow)
                    {
                        iTD = 0;
                        sHTML += "<td width=30%>&nbsp;</td></tr><tr>";
                    }
                }

                if (nBid > n[i].nReserveAmount && nBid > n[i].nLowestAcceptableAmount)
                {
                    // This user is logged in and browsing... If their HIGHEST bid exceeds the Reserve, and the amount of time has gone by since bidding started, accept the offer:
                    string sql = "Select min(Added) a from nftbid where bidamount > 0 and nftid='" + sID + "'";
                    double nAge = gData.GetScalarAge(sql, "a");
                    if (nAge > (60 * 60 * 24 * 3))
                    {
                        sql = "select nftid,bidamount,userid from nftbid  where nftid = '" + sID + "' and bidamount > 0 and bought is null order by bidamount desc";
                        DataTable dt1 = gData.GetDataTable(sql);
                        if (dt1.Rows.Count > 0)
                        {
                            // 4-17-2021
                            sql = "Update nftbid set bought=getdate() where id='" + dt1.Rows[0]["nftid"].ToString() + "'";
                            gData.Exec(sql);
                            Log("Auto buying NFT" + dt1.Rows[0]["nftid"].ToString());
                            BuyNFT1(dt1.Rows[0]["UserID"].ToString(), dt1.Rows[0]["nftid"].ToString(), GetDouble(dt1.Rows[0]["bidamount"]), false);
                        }
                    }
                }

            }
            sHTML += "</TR></TABLE>";
            var uri = new Uri(h.Request.Url.AbsoluteUri);
            string path = uri.GetLeftPart(UriPartial.Path);
            string sURL = path + "?t=1";
            sHTML += UICommon.GetPagControl(sURL, nPageNo, (int)nTotalPages);
            return sHTML;
        }
     

        public struct SimpleUTXO
        {
            public string Address;
            public string TXID;
            public double Amount;
            public double QueryAmount;
            public int Height;
            public int Ordinal;
            public int TxCount;
            public string Ticker;
            public int UTXOTxTime;
            public string Added;
            public bool found;
            public string Account;
            public double TotalBalance;
        };


        public static string TickerToName(string sTicker)
        {
            if (sTicker == "DOGE")
            {
                return "dogecoin";
            }
            else if(sTicker == "BTC")
            {
                return "bitcoin";
            }
            else if (sTicker == "DASH")
            {
                return "dash";
            }
            else if (sTicker == "LTC")
            {
                return "litecoin";
            }
            else if (sTicker == "XRP")
            {
                return "ripple";
            }
            else if (sTicker == "XLM")
            {
                return "stellar";
            }

            return "";

        }
        private static void Persist(SimpleUTXO u)
        {
            // CRITICAL TODO: STORE REQUEST DATETIME, and we only delete records older than 1 day with a requestdatetime < NNNN.....
            string sDele = "Delete from Blockchair where added < getdate()-1";
            gData.Exec(sDele);
            u.TXID = GetSha256HashI(u.Address + u.Amount.ToString());

            // Mission critical todo : Delete the blockchair data once per day (not once per query)
            DateTime sUTXOTime = UnixTimeStampToDateTime(u.UTXOTxTime);
            string sql = " IF NOT EXISTS (SELECT TXID FROM BlockChair WHERE txid='"
                                      + u.TXID + "') BEGIN \r\n INSERT INTO BlockChair (id,ticker,added,amount,queryamount,Address,ordinal,txid,height,account,TotalBalance,utxotxtime,txcount) values (newid(),'" 
                                      + u.Ticker + "',getdate(),'" + u.Amount.ToString() + "','" 
                                      + u.QueryAmount.ToString() + "','" + u.Address.ToString() + "','" + u.Ordinal.ToString() + "','" 
                                      + u.TXID + "','" + u.Height.ToString() + "','" + BMS.PurifySQL(u.Account, 256) + "','" + u.TotalBalance.ToString() 
                                      + "','" + sUTXOTime.ToString() + "','" + u.TxCount.ToString() + "') END";
            gData.Exec(sql, false, true);
            Log(sql);

        }

        public static int GetAddressAge(string sAddress, string sTicker)
        {
            string sql1 = "Select max(added) added1 from BlockChair where Address='" + sAddress + "' and ticker='" + BMS.PurifySQL(sTicker, 200) + "'";
            string sDt = gData.GetScalarString(sql1, "added1");
            TimeSpan vTime = DateTime.Now - Convert.ToDateTime(sDt);
            int age = (int)vTime.TotalSeconds;
            return age;
        }

        private static dynamic GetBlockChairData(string sURL)
        {
            //Allows us to log the request so we can control costs
            string sData = BMS.ExecMVCCommand(sURL);
            dynamic oJson = JsonConvert.DeserializeObject<dynamic>(sData);

            string sql = "Insert into BlockChairRequestLog (id,added,URL) values (newid(), getdate(), '" + sURL + "')";
            gData.Exec(sql);
            return oJson;
        }


        private static SimpleUTXO ConvertDataRowToUTXO(DataRow r)
        {
            SimpleUTXO u = new SimpleUTXO();
            u.Ticker = r["Ticker"].ToString();
            u.Address = r["Address"].ToString();
            u.Amount = GetDouble(r["Amount"]);
            u.Ordinal = (int)GetDouble(r["ordinal"]);
            u.Added = r["Added"].ToString();
            u.QueryAmount = GetDouble(r["QueryAmount"]);
            u.TXID = r["TXID"].ToString();
            u.Account = r["Account"].ToString();
            u.TotalBalance = GetDouble(r["TotalBalance"]);
            u.Height = (int)GetDouble(r["Height"]);
            u.found = true;
            return u;
        }
        public static SimpleUTXO GetDBUTXO(string sTicker, string sAddress, double nAmount)
        {
            string sAmountField = "Amount";
            string sql = "Select * from BlockChair where address='" + BMS.PurifySQL(sAddress, 256) + "' and Ticker='" + BMS.PurifySQL(sTicker, 200)
                + "' and (" + sAmountField + "= '" + nAmount.ToString() + "')";

            DataTable dt = gData.GetDataTable(sql, false);
            SimpleUTXO u = new SimpleUTXO();
            if (dt.Rows.Count > 0)
            {
                u = ConvertDataRowToUTXO(dt.Rows[0]);
                Log("Found " + u.Ticker + " for " + u.Amount.ToString() + " at height " + " " + u.Height.ToString());
                return u;
            }

            // First test if empty
            string sql2 = "Select * from BlockChair where address='" + BMS.PurifySQL(sAddress, 256) + "' and ticker='" + BMS.PurifySQL(sTicker, 200) + "'";
            dt = gData.GetDataTable(sql2, false);

            if (dt.Rows.Count > 0)
            {
                u.Ticker = dt.Rows[0]["Ticker"].ToString();
                u.Address = dt.Rows[0]["Address"].ToString();
                u.Height = (int)GetDouble(dt.Rows[0]["Height"]);
                if (u.Height < 0)
                {
                    // SPENT
                    u.found = true;
                    return u;
                }
            }
            return u;
        }

        private static string ToNonNull(object o)
        {
            if (o == null)
                return "";

            string o1= o.ToNonNullString();
            return o1;
        }
    
        public static List<SimpleUTXO> QueryUTXOs(string sTicker, string sAddress)
        {
            QueryUTXO(sTicker, sAddress, 0, 0);
            string sql = "Select * from BlockChair where address='" + BMS.PurifySQL(sAddress, 256) + "' and Ticker='" + BMS.PurifySQL(sTicker, 200) + "'";
            List<SimpleUTXO> l = new List<SimpleUTXO>();
            DataTable dt = gData.GetDataTable(sql, false);
            SimpleUTXO u = new SimpleUTXO();
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                u = ConvertDataRowToUTXO(dt.Rows[i]);
                if (u.Amount > 0)
                {
                    l.Add(u);
                }
            }
            return l;
        }

        // Blockchair integration
        public static SimpleUTXO QueryUTXO(string sTicker, string sAddress, double nAmount, int nUTXOTxTime)
        {
            SimpleUTXO u = GetDBUTXO(sTicker, sAddress, nAmount);
           
            u.UTXOTxTime = nUTXOTxTime;

            if (u.found)
                return u;

            /*
            SimpleUTXO q = GetDBUTXO(sTicker, sAddress, nAmount, true);
            if (q.found)
            {
                return q;
            }
            */

            string sKey = GetBMSConfigurationKeyValue("blockchairkey");

            try
            {
                string sURL = "";
                if (sTicker == "XLM")
                {

                    // With stellar, the user must have one transaction matching the amount(with pin suffix) + balance must be equal to or greater than that stake.
                    // HARVEST TODO: Loop through "operations", find "amount:", and with a matching receipt, and a greater balance, use the "amount" field.

                    sURL = "https://api.blockchair.com/stellar/raw/account/" + sAddress + "?transactions=true&operations=true&key=" + sKey;
                    dynamic oJson = GetBlockChairData(sURL);
                    dynamic oBalances = oJson["data"][sAddress]["account"]["balances"];

                    dynamic oOps = oJson["data"][sAddress]["operations"];
                    double nBalance = 0;
                    foreach (var b in oBalances)
                    {
                        if (b["asset_type"].Value == "native")
                        {
                            nBalance = GetDouble(b["balance"].Value);
                        }
                    }


                    foreach (var o in oOps)
                    {
                        u = new SimpleUTXO();
                        u.Ticker = sTicker;
                        u.Address = sAddress;
                        string sTxType = ToNonNull(o["type"]);
                        string sType = ToNonNull(o["asset_type"]);
                        if (sType == "native")
                        {
                            u.Amount = GetDouble(o["amount"].Value);
                            u.TXID = GetSha256HashI(u.Address + u.Amount.ToString());
                            if (u.Amount > 0)
                                 Persist(u);
                            if (u.Amount == nAmount && nAmount > 0 && nBalance > nAmount)
                                return u;
                        }
                    }

                }
                else if (sTicker == "XRP")
                {
                    // With Ripple, the user must have one total matching balance(to the pin) and no extra transactions.
                    // Ripple toDO: Loop through transactions, count the tx that matches the pin - and verify the amount is greater than the tx and use the tx amount


                    sURL = "https://api.blockchair.com/ripple/raw/account/" + sAddress + "?transactions=true&key=" + sKey;
                    dynamic oJson = GetBlockChairData(sURL);
                    dynamic oBalances = oJson["data"][sAddress]["account"]["account_data"];
                    dynamic oTx = oJson["data"][sAddress]["transactions"]["transactions"];

                    double nBalance = GetDouble(oBalances["Balance"].Value) / 1000000;

                    int nTxCount = 0;
                    foreach (dynamic oMyTx in oTx)
                    {
                        nTxCount++;
                        u = new SimpleUTXO();
                        u.Ticker = sTicker;
                        u.Address = sAddress;
                        u.Amount = GetDouble(oMyTx["tx"]["Amount"].Value) / 100000000;
                        u.TXID = GetSha256HashI(sAddress + u.Amount.ToString());
                        if (u.Amount > 0)
                        {
                            Persist(u);
                        }
                        if (u.Amount == nAmount && u.Amount > 0 && nBalance > nAmount)
                            return u;
                    }


                }
                else if (sTicker == "DOGE" || sTicker == "BTC" || sTicker == "DASH" || sTicker == "LTC")
                {
                    string sTickerName = TickerToName(sTicker);

                    sURL = "https://api.blockchair.com/" + sTickerName + "/dashboards/address/" + sAddress + "?key=" + sKey;
                    // https://api.blockchair.com/DOGE/dashboards/address/DJiaxWByoQASvhGPjnY6rxCqJkJxVvU41c

                    dynamic oJson = GetBlockChairData(sURL);

                    dynamic oBalances = oJson["data"][sAddress]["utxo"];
                    u.Ticker = sTicker;
                    u.Address = sAddress;

                    if (oBalances.Count == 0)
                    {
                        u.Height = -1;
                        Persist(u);
                        return u;
                    }
                    // http://jsonviewer.stack.hu/
                    foreach (var b in oBalances)
                    {
                        u.Amount = GetDouble(b["value"].Value) / 100000000;
                        u.TXID = b["transaction_hash"].Value;
                        u.Ordinal = (int)GetDouble(b["index"].Value);
                        u.Height = (int)GetDouble(b["block_id"].Value);
                     
                        Persist(u);
                        if (u.Amount == nAmount && nAmount > 0)
                            return u;
                    }

                }
                else if (sTicker == "ETH")
                {
                    sURL = "https://api.blockchair.com/ethereum/dashboards/address/" + sAddress + "?transactions=true&key=" + sKey;
                    dynamic oJson = GetBlockChairData(sURL);
                    dynamic oBalance = oJson["data"][sAddress.ToLower()]["address"];
                    int nTxCount = (int)GetDouble(oJson["data"][sAddress.ToLower()]["address"]["transaction_count"].Value);
                    double nBalance = GetDouble(oBalance["balance"].Value) / 100000000 / 10000000000;
                    dynamic oCalls = oJson["data"][sAddress.ToLower()]["calls"];
                    foreach (dynamic oCall in oCalls)
                    {
                        u = new SimpleUTXO();
                        u.Ticker = sTicker;
                        u.Address = sAddress;
                        u.Amount = GetDouble(oCall["value"].Value) / 100000000 / 10000000000;
                        u.TXID = GetSha256HashI(u.Address + u.Amount.ToString());

                        Persist(u);
                        if (u.Amount == nAmount && nAmount > 0 && nBalance > nAmount)
                            return u;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            // If we didnt find it... Persist the missing one...
            // This logic is flawed cause if they are trying to get a new utxo, maybe they will never be able to get it (if it was not sent yet).
            
            SimpleUTXO u1 = new SimpleUTXO();
            u1.Ticker = sTicker;
            u1.Address = sAddress;
            // Persist(u1);
            return u1;
        }
        
        public static string GetChartOfIndex()
        {
        
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.Black;
            System.Drawing.Color primaryColor = System.Drawing.Color.Blue;
            System.Drawing.Color textColor = System.Drawing.Color.White;
            c.Width = 1000;
            c.Height = 400;
            string sChartName = "BiblePay Weighted CryptoCurrency Index";
            string sTickers = "BTC/USD,DASH/BTC,DOGE/BTC,LTC/BTC,ETH/BTC,XRP/BTC,XLM/BTC,BBP/BTC";
            string[] vTickers = sTickers.Split(",");
            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;

            /*
            for (int k = 0; k < vTickers.Length; k++)
            {
                string sTheTicker = GE(vTickers[k], "/", 0);
                Series s = new Series(sTheTicker);
                s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
                // s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;
                s.LabelForeColor = textColor;
                s.Color = primaryColor;
                s.BackSecondaryColor = bg;
                c.Series.Add(s);
            }
            */

            //Index
            Series s = new Series("Index");
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;

            s.LabelForeColor = textColor;
            s.Color = primaryColor;
            s.BackSecondaryColor = bg;
            c.Series.Add(s);



            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;
            
            for (int i = 0; i < 180; i += 1)
            {
                DateTime dt = DateTime.Now.AddDays(i * -1);
                double dTotalIndex = 0;
                for (int j = 0; j < vTickers.Length; j++)
                {
                    string sTheTicker = GE(vTickers[j], "/", 0);
                    string sql = "Select * from QuoteHistory where added='" + dt.ToShortDateString() + "' and ticker='" + sTheTicker + "'";
                    DataTable dt1 = gData.GetDataTable(sql, false);
                    if (dt1.Rows.Count > 0)
                    {
                        double dA = GetDouble(dt1.Rows[0]["USD"]);
                        dTotalIndex += dA;
                    }

                    
                }
                c.Series["Index"].Points.AddXY(dt, dTotalIndex);

            }

            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;

            c.BackColor = bg;
            c.ForeColor = primaryColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/index.png");
            c.SaveImage(sSan);
            return sSan;

        }

        public static string GE(string sData,string sDelim, int iEle)
        {
            string[] vGE = sData.Split(sDelim);
            if (vGE.Length > iEle)
                return vGE[iEle];
            else
                return "";
        }
        public static void StoreHistory(string ticker, double sUSDValue, double sBTCValue, DateTime theDate)
        {
            string added = theDate.ToShortDateString();
            string sql = "Delete from QuoteHistory where ticker='" + ticker + "' and added='" + added + "'\r\nInsert Into QuoteHistory (id,added,ticker,usd,btc) values (newid(),'" 
                + added + "','" + ticker + "','" + sUSDValue.ToString() + "','" + sBTCValue.ToString() + "')";
            gData.Exec(sql);
        }
        // Once per day we will store the historical quotes, to build the cryptocurrency index chart

        public static void StoreQuotes(int offset)
        {
            try
            {
                DateTime theDate = DateTime.Now;

                if (offset < 0)
                {
                    theDate = DateTime.Now.Subtract(TimeSpan.FromDays(offset * -1));

                }
                string sTickers = "BTC/USD,DASH/BTC,DOGE/BTC,LTC/BTC,ETH/BTC,XRP/BTC,XLM/BTC,BBP/BTC";
                string[] vTickers = sTickers.Split(",");
                double nBTCUSD = BMS.RQ("BTC/USD");
                for (int i = 0; i < vTickers.Length; i++)
                {
                    double nQuote = BMS.RQ(vTickers[i]);
                    double nUSDValue = 0;
                    if (vTickers[i] != "BTC/USD")
                    {
                        nUSDValue = nBTCUSD * nQuote;
                        if (offset < 0)
                        {
                            nUSDValue += (offset * -125);
                        }

                    }
                    else
                    {
                        nUSDValue = nQuote;
                        if (offset < 0)
                        {
                            nQuote += (offset * -.000000123);
                        }
                    }
                    string sTicker = GE(vTickers[i], "/", 0);
                    StoreHistory(sTicker, nUSDValue, nQuote, theDate);

                }
            }catch(Exception ex)
            {
                Log("Store Quote History:" + ex.Message);
            }
        }


        public static string FreezerImage(string sURL)
        {
            try
            {
                string sql = "Select * from cache where thekey='" + BMS.PurifySQL(sURL, 999) + "'";
                string sTheValue = gData.GetScalarString(sql, "thevalue");
                if (sTheValue != "")
                {
                    return sTheValue;
                }
                string sFullURL = "http://api.screenshotlayer.com/api/capture?access_key=" + GetBMSConfigurationKeyValue("freezerkey") + "&url=" + HttpUtility.UrlEncode(sURL) + "&viewport=1440x900&width=350&format=png";

                string sFN = Guid.NewGuid().ToString() + ".png";
                string sFullName = GetFolderUploads("NFT") + "\\" + sFN;

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(sFullURL);
                Bitmap bitmap; 
                bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(sFullName, System.Drawing.Imaging.ImageFormat.Png);
                }

                stream.Flush();
                stream.Close();
                
                string sHostURL = "https://foundation.biblepay.org/Uploads/NFT/" + sFN;
                sql = "Insert into Cache (id,thekey,thevalue,added) values (newid(), '" + BMS.PurifySQL(sURL, 999) + "','" + sHostURL + "',getdate())";
                gData.Exec(sql);
                return sHostURL;
            }
            catch(Exception ex)
            {
                Log("FreezerImage Error:: " + ex.Message);
                return sURL;
            }
        }

    }
}
 