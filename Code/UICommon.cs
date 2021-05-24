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
using static Saved.Code.Common;

namespace Saved.Code
{



    public static class UICommon
    {

        public static string GetHPSLabel(double dHR)
        {
            string KH = Math.Round(dHR / 1000, 2).ToString() + " KH/S";
            string MH = Math.Round(dHR / 1000000, 2).ToString() + " MH/S";
            string H = Math.Round(dHR, 2).ToString() + " H/S";
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
             + "     $('#bbpdd" + item.ToString() + "').css('display',disp);\" >"
             + "	<i class='fa " + sIcon + "'></i>&nbsp;<span>" + MenuName + "</span>"
             + "	<span class='pull-right-container'><i class='fa fa-angle-left pull-right'></i></span></a>"
             + "	<ul class='treeview-menu' id='bbpdd" + item.ToString() + "'><script>" + js3 + "$('#bbpdd" + item.ToString() + "').css('display',disp);</script>";


            for (int i = 0; i < vLinkNames.Length; i++)
            {
                menu += "<li><a href='" + vURLs[i] + "'><span style='overflow:visibile;'>" + vLinkNames[i] + "</span></a></li>";
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

        public static string GetTableBeginning(string sTableName)
        {
            string css = "<style> html {    font-size: 1em;    color: black;    font-family: verdana }  .r1 { font-family: verdana; font-size: 10; }</style>";
            string logo = "https://www.biblepay.org/wp-content/uploads/2018/04/Biblepay70x282_96px_color_trans_bkgnd.png";
            string sLogoInsert = "<img width=300 height=100 src='" + logo + "'>";
            string HTML = "<HTML>" + css + "<BODY><div><div style='margin-left:12px'><TABLE class=r1><TR><TD width=70%>" + sLogoInsert
                + "<td width=25% align=center>" + sTableName + "</td><td width=5%>" + DateTime.Now.ToShortDateString() + "</td></tr>";

            HTML += "<TR><TD><td></tr>" + "<TR><TD><td></tr>" + "<TR><TD><td></tr>";
            HTML += "</table>";
            return HTML;
        }
        public static string GetTableHTML(string sql)
        {
            string HTML = GetTableBeginning("Accountability");
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
            DataTable dt;
            try
            {
                dt = gData.GetDataTable(command, false, true);
            }
            catch (Exception ex)
            {
                return "";
            }
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
                + "Select sum(Amount) balance from OrphanExpense where charity='" + sCharity + "' and childid in (select childid from SponsoredOrphan)";
            double nAmt = gData.GetScalarDouble(sql, "balance");


            HTML += "<tr><td>&nbsp;</td></tr>";
            HTML += "<tr><td>BALANCE:<td><td>" + DoFormat(nAmt) + "</tr>";
            HTML += "</body></html>";
            return HTML;
        }

        public static string GetFooter(Page p)
        {
            string sOverridden = Common.GetBMSConfigurationKeyValue("footer");
            if (sOverridden.Length > 0)
                return sOverridden;

            string sFooter = DateTime.Now.Year.ToString() + " - " + Common.GetLongSiteName(p);
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

                div += "<td>" + DataOps.GetAvatar(s.Props.Picture) + "&nbsp;" + sUserName + "</td>";

                div += UICommon.GetTd2(dt.Rows[y], "Added", sAnchor, sCheckmark, fBold) + UICommon.GetTd2(dt.Rows[y], "subject", sAnchor, sCheckmark, fBold) + "</tr>";

                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }

        public static bool SendMassDailyTweetReport()
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
                // First we check to see if we have any tweets that have not been mass notified in 24 hours
                string sql = "Select count(*) ct from Tweet where added > getdate()-1";
                // if none we bail
                double dLast24 = gData.GetScalarDouble(sql, "ct");
                if (dLast24 < 1)
                    return true;
                // Now we need a manifest of tweets that have gone out in the last 30 days
                sql = "Select id from tweet where added > getdate()-2 order by added desc";
                string TweetID = gData.GetScalarString(sql, "id");
                sql = "Select top 500 * from Users where verification='Deliverable' and isnull(emailaddress,'') != '' and Unsubscribe is null and isnull(LastEmail,'1-1-1970') < getdate()-2 and Users.ID not in (Select userid from tweetread where parentid='" + TweetID + "')";
                DataTable dt1 = gData.GetDataTable(sql);
                MailAddress rTo = new MailAddress("rob@biblepay.org", "BiblePay Team");
                MailAddress r = new MailAddress("rob@saved.one", "BiblePay Team");
                MailMessage m = new MailMessage(r, rTo);
                m.Subject = "My Tweet Report";
                m.IsBodyHtml = true;
                string sData = GetTweetList("", 14, true);
                string sBody = "<html><br>Dear BiblePay Foundation User, <br><br>You have unread tweets that have been added in the last 14 days.  <br><br>This report will show you tweets that our users have paid for that they believe are extremely valuable.  <br><br>We will only send this report once per new tweet and only if you have not read the most recent tweet in 24 hours.<br><br>";
                sBody += sData;
                // Append the single tweet
                string sTweet = DataOps.GetSingleTweet(TweetID);
                sBody += "<br><br><h3>Our Last Tweet</h3><br><br>" + sTweet;
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
                if ((i - 1) == iActivePage)
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
            html += AddMenuOption("Orphans", "Report?name=orphantx", 
                "My Orphan Payments", "fa-child");
            html += AddMenuOption("Reports", "Accountability.aspx;Viewer.aspx?target=collage;Partners.aspx", "Accountability;Orphan Collage;Partners", "fa-table");
            html += AddMenuOption("Pool", "Leaderboard.aspx;GettingStarted.aspx;PoolAbout.aspx;BlockHistory.aspx;Viewer.aspx?target="
                + System.Web.HttpUtility.UrlEncode("https://minexmr.com/dashboard") + ";MiningCalculator.aspx",
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

                string sUserPic = DataOps.GetAvatar(s.Props.Picture);

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
        public static string GetHeaderBanner(Page p)
        {
            return GetBMSConfigurationKeyValue("sitename");
        }

        public static string GetTd2(DataRow dr, string colname, string sAnchor, string sPrefix = "", bool fBold = false)
        {
            string val = dr[colname].ToString();
            string sBold = fBold ? "<b>" : "";
            string td = "<td><nobr>" + sBold + sPrefix + sAnchor + val + "</a></td>";
            return td;
        }

        public static void PersistBioImg(string URL, string orphanid)
        {

            string sql = "Delete from BIO where orphanid = @orphanid\r\nInsert into BIO values (newid(), @orphanid, @url)";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@orphanid", orphanid);
            command.Parameters.AddWithValue("@url", URL);
            gData.ExecCmd(command);
        }
        public static string GetSiteName(Page p)
        {
            return GetHeaderBanner(p);
        }
    }
}