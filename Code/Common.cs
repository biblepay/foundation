using Google.Authenticator;
using Microsoft.VisualBasic;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Web.UI;

namespace Saved.Code
{

    public static class FileSplitter
    {

        public static bool RelinquishSpace(string sPath)
        {
            try
            {
                string sDir = Path.Combine(Path.GetTempPath(), "SVR" + sPath.GetHashCode().ToString());
                Directory.Delete(sDir, true);
                return true;
            }
            catch(Exception ex)
            {
                Common.Log("Unable to relinquish space in " + sPath);
            }
            return false;
        }

        public static string SplitFile(string sPath)
        {
            int iPart = 0;
            string sDir = Path.Combine(Path.GetTempPath(), "SVR" + sPath.GetHashCode().ToString());
            using (Stream source = File.OpenRead(sPath))
            {
                byte[] buffer = new byte[10000000];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string sPartPath = Path.Combine(sDir, iPart.ToString() + ".dat");
                    if (!System.IO.Directory.Exists(sDir))
                        System.IO.Directory.CreateDirectory(sDir);
                    Stream dest = new FileStream(sPartPath, FileMode.Create);
                    dest.Write(buffer, 0, bytesRead);
                    dest.Close();
                    iPart++;
                }
            }
            return sDir;
        }
        private static int MAX_PARTS = 7000;
        public static void ResurrectFile(string sFolder, string sFinalFileName)
        {
            DirectoryInfo di = new DirectoryInfo(sFolder);
            string sMasterOut = Path.Combine(sFolder, sFinalFileName);
            Stream dest = new FileStream(sMasterOut, FileMode.Create);
            for (int i = 0; i < MAX_PARTS; i++)
            {
                string sFN = i.ToString() + ".dat";
                string sPath = Path.Combine(di.FullName, sFN);
                if (File.Exists(sPath))
                {
                    byte[] b = System.IO.File.ReadAllBytes(sPath);
                    dest.Write(b, 0, b.Length);
                }
                else
                {
                    break;
                }
            }
            dest.Close();
        }
    

    }

    public static class Common
    {
        public static Data gData = new Data(Data.SecurityType.REQ_SA);

        public static Pool _pool = null;
        public static XMRPool _xmrpool = null;
        public static double nCampaignRewardAmount = 10000;

        private static string sCachedHomePath = string.Empty;

        public static string CleanseHeading(string sMyHeading)
        {
            int iPos = 0;
            for (int i = 0; i < sMyHeading.Length; i++)
            {
                if (sMyHeading.Substring(i, 1) == "-")
                    iPos = i;
            }
            if (iPos > 1)
            {
                string sOut = sMyHeading.Substring(0, iPos - 1);
                return sOut;
            }
            return sMyHeading;
        }

        public static string PoolBonusNarrative()
        {
            double nBonus = GetDouble(GetBMSConfigurationKeyValue("PoolBlockBonus"));
            if (nBonus > 0)
            {
                string sNarr = "We are giving away an extra " + nBonus.ToString() + " BBP per block split equally across participating miners who have more than 110 shares in the leaderboard (see Block Bonus).";
                return sNarr;
            }
            return "";
        }
        public static void ConvertVideos()
        {
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
                    string sNotes = CleanseHeading(sHeading) + "\r\n\r\n" + Left(GetNotes(sPath), 4000);
                    Task<List<string>> t = Uplink.Store2("video-" + sNewFileName, "", "", sPath);
                    if (t.Result.Count > 0)
                    {
                        sql = "Insert into Rapture (id,added,Notes,URL,FileName,Category,UserID) values (newid(), getdate(), @notes, @url, @filename, @category, @userid)";
                        SqlCommand command = new SqlCommand(sql);
                        command.Parameters.AddWithValue("@notes", sNotes);
                        command.Parameters.AddWithValue("@url", t.Result[0]);
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
        public static string GetSiteName(Page p)
        {
            return GetHeaderBanner(p);
        }
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

        public static string GetPagControl(string sURL, int iActivePage, int iTotalPages)
        {
            if (iActivePage < 0)
                iActivePage = 0;
            int iPriorPage = iActivePage - 1;
            if (iPriorPage < 0)
                iPriorPage = 0;

            int iNextPage = iActivePage + 1;
            if (iNextPage > iTotalPages - 1)
                iNextPage = iTotalPages - 1;


            string pag = "<div class='pagination'><a href='" + sURL + "&pag=0'>&laquo;</a>";
            pag += "<a href='" + sURL + "&pag=" + (iPriorPage).ToString() + "'>&larr;</a>";

            int iMaxPages = 18;
            int iPos = 0;
            int iStartPage = iActivePage - (iMaxPages / 2);
            if (iStartPage < 1) iStartPage = 1;

            for (int i = iStartPage; i <= iTotalPages; i++)
            {
                string sActive = "";
                if ((i-1) == iActivePage)
                    sActive = "class='active'";
                string sRow = "<a href='" + sURL + "&pag=" + (i - 1).ToString() + "' " + sActive + "> " + i.ToString() + " </a>";
                pag += sRow;
                iPos++;
                if (iPos >= iMaxPages)
                    break;
            }
            pag += "<a href='" + sURL + "&pag=" + (iNextPage).ToString() + "'>&rarr;</a>";

            pag += "<a href='" + sURL + "&pag=" + (iTotalPages - 1).ToString() + "'>&raquo;</a></div>";

            return pag;
        }
        public static string GetBioImg(string orphanid)
        {
            string sql = "Select BioURL from SponsoredOrphan where orphanid=@orphanid";
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


        const int logonWithProfile = 1;
        const int logonTypeNetwork = 3;
        const int logonProviderDefault = 0;


        public const UInt32 Infinite = 0xffffffff;
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

        public static string run_cmd2(string cmd, string args)
        {
            string result = "";

            if (ImpersonateUser(GetBMSConfigurationKeyValue("impersonationuser"), GetBMSConfigurationKeyValue("impersonationdomain"), GetBMSConfigurationKeyValue("impersonationpassword")))
            {
                ProcessStartInfo start = new ProcessStartInfo();
                //Found in "where python"
                start.FileName = GetBMSConfigurationKeyValue("pypath");
                start.Arguments = string.Format("{0} {1}", cmd, args);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
                UndoImpersonation();
                return result;
            }
            else
            {
                //Your impersonation failed. Therefore, include a fail-safe mechanism here.
                Log("Impersonation failed-most likely the password is bad.");
            }
            return result;

        }

        public static int B2N(bool bIn)
        {
            return bIn ? 1 : 0;
        }

        public static string VerifyEmailAddress(string sEmail, string sID)
        {
            string sKey = GetBMSConfigurationKeyValue("thechecker");
            string sURL = "https://api.thechecker.co/v2/verify?email=" + sEmail + "&api_key=" + sKey;
            string sOut = BMS.GetWebJsonApi(sURL, "", "");
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
                    AdjBalance(nAmt, sUserID, "Video Reward for " + Math.Round(nPCT * 100,2).ToString() + "% for [" + sCategory + "]");
                    sql = "Update Tip set Paid = getdate() where id = '" + dt1.Rows[i]["id"].ToString() + "'";
                    gData.Exec(sql);
                }
            }catch(Exception ex)
            {
                Log("Unable to pay " + ex.Message);
            }
        }


        /*
        public static string CommitToObjectStorage(string sPath)
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
        */


        public static string GetTd(DataRow dr, string colname, string sAnchor)
        {
            string val = dr[colname].ToString();
            string td = "<td>" + sAnchor + val + "</a></td>";
            return td;
        }

        public static string RenderControlToHtml(Control ControlToRender)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter stWriter = new System.IO.StringWriter(sb);
            System.Web.UI.HtmlTextWriter htmlWriter = new System.Web.UI.HtmlTextWriter(stWriter);
            ControlToRender.RenderControl(htmlWriter);
            return sb.ToString();
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
            //if (GetDouble(p.Session["Tweet"]) == 1)
            //    return "";
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
       
                
           html += AddMenuOption("Doctrine", "Guides.aspx;Study.aspx;Illustrations.aspx;Illustrations.aspx?type=wiki;MediaList.aspx;RequestVideo.aspx", "Guides for Christians;Theological Studies;Illustrations/Slides;Wiki Theology;Video Lists & Media;Request a Video", "fa-life-ring");
           html += AddMenuOption("Community", "Default.aspx;PrayerBlog.aspx;PrayerAdd.aspx;Dashboard.aspx;LandingPage?faucet=1", "Home;Prayer Requests List Blog;Add New Prayer Request;Salvation Dashboard;Faucet", "fa-ambulance");
           html += AddMenuOption("Orphans", "SponsorOrphanList.aspx;DonorMatchList.aspx;Report?name=myorphans;Report?name=orphantx", "Sponsor An Orphan;Donor Match List;My Orphans;My Orphan Payments", "fa-child");
            
           html += AddMenuOption("Reports", "Accountability.aspx;Viewer.aspx?target=collage;Partners.aspx", "Accountability;Orphan Collage;Partners", "fa-table");
           // html += AddMenuOption("Dashboard", "Dashboard.aspx", "Dashboard", "fa-line-chart");
           html += AddMenuOption("Pool", "Leaderboard.aspx;GettingStarted.aspx;PoolAbout.aspx;BlockHistory.aspx;Viewer.aspx?target=" 
               + System.Web.HttpUtility.UrlEncode("https://minexmr.com/#worker_stats") + ";MiningCalculator.aspx",
               "Leaderboard;Getting Started;About;Block History;XMR Inquiry;Mining Calculator", "fa-sitemap");

           html += AddMenuOption("Account", "https://forum.biblepay.org/sso.php?source=https://foundation.biblepay.org/Default.aspx;Login.aspx?opt=logout;AccountEdit.aspx;Deposit.aspx;Deposit.aspx;FractionalSanctuaries.aspx", 
               "Log In;Log Out;Account;Deposit;Withdrawal;Fractional Sanctuaries", "fa-unlock-alt");
           html += AddMenuOption("Tweets", "TweetList;TweetAdd", "Tweet List;Advertise a Tweet", "fa-line-chart");

           html += AddMenuOption("Admin", "Markup.aspx", "Markup Edit", "fa-wrench");
           html += "</section></aside>";

           return html;
        }

        public static string GetComments(string id, Page p)
        {
            // Shows the comments section for the object.  Also shows the replies to the comments.
            string sql = "Select * from Comments Inner Join Users on Users.ID = Comments.UserID  where comments.ParentID = @id order by comments.added";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            string sHTML = "<div><h3>Comments:</h3><br>"
                + "<table style='padding:10px;' width=73%>"
                + "<tr><th width=14%>User<th width=10%>Added<th width=64%>Comment</tr>";

                for (int i = 0; i < dt.Rows.Count; i++)
            {
                SavedObject s = RowToObject(dt.Rows[i]);

                string sUserPic = GetAvatar(s.Props.Picture);

                string sUserName = NotNull(s.Props.UserName);
                if (sUserName == "")
                    sUserName = "N/A";
                string sBody = ReplaceURLs(s.Props.Body);

                string div = "<tr><td>" + sUserPic + "<br>" + sUserName + "</br></td><td>" + s.Props.Added.ToString() + "</td><td style='border:1px solid lightgrey'><br>" + sBody
                    + "</td></tr>";
                sHTML += div;

            }
            sHTML += "</table><table width=100%><tr><th colspan=2><h2>Add a Comment:</h2></tr>";

            if (!gUser(p).LoggedIn)
            {
                sHTML += "<tr><td><font color=red>Sorry, you must be logged in to add a comment.</td></tr></table></div>";
                return sHTML;
            }

            string sButtons = "<tr><td>Comment:</td><td><textarea id='txtComment' name='txtComment' rows=10  style='width: 70%;' cols=70></textarea><br><br><button id='btnSaveComment' name='btnSaveComment' value='Save'>Save Comment</button></tr>";

            sButtons += "</table></div>";

            sHTML += sButtons;
            return sHTML;
        }



        public static string GetHPSLabel(double dHR)
        {
            string KH = Math.Round(dHR / 1000,2).ToString() + " KH/S";
            string MH = Math.Round(dHR / 1000000,2).ToString() + " MH/S";
            string H = Math.Round(dHR,2).ToString() + " H/S";
            if (dHR < 10000)
            {
                return H;
            }
            else if (dHR >= 10000 && dHR <= 1000000)
            {
                return KH;
            }
            else if (dHR > 1000000)
            {
                return MH;
            }
            else
            {
                return H;
            }

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
        
        public static User GetUserRecord(string id)
        {
            string sql = "Select * from USERS where id = '" + id + "'";
            DataTable dt = gData.GetDataTable(sql);
            User u = new User();
            if (dt.Rows.Count > 0)
            {
                u.EmailAddress = dt.Rows[0]["EmailAddress"].ToString();
                u.UserId = dt.Rows[0]["id"].ToString();
                u.UserName = dt.Rows[0]["username"].ToString();
            }
            return u;
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
                    double dAmtGross = gData.GetScalarDouble(sql, "MonthlyAmount");
                    double dMatchPct = GetDouble(dt1.Rows[i]["matchpercentage"]);
                    double dMatchAmt = dAmtGross * dMatchPct;
                    double dAmt = Math.Round(dAmtGross - dMatchAmt, 2);
                    double dBalance = GetUserBalance(userid);
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
                        AdjBalance(-1 * dMonthly, userid, sNotes);
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
                                User u = GetUserRecord(sUserId);
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
            }catch(Exception ex2)
            {
                Log("PayMonthlyOrphanSponsorships[2]:" + ex2.Message);
            }
            string test = "";

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
                u.RandomXBBPAddress = d1["RandomXBBPAddress"].ToNonNullString();
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

        public static string GetCharityTableHTML(string sql)
        {
            string css = "<style> html {    font-size: 1em;    color: black;    font-family: verdana }  .r1 { font-family: verdana; font-size: 10; }</style>";
            string logo = "https://www.biblepay.org/wp-content/uploads/2018/04/Biblepay70x282_96px_color_trans_bkgnd.png";
            string name = "BiblePay";
            string sLogoInsert = "<img width=300 height=100 src='" + logo + "'>";
            string HTML = "<HTML>" + css + "<BODY><div><div style='margin-left:12px'><TABLE class=r1><TR><TD width=95%>" + sLogoInsert
                + "<td width=5% align=right>Accountability</td><td>" + DateTime.Now.ToShortDateString() + "</td></tr>";

            HTML += "<TR><TD><td></tr>" + "<TR><TD><td></tr>" + "<TR><TD><td></tr>";
            HTML += "</table>";

            string header = "<TR><Th>Date<Th>Amount<th>Charity<th>Child Name<th>Child ID<th>Balance<Th width=30%>Notes</tr>";
            HTML += "<table width=100%>" + header + "<tr><td colspan=7 width=100%><hr></tr>";
            SqlCommand command = new SqlCommand(sql);
            double nDR = 0;
            double nCR = 0;
            double nTotal = 0;
            DataTable dt = gData.GetDataTable(command, false);
            string sCharity = "";
            string sOldDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                double dAmt = GetDouble(dt.Rows[i]["Amount"]);
                string sType = dAmt >= 0 ? "DR" : "CR";
                nTotal += GetDouble(dt.Rows[i]["Balance"]);
                string dt1 = Convert.ToDateTime(dt.Rows[i]["Added"]).ToShortDateString();
                sCharity = dt.Rows[i]["Charity"].ToString();
                string row = "<tr><td align=right>" + dt1 + "<td align=right>" + DoFormat(GetDouble(dt.Rows[i]["Amount"])) + "<td align=right>" + dt.Rows[i]["Charity"].ToString()
                    + "<td align=right>" + dt.Rows[i]["Name"] + "<td align=right>" + dt.Rows[i]["ChildID"] + "<td align=right>" + DoFormat(GetDouble(dt.Rows[i]["Balance"])) 
                    + "<td align=right><small>" + dt.Rows[i]["Notes"].ToString() + "</small></tr>";
                
                // Add the totals
                if (dAmt > 0)
                { 
                    nDR += dAmt;
                }
                else
                {
                    nCR += dAmt;
                }
                
                if (sOldDate != dt1 && i > 1)
                {
                    HTML += "<tr><td colspan=10><hr></td></tr>";
                }

                HTML += row;
                sOldDate = dt1;
            }
            sql = "update sponsoredOrphan set balance = (Select top 1 Balance from OrphanExpense where SponsoredOrphan.childid=orphanexpense.childid order by added desc)\r\n"
                + "Select sum(balance) balance from sponsoredOrphan where charity='" + sCharity + "'";
            double nAmt = gData.GetScalarDouble(sql, "balance");


            HTML += "<tr><td>&nbsp;</td></tr>";
            HTML += "<tr><td>BALANCE:<td><td>" + DoFormat(nAmt) + "</tr>";
            HTML += "</body></html>";
            return HTML;
        }


        public static string Sign(string sPrivKey, string sMessage, bool fProd)
        {
            if (sPrivKey == null || sMessage == String.Empty || sMessage == null)
                return string.Empty;

            BitcoinSecret bsSec;
            if (fProd)
            {
                bsSec=Network.BiblepayMain.CreateBitcoinSecret(sPrivKey);
            }
            else
            {
                bsSec = Network.BiblepayTest.CreateBitcoinSecret(sPrivKey);
            }
            string sSig = bsSec.PrivateKey.SignMessage(sMessage);
            string sPK = bsSec.GetAddress().ToString();
            var fSuc = VerifySignature(sPK, sMessage, sSig);
            return sSig;
        }

        public static bool VerifySignature(string BBPAddress, string sMessage, string sSig)
        {
            if (BBPAddress == null || sSig == String.Empty)
                return false;
            try
            {
                // Determine the network:
                BitcoinPubKeyAddress bpk;
                if (BBPAddress.StartsWith("y"))
                {
                    bpk = new BitcoinPubKeyAddress(BBPAddress, Network.BiblepayTest);
                }
                else
                {
                    bpk = new BitcoinPubKeyAddress(BBPAddress, Network.BiblepayMain);
                }

                bool b1 = bpk.VerifyMessage(sMessage, sSig);
                return b1;
            }
            catch (Exception ex)
            {
                Log("VerifySignature::" + ex.Message + " for key " + BBPAddress);
                return false;
            }
        }


        public static void MsgBox(string sTitle, string sBody, System.Web.UI.Page p)
        {
            p.Session["MSGBOX_TITLE"] = sTitle;
            p.Session["MSGBOX_BODY"] = sBody;
            p.Response.Redirect("MessagePage.aspx");
        }

        public static double GetEstimatedHODL(bool fWithCompounding, double nBonusPercent)
        {
            string sql = "select sum(amount)/7/4500001*365 amt from sanctuaryPayment where added > getdate()-7";
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

        public static string GetTd2(DataRow dr, string colname, string sAnchor, string sPrefix = "", bool fBold = false)
        {
            string val = dr[colname].ToString();
            string sBold = fBold ? "<b>" : "";
            string td = "<td><nobr>" + sBold + sPrefix + sAnchor + val + "</a></td>";
            return td;
        }

        public static string GetTweetList(string sUserId, int days, bool fExcludeUser = false)
        {
            if (sUserId == "" || sUserId == null)
                sUserId = "BAF8C6FE-E1B2-42FB-0000-4A8289A90CA2";  // system user

            string sql = "Select * from Tweet left Join Users on Users.ID = Tweet.UserID left join TweetRead on TweetRead.ParentID=Tweet.ID and TweetRead.UserID = '"
                + sUserId + "' where tweet.added > getdate()-" + days.ToString() + " order by Tweet.Added desc";
            DataTable dt = gData.GetDataTable(sql);
            string html = "<table class=saved><tr><th>Read?<th width=20%>User</th><th width=20%>Added<th width=50%>Subject";
            if (fExcludeUser)
            {
                html = "<table class=saved><tr><th width=20%>User<th width=20%>Added<th width=50%>Subject";

            }
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                SavedObject s = RowToObject(dt.Rows[y]);
                string sUserName = NotNull(s.Props.UserName);
                if (sUserName == "")
                    sUserName = "N/A";
                string sAnchor = "<a href='https://foundation.biblepay.org/TweetView.aspx?id=" + s.Props.id.ToString() + "'>";
                string sReadTime = dt.Rows[y]["ReadTime"].ToNonNullString();
                bool fBold = sReadTime == "" ? false : true;
                string sCheckmark = sReadTime != "" ? "<i class='fa fa-check'></i>&nbsp;" : "<i class='fa fa-envelope'></i>&nbsp;";

                string div = "<tr>";
                if (!fExcludeUser)
                    div += GetTd2(dt.Rows[y], "ReadTime", sAnchor, sCheckmark, fBold);

                div += "<td>" + GetAvatar(s.Props.Picture) + "&nbsp;" + sUserName + "</td>";

                div += Common.GetTd2(dt.Rows[y], "Added", sAnchor, sCheckmark, fBold)  +  Common.GetTd2(dt.Rows[y], "subject", sAnchor, sCheckmark, fBold) + "</tr>";

                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }

        public static bool SendMassDailyTweetReport()
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

                // First we check to see if we have any tweets that have not been mass notified in 24 hours
                string sql = "Select count(*) ct from Tweet where added > getdate()-1";
                // if none we bail
                double dLast24 = gData.GetScalarDouble(sql, "ct");
                if (dLast24 < 1)
                    return true;
                // Now we need a manifest of tweets that have gone out in the last 30 days
                sql = "Select id from tweet where added > getdate()-2 order by added desc";
                string TweetID = gData.GetScalarString(sql, "id");

                sql = "Select * from Users where verification='Deliverable' and isnull(emailaddress,'') != '' and Unsubscribe is null and isnull(LastEmail,'1-1-1970') < getdate()-2 and Users.ID not in (Select userid from tweetread where parentid='" + TweetID + "')";
                //                sql = "Select * from Users where Users.id = 'BAF8C6FE-E1B2-42FB-9999-4A8289A90CA2'";

                DataTable dt1 = gData.GetDataTable(sql);
                MailAddress rTo = new MailAddress("rob@biblepay.org", "BiblePay Team");
                MailAddress r = new MailAddress("rob@saved.one", "BiblePay Team");

                MailMessage m = new MailMessage(r, rTo);

                m.Subject = "My Tweet Report";
                m.IsBodyHtml = true;

                string sData = GetTweetList("", 14, true);
                string sBody = "<html><br>Dear BiblePay Foundation User, <br><br>You have unread tweets that have been added in the last 14 days.  <br><br>This report will show you tweets that our users have paid for that they believe are extremely valuable.  <br><br>We will only send this report once per new tweet and only if you have not read the most recent tweet in 24 hours.<br><br>";
                sBody += sData;
                sBody += "<br><br>To unsubscribe from this transactional e-mail, please edit your account settings <a href=https://foundation.biblepay.org/AccountEdit>here</a> and click unsubscribe.<br><br>The BiblePay Tweet Team";

                m.Body = sBody;

                for (int i = 0; i < dt1.Rows.Count; i++)
                {

                    MailAddress t = new MailAddress(dt1.Rows[i]["EmailAddress"].ToString(), dt1.Rows[i]["UserName"].ToString());
                    m.Bcc.Add(t);
                    sql = "Update Users set LastEmail=getdate() where id = '" + dt1.Rows[i]["id"].ToString() + "'";
                    gData.Exec(sql);
                }

                try
                {
                     client.Send(m);
                     return true;
                }
                catch (Exception e)
                {
                     Console.WriteLine("Error in Send email: {0}", e.Message);
                     return false;
                }
                 
            }
            catch (Exception ex2)
            {
                Log("Cannot send Mass Mail: " + ex2.Message);
            }
            return false;
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
                        if ((fQuiet && iRowModulus % 100 == 0) || (!fQuiet))
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
                    var nbal=  _rpcclient.GetBalance();
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

        public static double GetTotalFrom(string userid, string table, string where)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null and " + where;
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }


        public static double GetTotalFrom(string userid, string table)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null";

            if (userid == "")
            {
                sql = "Select sum(amount) amount from " + table + " where amount is not null";
            }

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }

        public static string GetUserIDByAPIKey(string sAPIKEY)
        {
            string sql = "Select ID from Users where APIKEY=@apikey";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@apikey", sAPIKEY);
            string sID = gData.GetScalarString(command, "id");
            return sID;
        }
        public static bool ChargeTransaction(string sAPIKEY, double nBytes, string sTransType, string sFN)
        {
            string sUserId = GetUserIDByAPIKey(sAPIKEY);
            if (sUserId == "")
                return false;

            double dAmount = nBytes / 1000000;
            string sNotes = sTransType + " " + nBytes.ToString() + " " + Left(sFN, 100);
            AdjBalance(-1 * dAmount, sUserId, sNotes);
            return true;
        }


        public static double GetUserBalance(string id)
        {
            return GetTotalFrom(id, "Deposit");
        }

        public static double GetUserBalance(Page p)
        {
            return GetTotalFrom(gUser(p).UserId.ToString(), "Deposit");
        }

        public static string GetTotalSancInvestment(Page p)
        {
            return GetTotalFrom(gUser(p).UserId.ToString(), "SanctuaryInvestments").ToString();
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
                u.Admin = GetDouble(dt.Rows[0]["Admin"].ToString()) == 0 ? false : true;

                u.RandomXBBPAddress = dt.Rows[0]["RandomXBBPAddress"].ToNonNullString();
            }

        }

        public static double GetHPS(string bbp)
        {
            string sql = "Select sum(Hashrate) hr from leaderboard where bbpaddress = '" + bbp + "'";
            double dR = gData.GetScalarDouble(sql, "hr");
            return dR;
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
        public static string Left(string source, int iHowMuch)
        {
            if (source.Length < iHowMuch)
                return source;
            return source.Substring(0, iHowMuch);
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
        // coding horror
        ///
        /// CreateProcessWithLogonW is the unmangaged method used to launch a process under the context of
        /// alternative, user provided, credentials. It is called by the managed method CreateProcessAsUser
        /// defined earlier in this class. Further information is available on MSDN under
        /// CreateProcessWithLogonW (a href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/"http://msdn.microsoft.com/library/default.asp?url=/library/en-us//a
        /// dllproc/base/createprocesswithlogonw.asp).
        ///
        /// Whether to load a full user profile(param value = 1) or perform a
        /// network only (param value = 2) logon.
        /// The application to execute (populate either this parameter
        /// or the commandLine parameter).
        /// The command to execute.
        /// Flags that control how the process is created.
        ///
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool CreateProcessWithLogonW(string userName, string domain, string password, int logonFlags, string applicationPath, string commandLine,
        int creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInformation, out ProcessInformation processInformation);

        /*
        /// CloseHandle closes an open object handle.
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr handle);
        */


        /// 
        /// The StartupInformation structure is used to specify the window station, desktop, standard handles
        /// and appearance of the main window for the new process. Further information is available on MSDN
        /// under STARTUPINFO (a href="http://msdn.microsoft.com/library/en-us/dllproc/base/startupinfo_str.asp)."http://msdn.microsoft.com/library/en-us/dllproc/base/startupinfo_str.asp)./a
        /// 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct StartupInformation
        {
            internal int cb;
            internal string reserved;
            internal string desktop;
            internal string title;
            internal int x;
            internal int y;
            internal int xSize;
            internal int ySize;
            internal int xCountChars;
            internal int yCountChars;
            internal int fillAttribute;
            internal int flags;
            internal UInt16 showWindow;
            internal UInt16 reserved2;
            internal byte reserved3;
            internal IntPtr stdInput;
            internal IntPtr stdOutput;
            internal IntPtr stdError;
        }

        /// 
        /// The ProcessInformation structure contains information about the newly created process and its
        /// primary thread.
        /// 
        /// hProcess is a handle to the newly created process.
        /// hThread is a handle to the primary thread of the newly created process.
        [StructLayout(LayoutKind.Sequential)]
        struct ProcessInformation
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal int processId;
            internal int threadId;
        }



        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr process, ref UInt32 exitCode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);


        // end of coding horror
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        public static  WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,        String lpszDomain,        String lpszPassword,        int dwLogonType,        int dwLogonProvider,        ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,        int impersonationLevel,        ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        public static bool ImpersonateUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        public static void UndoImpersonation()
        {
            impersonationContext.Undo();
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
 