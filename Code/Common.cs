using Google.Authenticator;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Saved.Code
{
    public static class Common
    {
        public static Data gData = new Data(Data.SecurityType.REQ_SA);

        public static Pool _pool = null;
        public static XMRPool _xmrpool = null;
        public static double nCampaignRewardAmount = 10000;

        private static string sCachedHomePath = string.Empty;

        public static string GetSiteName(Page p)
        {
            return GetHeaderBanner(p);
        }

        public static string GetLongSiteName(Page p)
        {
            return GetBMSConfigurationKeyValue("longsitename");
        }

        public static string GetBioImg(string orphanid)
        {
            string sql = "Select URL from BIO where orphanid=@orphanid";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@orphanid", orphanid);
            string bio = gData.GetScalarString(command, "URL", false);
            return bio;
        }
        public static string ScrapeImage(string sURL, string sCharity, string sOrphanID)
        {
            // First check the database
            string sImg1 = GetBioImg(sOrphanID);
            if (sImg1 != "")
                return sImg1;

            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;
            MyWebClient w = new MyWebClient();
            string sData = "";
            try
            {
                sData = w.DownloadString(sURL);
            }
            catch (Exception ex)
            {

            }
            string sImg = ExtractXML(sData, "<img src=\"", "\"").ToString();
            if (sImg == "")
                return "";
            if (sCharity == "kairos" && sImg != "")
            {
                sImg = "https://kairoschildrensfund.com/bios/" + sImg;
            }
            PersistBioImg(sImg, sOrphanID);
            return sImg;

        }

        public static void PersistBioImg(string URL, string orphanid)
        {

            string sql = "Delete from BIO where orphanid = @orphanid\r\nInsert into BIO values (newid(), @orphanid, @url)";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@orphanid", orphanid);
            command.Parameters.AddWithValue("@url", URL);
            gData.ExecCmd(command);
        }

        public static void CoerceUser(HttpSessionState Session)
        {
             User u = new User();
             u.UserName = GetBMSConfigurationKeyValue("administratorusername");
             u.LoggedIn = true;
             u.TwoFactorAuthorized = true;
             u.Require2FA = 1;
             PersistUser(ref u);
             Session["CurrentUser"] = u;
        }

        public static void AdjBalance(double nAmount, string sUserId, string sNotes)
        {
            string sql = "Insert into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), '', @txid, @userid, getdate(), @amount, @height, @notes)";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", sUserId);
            command.Parameters.AddWithValue("@amount", nAmount);
            command.Parameters.AddWithValue("@txid", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@height", _pool._template.height);
            command.Parameters.AddWithValue("@notes", sNotes);
            gData.ExecCmd(command, false, true, true);
        }

        public static string GetSideBar(Page p)
        {


            item = 0;

            string sKeys = gUser(p).TwoFactorAuthorized ? "<li><a href='AccountEdit.aspx'><i class='fa fa-key'></i></a></li>" : "";

            string html = "<aside class='main-sidebar' id='mySidenav'>";
            html += "<section class='sidebar'><div class='user-panel' style='z-index: 9000;'>"
                + "<a onclick='closeNav();' href = '#' class='sidebar-toggle' data-toggle='offcanvas' role='button'>"
                + " <i class='fa fa-close'></i></a>"
                + " <div class='pull-left myavatar'> "
            + "	" + gUser(p).AvatarURL + ""
        + "					</div>"
        + "		<div class='pull-left info'>"
        + "		<p>" + gUser(p).UserName + "</p>"
        + "	</div>"
        + " <div class='myicons'><ul>" + sKeys + "</ul></div>"
        + "	<!--<div class='myicons'>"
        + "		<ul><li><a href ='showposts' ><i class='fa fa-comments'></i></a></li>"
        + "	    <li><a href = 'https://forum.biblepay.org/index.php?action=pm' ><i class='fa fa-envelope'></i></a></li>"
        + "	</ul></div>-->"
        + "	</div>"
        + "	<ul class='sidebar-menu'>";
       
                
           html += AddMenuOption("Doctrine", "Guides.aspx;Study.aspx;Illustrations.aspx;Illustrations.aspx?type=wiki;;MediaList.aspx", "Guides for Christians;Theological Studies;Illustrations/Slides;Wiki Theology;Video Lists & Media", "fa-life-ring");
           html += AddMenuOption("Community", "Default.aspx;PrayerBlog.aspx;PrayerAdd.aspx", "Home;Prayer Requests List Blog;Add New Prayer Request", "fa-ambulance");
           html += AddMenuOption("Reports", "Accountability.aspx;Viewer.aspx?target=collage", "Accountability;Orphan Collage", "fa-table");
           html += AddMenuOption("Dashboard", "Dashboard.aspx", "Dashboard", "fa-line-chart");
           html += AddMenuOption("Pool", "Leaderboard.aspx;GettingStarted.aspx;PoolAbout.aspx;BlockHistory.aspx;Viewer.aspx?target=" 
               + System.Web.HttpUtility.UrlEncode("https://minexmr.com/#worker_stats") + ";MiningCalculator.aspx",
               "Leaderboard;Getting Started;About;Block History;XMR Inquiry;Mining Calculator", "fa-sitemap");

           html += AddMenuOption("Account", "https://forum.biblepay.org/sso.php?source=https://foundation.biblepay.org/Default.aspx;Login.aspx?opt=logout;AccountEdit.aspx;Deposit.aspx;Deposit.aspx;FractionalSanctuaries.aspx", 
               "Log In;Log Out;Account Edit;Deposit;Withdrawal;Fractional Sanctuaries", "fa-unlock-alt");
           html += AddMenuOption("Admin", "Markup.aspx", "Markup Edit", "fa-wrench");
           html += "</section></aside>";

           return html;

        }


        private static int item = 0;
        private static string AddMenuOption(string MenuName, string URLs, string LinkNames, string sIcon)
        {

            double nEnabled = GetDouble(GetBMSConfigurationKeyValue(MenuName));
            if (nEnabled == -1)
                return "";
                
            string[] vURLs = URLs.Split(";");
            string[] vLinkNames = LinkNames.Split(";");


            var js2 = "   var xp = parseFloat(localStorage.getItem('bbpdd" + item.ToString() + "')); "
             + "   var xe = xp==0?1:0; localStorage.setItem('bbpdd" + item.ToString() + "', xe); var disp = xp == 0 ? 'none' : 'block';";

            var js3 = "   var xp = parseFloat(localStorage.getItem('bbpdd" + item.ToString() + "')); "
             + "   var xe = xp==0?1:0; var disp = xe == 0 ? 'none' : 'block';";

            string menu = "<li id ='button_" + MenuName + "' class='dropdown'>"
             + "	<a class='dropdown-toggle' href='#' data-toggle='dropdown' onclick=\"" + js2 + " $('#bbpdd" + item.ToString() + "').attr('expanded', xe); "
             +"     $('#bbpdd" + item.ToString() + "').css('display',disp);\" >"
             + "	<i class='fa " + sIcon + "'></i>&nbsp;<span>" + MenuName + "</span>"
             + "	<span class='pull-right-container'><i class='fa fa-angle-left pull-right'></i></span></a>"
             + "	<ul class='treeview-menu' id='bbpdd" + item.ToString() + "'><script>" + js3 + "$('#bbpdd" + item.ToString() + "').css('display',disp);</script>";


            for (int i = 0; i < vLinkNames.Length; i++)
            {
                menu += "<li><a href='" + vURLs[i]  + "'><span style='overflow:visibile;'>" + vLinkNames[i] + "</span></a></li>";
            }
            menu += "</ul></li>";

            item++;
            return menu;

        }

        public static string decipherSSO(string data)
        {
            string key = GetBMSConfigurationKeyValue("ssokey");
            string outText = "";
            int keypos = 0;
            for (int i = 0; i < data.Length; i++)
            {
                string keychar = key[keypos].ToString();
                outText += (char)((short)data[i] ^ (short)keychar[0]);
                keypos++;
                if (keypos > key.Length - 1)
                    keypos = 0;
            }
            return outText;
        }

        public static void IncSysByFloat(string sKey, double nFloat)
        {
            string sql = "select Value from System where  systemKey = '" + sKey + "'";
            double dAmt = gData.GetScalarDouble(sql, "Value");
            dAmt += nFloat;
            sql = "Update System set Value='" + dAmt.ToString() + "' where SystemKey = '" + sKey + "'";
            gData.Exec(sql);
        }

        public static void IncrementAmountByFloat(string table, double nincrby, string userid)
        {
            try
            {
                string sql = "Select count(*) ct from " + table + " where userid=@userid";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", userid);
                double nCt = gData.GetScalarDouble(command, "ct");
                if (nCt == 0)
                {
                    // Add the shell
                    sql = "insert into " + table + " (id, userid, added, amount) values (newid(), @userid, getdate(), 0)";
                    command = new SqlCommand(sql);
                    command.Parameters.AddWithValue("@userid", userid);
                    gData.ExecCmd(command, false, true, true);
                }
                sql = "Update " + table + " set updated=getdate(),amount=amount+" + nincrby.ToString() + " where userid=@userid and id in (Select top 1 ID from " + table + " where userid=@userid)";
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", userid);
                gData.ExecCmd(command, false, true, true);
            }catch(Exception ex)
            {
                Log("IncAmountByFloat::" + ex.Message);
            }
        }
        
        public static string GetBaseHomeFolder()
        {
            string sHomePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
             Environment.OSVersion.Platform == PlatformID.MacOSX) ? Environment.GetEnvironmentVariable("HOME")
                 : Environment.ExpandEnvironmentVariables("%APPDATA%");
            sHomePath = "c:\\inetpub\\wwwroot\\";

            return sHomePath;
        }

        public static string RenderGauge(int width, string Name, int value)
        {

            string s = "<div id='chart_div'></div><script type=text/javascript> google.load( *visualization*, *1*, {packages:[*gauge*]});"
                + "     google.setOnLoadCallback(drawChart);"
                + "      function drawChart() {"
                + "      var data = new google.visualization.DataTable();"
                + "      data.addColumn('string', 'item');"
                + "      data.addColumn('number', 'value');     "
                + "     data.addRows(1);";
            s += "data.setValue(0,0,'" + Name + "');";
            s += "data.setValue(0,1," + value.ToString() + ");";
            s += "var options = {width: " + width.ToString() + ", height: " + width.ToString() + ",redFrom: 90, redTo: 100,yellowFrom:75, yellowTo: 90,minorTicks: 5};";
            s += " var chart = new google.visualization.Gauge(document.getElementById('chart_div'));";
            s += "chart.draw(data, options); }";
            s += "</script>";
            s = s.TrimEnd(',').Replace('*', '"');
            return s;

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
                if (u.Require2FA != 1)
                {
                    u.LoggedIn = true;
                    u.TwoFactorAuthorized = false;
                    h["CurrentUser"] = u;
                    return true;
                }
                else 
                {
                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    if (pin == "")
                    {
                        u.LoggedIn = false;
                        u.TwoFactorAuthorized = false;
                        h["CurrentUser"] = u;
                        return false;
                    }
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

        public static string GetTableHTML(string sql)
        {
            string css = "<style> html {    font-size: 1em;    color: black;    font-family: verdana }  .r1 { font-family: verdana; font-size: 10; }</style>";
            string logo = "https://www.biblepay.org/wp-content/uploads/2018/04/Biblepay70x282_96px_color_trans_bkgnd.png";
            string name = "BiblePay";
            string sLogoInsert = "<img width=300 height=100 src='" + logo + "'>";
            string HTML = "<HTML>" + css + "<BODY><div><div style='margin-left:12px'><TABLE class=r1><TR><TD width=95%>" + sLogoInsert
                + "<td width=5% align=right>Accountability</td><td>" + DateTime.Now.ToShortDateString() + "</td></tr>";

            HTML += "<TR><TD><td></tr>" + "<TR><TD><td></tr>" + "<TR><TD><td></tr>";
            HTML += "</table>";

            string header = "<TR><Th width=20%>Date<Th>Type<th>Amount<th width=30%>Charity<Th width=30%>Notes</tr>";
            HTML += "<table width=100%>" + header + "<tr><td colspan=5 width=100%><hr></tr>";
            SqlCommand command = new SqlCommand(sql);

            DataTable dt = gData.GetDataTable(command, false);
            double nDR = 0;
            double nCR = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string row = "<tr><td align=right>" + dt.Rows[i]["Added"].ToString() + "<td align=right>" + dt.Rows[i]["type"].ToString()
                    + "<td align=right>" + DoFormat(GetDouble(dt.Rows[i]["Amount"])) + "<td align=right>" + dt.Rows[i]["Charity"].ToString() 
                    + "<td align=right>" + dt.Rows[i]["Notes"].ToString() + "</tr>";
                // Add the totals
                bool fDebit = dt.Rows[i]["type"].ToString() == "DR";
                double dAmt = GetDouble(dt.Rows[i]["Amount"]);
                if (fDebit)
                {
                    nDR += dAmt;
                }
                else
                {
                    nCR += dAmt;
                }
                HTML += row;

            }

            HTML += "<tr><td>&nbsp;</td></tr>";
            HTML += "<tr><td>TOTAL DEBITS:<td><td>" + DoFormat(nDR) + "</tr>";
            HTML += "<tr><TD>TOTAL CREDITS:<td><td>" + DoFormat(nCR) + "</tr>";
            HTML += "</body></html>";
            return HTML;
        }


        public static void MsgBox(string sTitle, string sBody, System.Web.UI.Page p)
        {
            p.Session["MSGBOX_TITLE"] = sTitle;
            p.Session["MSGBOX_BODY"] = sBody;
            p.Response.Redirect("MessagePage.aspx");
        }

        public static double GetEstimatedHODL(bool fWithCompounding, double nBonusPercent)
        {
            string sql = "select sum(amount)/3/4500001*365 amt from sanctuaryPayment where added > getdate()-3.15";
            double nROI = gData.GetScalarDouble(sql, "amt");
            nROI += nBonusPercent;
            if (fWithCompounding)
            {
                nROI = GetCompounded(nROI);
            }
            return nROI;
        }


        public static string GetFooter(Page p)
        {
            string sOverridden = GetBMSConfigurationKeyValue("footer");
            if (sOverridden.Length > 0)
                return sOverridden;

            string sFooter = DateTime.Now.Year.ToString() + " - " + GetLongSiteName(p);
            return sFooter;

        }
        public static string GetHeaderImage(Page p)
        {
            if (p.Request.Url.OriginalString.ToLower().Contains("saved"))
            {
                return "images/SavedOneLogo.png";
            }
            else
            {
                return "Images/bbphoriz.png";
            }
        }

        public static string NotNull(object o)
        {

            if (o == null || o == DBNull.Value) return "";
            return o.ToString();
        }
        public static string GetHeaderBanner(Page p)
        {
            return GetBMSConfigurationKeyValue("sitename");
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
                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("1", "2"); // Do not change these values, change the config values.
                client.Port = 587;        // this is critical
                client.EnableSsl = true;  // this is critical
                                          // smtp.office365.com; sender@domain.org; smtppassword (Works with exchange)
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
        public static void Log(string sData, bool fQuiet=false)
        {
            lock (cs_log)
            {
                {
                    try
                    {
                        iRowModulus++;
                        if ((fQuiet && iRowModulus % 100 == 0) || (!fQuiet))
                        {
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

        private static NBitcoin.RPC.RPCClient _rpcclient = null;

        public static NBitcoin.RPC.RPCClient GetLocalRPCClient()
        {
            if (_rpcclient == null)
            {
                NBitcoin.RPC.RPCCredentialString r = new NBitcoin.RPC.RPCCredentialString();
                System.Net.NetworkCredential t = new System.Net.NetworkCredential(GetBMSConfigurationKeyValue("rpcuser"), GetBMSConfigurationKeyValue("rpcpassword"));
                r.UserPassword = t;
                string sHost = GetBMSConfigurationKeyValue("rpchost");
                NBitcoin.RPC.RPCClient n = new NBitcoin.RPC.RPCClient(r, sHost, NBitcoin.Network.BiblepayMain);
                _rpcclient = n;
                return n;
            }
            else
            {
                try
                {
                    var nbal=                    _rpcclient.GetBalance();
                    var n0 = 0;
                }
                catch(Exception ex)
                {
                    _rpcclient = null;
                    return GetLocalRPCClient();
                }
                return _rpcclient;
            }
        }


        public static string GetNewDepositAddress()
        {
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
            string sAddress = n.GetNewAddress().ToString();
            return sAddress;
        }

        public static int GetHeight()
        {
            object[] oParams = new object[1];
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
            dynamic oOut = n.SendCommand("getmininginfo");
            int nBlocks = (int)GetDouble(oOut.Result["blocks"]);
            return nBlocks;
            
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

        public static bool UpdateSingleField(string sTable, string field1, string fieldvalue, string fieldkey)
        {
            string sql = "Update " + sTable + " Set " + field1 + "=@fieldvalue WHERE id=@key";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@field1", field1);
            command.Parameters.AddWithValue("@fieldvalue", fieldvalue);
            command.Parameters.AddWithValue("@key", fieldkey);
            gData.ExecCmd(command);
            return true;
        }

        public static void PersistUser(ref User u)
        {
            // Store in database if not there, otherwise pull in ID; set picture=avatar
            string sql0 = "Select * from Users where UserName=@un";
            SqlCommand command0 = new SqlCommand(sql0);
            command0.Parameters.AddWithValue("@un", u.UserName);
            DataTable dt = gData.GetDataTable(command0, false);
            if (dt.Rows.Count == 0)
            {
                //Add the user here
                string sql = "Insert into Users (id,emailaddress,passwordhash,added,updated,picture,username,admin) values (newid(),'','',getdate(),getdate(),@avatar,@username,0)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@avatar", u.AvatarURL);
                command.Parameters.AddWithValue("@username", u.UserName);
                gData.ExecCmd(command);
            }
            else
            {
                // Freshen
                string sql = "Update Users set picture=@avatar,updated=getdate() where username=@username";
                SqlCommand command = new SqlCommand(sql);
                   if (u.AvatarURL == "" || u.AvatarURL == null)
                {
                    u.AvatarURL = " ";
                }
                command.Parameters.AddWithValue("@avatar", u.AvatarURL);
                command.Parameters.AddWithValue("@username", u.UserName);
                gData.ExecCmd(command);
            }
            dt = gData.GetDataTable(command0, false);
            if (dt.Rows.Count > 0)
            {
                u.UserId = dt.Rows[0]["Id"].ToString();
                u.Require2FA = GetDouble(dt.Rows[0]["TwoFactor"]);
            }

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
            string sFolderPath = "h:\\youtube\\";
            //string[] vTube = sURL.Split(new string[] { "/" }, StringSplitOptions.None);
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
        public static string GetNotes(string sPath)
        {
            string sNotesPath = sPath.Replace(".mp4", ".description");
            if (!System.IO.File.Exists(sNotesPath))
                return "";

            System.IO.StreamReader file = new System.IO.StreamReader(sNotesPath);
            string data = file.ReadToEnd();
            data = data.Replace("'", "");
            data = data.Replace("`", "");
            data = data.Replace("\"", "");
            if (data.Length > 7999)
                data = data.Substring(0, 7999);

            return data;
        }
        public static string Chop(string source, int iHowMuch)
        {
            int nLeft = source.Length - iHowMuch;
            if (nLeft < 1)
                return "";
            string sOut = source.Substring(0, nLeft);
            return sOut;
        }

        public static string GetAvatar(object field)
        {
            string sUserPic = NotNull(field); 
            if (sUserPic == "")
            {
                sUserPic = "<img src='images/emptyavatar.png' width=50 height=50 >";
            }
            return sUserPic;
        }
        public struct User
        {
            //public string EmailAddress;
            public bool LoggedIn;
            public bool Admin;
            public string UserId;
            public bool TwoFactorAuthorized;
            public double Require2FA;
            //public string ;
            public string AvatarURL;
            public string UserName;
        }
        public static User gUser(Page p)
        {
            User u = new User();
            if (p.Session["CurrentUser"] == null)
            {
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
            double nCompounded = Math.Abs(1 - (nBank / 10000));
            return nCompounded;
        }
        public static void GetVideo(string sURL)
        {
            MyWebClient w = new MyWebClient();
            string sData = w.DownloadString(sURL);
            string sTitle = ExtractXML(sData, "<title>", "</title>").ToString();
            sTitle = sTitle.Replace("YouTube", "");
            sTitle = sTitle.Replace("- ", "");
            sTitle = sTitle.Trim();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "C:\\code\\youTubeDownloader\\youtube-dl.exe";
            psi.WorkingDirectory = "h:\\youtube\\";
            psi.Arguments = sURL + " -w --verbose --write-description --no-check-certificate";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(psi);
            p.WaitForExit(60000 * 15);
        }

    }


    public class MyWebClient : System.Net.WebClient
    {
        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            System.Net.WebRequest w = base.GetWebRequest(uri);
   
            w.Timeout = 7000;
            return w;
        }
    }



}
 