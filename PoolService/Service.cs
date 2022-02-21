using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using OpenPop;
using OpenPop.Mime;
using OpenPop.Mime.Header;
using static Saved.Code.StringExtension;

namespace PoolService
{
    class Service
    {
        // The Service loop performs functions that are not possible to run from the ASP.NET (IIS) Application
        // This includes:  Converting youtube videos to mp4
        // Not only are these conversions long running, but more specifically need to be run as Administrator
        // The IIS App Pool User does not have enough privilege to run external tools with arguments, such as youtube-dl.exe, etc.
        private static string sUploads = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\";
        public static void CropImage(string sSourceFile, string sOutFile)
        {
            Bitmap src = Image.FromFile(sSourceFile) as Bitmap;
            Rectangle cropRect = new Rectangle(250, 0, src.Width - 250, src.Height);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
                target.Save(sOutFile, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        public static void MakeRapColl(string sCategory)
        {
            int z = 0;
            try
            {
                string sql = "Select top 50 * from Rapture where isnull(url,'') <> '' and category = '" + sCategory + "' order by Title";
                SqlCommand command = new SqlCommand(sql);
                
                DataTable dt = Saved.Code.Common.gData.GetDataTable(command);
                int x = 0;
                int y = 0;
                int cols = 3;
                int rows = 3;
                Bitmap target = new Bitmap(1024, 768);
                //Graphics g = Graphics.FromImage(target);
                if (dt.Rows.Count < cols*rows)
                {
                    cols = dt.Rows.Count / 3;
                    rows = dt.Rows.Count / 3;
                    if (cols < 1) cols = 1;
                    if (rows < 1) rows = 1;
                }
                using (Graphics g = Graphics.FromImage(target))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string sTitle = Saved.Code.Common.Left(dt.Rows[i]["title"].ToString(), 40);
                        string sNotes = Saved.Code.Common.Left(dt.Rows[i]["notes"].ToString(), 200);
                        string sURL = dt.Rows[i]["thumbnail"].ToString();
                        string id = dt.Rows[i]["id"].ToString();

                        sTitle = sTitle.Replace("\"", "`");
                        sNotes = sNotes.Replace("\"", "`");
                        WebClient webClient = new WebClient();
                        string localFileName = sUploads + "Thumbnails\\" + id + ".jpg";
                        if (!System.IO.File.Exists(localFileName))
                        {
                            sql = "Update Rapture set Thumbnail=null where id = '" + id + "'\r\nDelete from rapturecategories where category = '" + sCategory + "'";
                            Saved.Code.Common.gData.Exec(sql);
                            break;
                        }
                        if (false && !System.IO.File.Exists(localFileName))
                        {
                            try
                            {
                                //if (System.IO.File.Exists(localFileName))
                                //  System.IO.File.Delete(localFileName);
                                webClient.DownloadFile(sURL, localFileName);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        Bitmap src = Image.FromFile(localFileName) as Bitmap;
                        Bitmap resized = new Bitmap(src, new Size(target.Width / cols, target.Height / rows));

                        Rectangle cropRect = new Rectangle(0, 0, resized.Width, resized.Height);
                        g.DrawImage(resized, new Rectangle(x, y, resized.Width, resized.Height),
                                             cropRect, GraphicsUnit.Pixel);

                        // Text

                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        RectangleF rectf = new RectangleF(x+10,y+10,resized.Width,resized.Height);
                        g.DrawString(sTitle, new Font("Arial", 8), Brushes.Gold, rectf);

                        g.Flush();

                        // End of Text

                        x += target.Width / cols;
                        if (x > target.Width)
                        {
                            y += target.Height / rows;
                            x = 0;
                        }
                        z++;
                        if (z > cols * rows)
                            break;
                    }
                }
                string sOutFile = sUploads + "Thumbnails\\" + sCategory + ".jpg";
                target.Save(sOutFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                string sOutURL = "https://foundation.biblepay.org/Uploads/Thumbnails/" + sCategory + ".jpg";
                sOutURL = sOutURL.Replace(" ", "%20");
                sql = "Insert into RaptureCategories (id,category,url) values (newid(), '" + sCategory + "','" + sOutURL + "')";
                Saved.Code.Common.gData.Exec(sql);

            }
            catch (Exception ex)
            {
                Saved.Code.Common.Log("GMLX " + ex.Message);
            }

}



        public static void AddRaptureDrillImages()
        {
            try
            {
                string sql = "Select distinct category from rapture where category not in (Select category from RaptureCategories where url is not null) order by Category";
                SqlCommand command = new SqlCommand(sql);

                DataTable dt = Saved.Code.Common.gData.GetDataTable(command);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    try
                    {
                        MakeRapColl(dt.Rows[i]["category"].ToString());
                    }
                    catch(Exception ex1)
                    {
                        Saved.Code.Common.Log("AddRDI" + ex1.Message);
                    }
                    /*
                    if (false)
                    {

                        string sSourceURL = "https://foundation.biblepay.org/MediaBlack?category=" + dt.Rows[i]["category"].ToString() + "&limit=3";
                        for (int j = 0; j < 7; j++)
                        {
                            try
                            {
                                var screenshotJob = ScreenshotJobBuilder.Create(sSourceURL)
                                  .SetBrowserSize(1100, 700)
                                  .SetCaptureZone(CaptureZone.FullPage) // Set what should be captured
                                  .SetTrigger(new WindowLoadTrigger()); // Set when the picture is taken
                                screenshotJob.SetTimeout(new TimeSpan(0, 0, 60));
                                
                                string sTempFile = sUploads + "1.bmp";
                                string sOutFile = sUploads + dt.Rows[i]["category"].ToString() + ".jpg";
                                System.IO.File.WriteAllBytes(sTempFile, screenshotJob.Freeze());
                                CropImage(sTempFile, sOutFile);
                                // ins record into rapturectegories
                                string sOutURL = "https://foundation.biblepay.org/Uploads/" + dt.Rows[i]["category"].ToString() + ".jpg";
                                sOutURL = sOutURL.Replace(" ", "%20");
                                sql = "Insert into RaptureCategories (id,category,url) values (newid(), '" + dt.Rows[i]["category"].ToString() + "','" + sOutURL + "')";
                                Saved.Code.Common.gData.Exec(sql);
                                break;
                            }
                            catch (Exception ex)
                            {
                                
                            }
                    }
            */

                }
            }
            catch (Exception ex)
            {
                Saved.Code.Common.Log("Unable to create rapture thumbnails " + ex.Message);
            }
        }


        private static void CleanUpInbox()
        {
            OpenPop.Pop3.Pop3Client p3 = new OpenPop.Pop3.Pop3Client();
            int iPort = 110;
            iPort = 995;
            bool bSSL = false;
            bSSL = true;
            string sPopHost = Saved.Code.Common.GetBMSConfigurationKeyValue("outlookhost");
            string sPopUser = Saved.Code.Common.GetBMSConfigurationKeyValue("smtppopuser");
            string sPopPass = Saved.Code.Common.GetBMSConfigurationKeyValue("smtppoppassword");
            p3.Connect(sPopHost, iPort, bSSL, 7000, 7000, null);
            p3.Authenticate(sPopUser, sPopPass);
            int iCount = p3.GetMessageCount();
            int i = 0;

                for (i = iCount; i > 0;i--)
                {

                    try
                    {
                        MessageHeader h = p3.GetMessageHeaders(i);
                        Message m = p3.GetMessage(i);
                        OpenPop.Mime.MessagePart plainText = m.FindFirstPlainTextVersion();
                        OpenPop.Mime.MessagePart htmlPart = m.FindFirstHtmlVersion();
                        string body = "";
                        string myto = m.Headers.Subject;
                        string mysentto = "";

                    if (myto != null)
                    {
                        string[] vTo = myto.Split(new string[] { "-" }, StringSplitOptions.None);
                        if (vTo.Length > 1)
                            mysentto = vTo[1].Trim();
                    }


                    if (plainText != null)
                        {
                            body += plainText.GetBodyAsText();
                        }
                        if (htmlPart != null)
                        {
                            body += htmlPart.GetBodyAsText();
                        }
                        bool fTotaled = false;
                        if (body.Contains("be delivered to one"))
                        {
                            fTotaled = true;

                        }
                        else if (body.Contains("hop count exceeded"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("A communication failure occurred"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("The email address you entered couldn't be found"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("The domain you attempted to contact"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("I'm afraid I wasn't able to deliver the following message"))
                    {
                        fTotaled = true;
                    }
                        else if(body.Contains("cannot be delivered"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("I was unable to deliver your message"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("Your message wasn't delivered"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("rejected your message to the following"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("Delivery to the following recipient failed permanently"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("This is a delivery failure notification message "))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("Delivery has failed to these recipients or groups"))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("There was a temporary problem delivering your message "))
                    {
                        fTotaled = true;
                    }
                        else if (body.Contains("The following addresses had permanent fatal errors"))
                    {
                        fTotaled = true;
                    }

                    else      if (body.Contains("the receiving email server outside Office 365 reported ")|| body.Contains("couldn't be delivered"))
                    {
                        fTotaled = true;
                    }

                    if (fTotaled)
                    {
                        string sql = "update Leads set Advertised=getdate() where email='" + mysentto + "'";
                        Saved.Code.Common.gData.Exec(sql);
                        p3.DeleteMessage(i);
                    }

                    }
                    catch (Exception)
                    {

                    }
                    
                }
            
            p3.Disconnect();
        }

        private static byte[] BytesFromString(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static int GetResponseCode(string ResponseString)
        {
            return int.Parse(ResponseString.Substring(0, 3));
        }
        
        public static void SendMarketingEmail2()
        {
            // Ensure we comply with this:  https://www.ftc.gov/tips-advice/business-center/guidance/can-spam-act-compliance-guide-business
            try
            {
                string sql = "Select top 10 * from Leads where Advertised is null and verification='deliverable'";
                SqlCommand command = new SqlCommand(sql);

                DataTable dt = Saved.Code.Common.gData.GetDataTable(command);
                int nMax = 5;
                int nSent = 0;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string email = dt.Rows[i]["email"].ToString();
                        string username = dt.Rows[i]["name"].ToString();
                        string sPath = "c:\\biblepay\\bbpcampaign1.html";
                        string sID = dt.Rows[i]["id"].ToString();
                        string body = System.IO.File.ReadAllText(sPath);
                        body = body.Replace("[name]", username);
                        string sLandURL = "https://foundation.biblepay.org/LandingPage?id=" + sID;
                        body = body.Replace("[reward]", "<a href='" + sLandURL + "'>Navigate here to find out how to unlock your wallet balance.</a>");
                        body = body.Replace("[Unsubscribe]", "<a href='https://foundation.biblepay.org/LandingPage?id=" + sID + "&action=unsubscribe'>Unsubscribe</a>");
                        body = body.Replace("[mine]", "<a href='http://wiki.biblepay.org/RandomX_Setup'>Mining Setup</a>");
                        body = body.Replace("[land]", sLandURL);
                        MimeKit.MailboxAddress r = new MailboxAddress(username, email);
                        string sn = sID.Substring(0, 4);
                        string sSubject = "You have received between 50,000 and 1,000,000 BBP - " + email + " - " + System.DateTime.Now.ToShortDateString();
                        bool fSent = Saved.Code.Common.SendMailSSL(body, r, sSubject);
                        if (fSent)
                        {
                            sql = "Update Leads set Advertised = getdate() where id = '" + sID + "'";
                            Saved.Code.Common.gData.Exec(sql);
                        }
                        nSent++;
                        if (nSent >= nMax)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Saved.Code.Common.Log("Send Marketing Email Issues: " + ex.Message);
            }
        }

        private static bool CheckList(string semail)
        {
            string slist = "gmail;proton;yahoo;aol;icloud;msn;live;zoho;gmx;hotmail;comcast;wanadoo;orange;rediffmail;netzero;free;gmx;web;yandex;ymail;outlook;libero;cox;sbcglobal;verizon;googlemail;rocketmail;att;rambler;tiscali";
            string[] vData = slist.Split(new string[] { ";" }, StringSplitOptions.None);
            for (int i = 0; i < vData.Length; i++)
            {
                if (semail.Contains(vData[i]))
                    return true;
            }
            return false;

        }

        private static void MarketingCampaign()
        {
            // Insert 100 more addresses
            string sPath = "c:\\BiblePay\\church_list.csv";
            System.IO.StreamReader file = new System.IO.StreamReader(sPath);
            // Seek to random position
            Random r = new Random();
            int rInt = r.Next(0, 818000);
            for (int i = 0; i < rInt; i++)
            {
                string line = file.ReadLine();
            }

            int iFound = 0;
            for (int i = 1; i < 30000; i++)
            {
                string line = file.ReadLine();
                line = Saved.Code.Common.FleeceCommas(line);
                //Company,Address,City,State,Zip,County,Phone,Website,Contact,Title,Direct Phone,Email,Sales,Employees,SIC Code,Industry
                line = line.Replace("'", "`");

                string[] vData = line.Split(new string[] { "," }, StringSplitOptions.None);
                string email = vData[11];
                if (CheckList(email))
                {
                    string sName = vData[8];
                    string sCompany = vData[0];
                    string sTitle = vData[9];
                    string sValid = Saved.Code.Common.CheckEmail(email, sCompany, sTitle, sName);
                    Debug.WriteLine(i.ToString() + ", " + email + " " + sValid);
                    iFound++;
                    if (iFound > 10)
                        break;
                }
            }
            file.Close();
        }

        public static void ServiceLoop()
        {
            int i = 0;
            while (true)
            {
                i++;
                if (i % 5 == 0)
                {
                    //Saved.Code.WebServices.AddThumbnails();
                    Console.WriteLine("Done with Thumbs");
                }

                System.Threading.Thread.Sleep(10000);
                Console.WriteLine("Working on videos");
                Saved.Code.WebServices.ConvertVideos();
                Console.WriteLine("Done with videos");
            }
        }
    }
}
