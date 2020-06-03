using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Saved
{
    public partial class Admin1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }
        }

        private string FleeceCommas(string data)
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
                    ch = "";

                if (ch != "" && ch!= "\"")
                {
                    data1 += ch;
                }
            }
            return data1;
        }
        protected void btnCampaign_Click(object sender, EventArgs e)
        {
            // Read the 800,000 users for our first Church Campaign
            string sFile = "c:\\bbp.csv";
            System.IO.StreamReader file = new System.IO.StreamReader(sFile);
            string data = file.ReadToEnd();
            string[] vBBP = data.Split("\r\n");
            for (int i = 1; i < vBBP.Length; i++)
            {
                string data1 = FleeceCommas(vBBP[i]);
                string[] vRow = data1.Split(",");
                if (vRow.Length > 10)
                {
                    string sCompany = vRow[0];
                    string sEmail = vRow[11];
                    string Title = vRow[9];
                    string sName = vRow[8];
                    if (!sEmail.Contains("@"))
                    {
                        string mytest = "";
                    }
                    else
                    {
                        string sql = "Insert into Leads (id, company, email, title, name, added) values (newid(), '" + sCompany + "','" + sEmail + "','" + Title + "','" + sName + "',getdate())";
                        gData.Exec(sql);
                    }
                    

                }
            }
            string test = "";



        }
        
        protected void btnRemoveBounce_Click(object sender, EventArgs e)
        {
            string sql = "Select top 5555 * from Leads where Verification is null and source like '%pobh%'";
            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string email = dt.Rows[i]["email"].ToString();
                string id = dt.Rows[i]["id"].ToString();
                string response = VerifyEmailAddress(email, id);
                string test = "";
            }
                
            string sTest = "";

        }
        protected void btnDSQL_Click(object sender, EventArgs e)
        {
            DataTable dt1 = UnchainedDatabase.GetDataTable("test");

            for (int i = 1; i < 99; i++)
            {
                string data = "test " + i.ToString();
                UnchainedDatabase.Insert("test", i.ToString(), data);

            }
            string mytest = "";

        }

        protected void btnCopyToAWS_Click(object sender, EventArgs e)
        {
        
            // Convert unconverted RequestVideo to Rapture videos
            string sql = "Select * from Rapture where url is not null and FileName like '%mp4%' and url2 is null";

            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string url = dt.Rows[i]["url"].ToString();
                string fnsource = dt.Rows[i]["FileName"].ToString();
                string path = "s:\\Rapture\\" + fnsource;
                string notes = dt.Rows[i]["Notes"].ToString();
                string sId = dt.Rows[i]["id"].ToString();
                if (System.IO.File.Exists(path))
                {

                    Task<string> myTask = Uplink.Store2(fnsource, "notes", notes, path);
                    sql = "Update Rapture set Url2='" + myTask.Result + "' where id = '" + sId + "'";
                    gData.Exec(sql);

                }
                else
                {
                    string sOneTest = "";

                }
            }

        }


        protected void btnConvert_Click(object sender, EventArgs e)
        {
            // Convert unconverted RequestVideo to Rapture videos
            string sql = "Select * from RequestVideo where status is null";

            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string url = dt.Rows[i]["url"].ToString();
                // Convert this particular youtube URL into a rapture video
                // Then store in the rapture table with an uncategorized category
                GetVideo(url);
                string sPath = GetPathFromTube(url);
                // Convert the path to hash
                string sNewFileName = "700" + sPath.GetHashCode().ToString() + ".mp4";

                System.IO.FileInfo fi = new FileInfo(sPath);
                string sHeading = Chop(fi.Name, 16);

                string notes = sHeading + "\r\n\r\n"+ Chop(GetNotes(sPath), 4000);
                string sNewPath = "s:\\Rapture\\" + sNewFileName;

                fi.CopyTo(sNewPath,true);

                sql = "Insert into Rapture (id,added,Notes,URL,timestamp,FileName,Category) values (newid(), getdate(), @notes, @url, 0, @filename, @category)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@notes", notes);
                string sNewUrl = "https://video.biblepay.org/" + sNewFileName;
                command.Parameters.AddWithValue("@url", sNewUrl);
                command.Parameters.AddWithValue("@filename", sNewFileName);
                command.Parameters.AddWithValue("@category", "Miscellaneous");

                gData.ExecCmd(command);

                sql = "Update RequestVideo set Status='FILLED' where id = '" + dt.Rows[i]["id"].ToString() + "'";

                gData.Exec(sql);

            }

            lblStatus.Text = "Updated " + DateTime.Now.ToString();

        }



        protected void btnSave_Click(object sender, EventArgs e)
        {

            StringBuilder sb = new StringBuilder();

            if (FileUpload1.HasFile)
            {
                try
                {
                    sb.AppendFormat(" Uploading file: {0}", FileUpload1.FileName);

                    //saving the file
                    FileUpload1.SaveAs("c:\\" + FileUpload1.FileName);

                    //Showing the file information
                    sb.AppendFormat("<br/> Save As: {0}", FileUpload1.PostedFile.FileName);
                    sb.AppendFormat("<br/> File type: {0}", FileUpload1.PostedFile.ContentType);
                    sb.AppendFormat("<br/> File length: {0}", FileUpload1.PostedFile.ContentLength);
                    sb.AppendFormat("<br/> File name: {0}", FileUpload1.PostedFile.FileName);

                }
                catch (Exception ex)
                {
                    sb.Append("<br/> Error <br/>");
                    sb.AppendFormat("Unable to save file <br/> {0}", ex.Message);
                }
            }
            else
            {
                lblmessage.Text = sb.ToString();
            }
        }
        protected void btnPDF_Click(object sender, EventArgs e)
        {

            // Make an Unchained Object
            UnchainedTransaction u = new UnchainedTransaction();
            u.SenderBBPAddress = "cqtp";
            u.RecipientBBPAddress = "toaddress";
            u.nTimestamp = UnixTimeStamp(DateTime.Now);

            Unchained.SubmitUnchainedTransaction(u);

            // TestNet signing (Use these TESTNET keys for now):
            string tprivk = "cQThZchr8gjxJ1JSTEMydDuJL9fHA4NxjYwZ6XnfRabYDgFZVNFB";
            string tpubk = "ycd5NdyC8KXT8kLuzwPt744WTKurJkVZyY";

            string sig = Sign(tprivk, "mymessage", false);
            bool fsig = VerifySignature(tpubk, "mymessage", sig);

            string sql = "select id,added as a1, FORMAT (added, 'MMMM yyyy') as Added,'DR' as Type,Amount,Charity, '' as Notes from expense "
                + " union all  select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type,Amount, Charity, Notes from Revenue  order by a1 ";
            string html = GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            ByteArrayToFile("c:\\test_pdf.pdf", result);
        }
    }
}