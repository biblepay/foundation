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
using System.Diagnostics;
using System.Net.Http;

namespace Saved
{
    public partial class Admin1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Debugger.IsAttached)
                CoerceUser(Session);


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
            if (!gUser(this).Admin)
            {
                Log("BRB");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }


            SendMassDailyTweetReport();

            return;
            // TODO - figure out how to update the bounce status on the server, and sync the users with new email addresses

            PoolCommon.SyncUsers();


            string sql = "Select top 5555 * from Leads where Verification is null and source like '%pobh%'";
            sql = "Select * from Users where verification is null and isnull(emailaddress,'') != ''";
            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string email = dt.Rows[i]["emailAddress"].ToString();
                string id = dt.Rows[i]["id"].ToString();
                string response = VerifyEmailAddress(email, id);
                sql = "Update users set verification='" + response + "' where id = '" + id + "'";
                gData.Exec(sql);

                string test = "";
            }
                
            string sTest = "";

        }

            

        protected void btnDSQL_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                Log("DSQL CBNA");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            // Replicate the Rapture table
            // Read in the rapture table, add a record, update the view, read the record
            string sql = "Select * from Rapture order by added";
            DataTable dt = gData.GetDataTable(sql);
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                DataRow dr1 = dt.Rows[iRow];
                UnchainedDatabase.InsertSQL("rapture", dr1["id"].ToString(), dr1);
            }

            string s2 = Uplink.Read("rapture/FFF9E193-32D7-4401-84F7-A9EFE470781C");

            DataRow dp1 = UnchainedDatabase.DeserializeDataRow(s2);


            // Verify one record
            string sData = Uplink.Read("test/10");
            DataRow dr199 = UnchainedDatabase.DeserializeDataRow(sData);

            Uplink.CreateView("test2");
            Uplink.CreateView("rapture");

            DataTable dt1234 = Uplink.GetDataTableByView("rapture");
            string test1 = "";

            DataRow[] foundRows = dt1234.Select("Notes like '%peters%'", "Notes desc");

            DataTable dt2 = foundRows.CopyToDataTable();


            // Store 1000 records; retrieve 1000 records; store the cache; retrieve the cache


            if (false)
            {
                DataTable dt1 = Uplink.GetFakeDataset();

                for (int i = 1; i < 999; i++)
                {
                    string data = "new-test " + i.ToString();
                    UnchainedDatabase.InsertSQL("test2", i.ToString(), dt1.Rows[i]);

                }
            }
            string mytest = "";

        }

        public static void SubmitPart(string sURL)
        {
            MyWebClient w = new MyWebClient();
            string sValue = "1234";
            w.Headers.Add("APIKey", sValue);
            w.Headers.Add("data", "12345");
            string sFile = "c:\\dominos.png";
            byte[] b = System.IO.File.ReadAllBytes(sFile);

            byte[] e = w.UploadData(sURL, b);

            string test = "";

        }

        protected void btnCopyToAWS_Click(object sender, EventArgs e)
        {

            if (!gUser(this).Admin)
            {
                Log("Clicked by non aws user");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            // Replicate the Rapture Table
            UnchainedDatabase.ReplicateTable("rapture");

            //string sDir = FileSplitter.SplitFile("c:\\dominos.png");
            //FileSplitter.ResurrectFile(sDir, "d1.png");
            // Test the ls performance of 99 parts

            DataTable dt1 = Uplink.GetFakeDataset();
            // Filter (as in where clause)

            DataRow[] foundRows = dt1.Select("Col1 like '%4%' or Col2 like '%4%'", "Col1 desc");
            DataTable dt2 = foundRows.CopyToDataTable();
            // Write data row to file
            UnchainedDatabase.InsertSQL("tbltest", "1", foundRows[17]);

            return;

        }


        protected void btnConvert_Click(object sender, EventArgs e)
        {
            string test =             Saved.Code.PoolCommon.GetChartOfSancs();


            // Upgrade san.biblepay images to uplink images
            string sql = "Select id,url from  SponsoredOrphan where url like '%san.biblepay%' ";
            DataTable dt = gData.GetDataTable(sql);
            for (int i =0; i < dt.Rows.Count; i++)
            {
                string bio = dt.Rows[i]["url"].ToString();
                string sURL = Uplink.Replicate(bio);
                sql = "Update SponsoredOrphan set url='" + sURL + "' where id = '" + dt.Rows[i]["id"].ToString() + "'";
                gData.Exec(sql);
            }
            lblStatus.Text = "Updated " + DateTime.Now.ToString();

        }


        protected void btnSave_Click(object sender, EventArgs e)
        {

            if (!gUser(this).Admin)
            {
                Log("Clicked save from non admin");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

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
            if (!gUser(this).Admin)
            {
                Log("Clicked from non pdf");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

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