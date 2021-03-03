using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web.UI;
using static Saved.Code.Common;
using static Saved.Code.Fastly;

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



        protected void btnRemoveSancs_Click(object sender, EventArgs e)
        {
        }

        protected void btnCampaign_Click(object sender, EventArgs e)
        {
            // Reserved Example : string sSig = SignChrome("bbp_privkey", "msg_sig", true);
            return;
        }
        
        protected void btnRemoveBounce_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            UICommon.SendMassDailyTweetReport();
            return;
        }
            

        protected void btnDSQL_Click(object sender, EventArgs e)
        {
            if (!gUser(this).Admin)
            {
                Log("DSQL CBNA");
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }

            // Read in the rapture table, add a record, update the view, read the record
            string sql = "Select * from Rapture order by added";
            DataTable dt = gData.GetDataTable(sql);
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                DataRow dr1 = dt.Rows[iRow];
                UnchainedDatabase.InsertSQL("rapture", dr1["id"].ToString(), dr1);
            }
        }

        public static void SubmitPart(string sURL)
        {
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
            DataTable dt1 = Uplink.GetFakeDataset();
            // Filter (as in where clause)
            DataRow[] foundRows = dt1.Select("Col1 like '%4%' or Col2 like '%4%'", "Col1 desc");
            DataTable dt2 = foundRows.CopyToDataTable();
            UnchainedDatabase.InsertSQL("tbltest", "1", foundRows[17]);
            return;
        }


        protected void btnConvert_Click(object sender, EventArgs e)
        {
            string test = Saved.Code.PoolCommon.GetChartOfSancs();
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
            string sql = "select id,added as a1, FORMAT (added, 'MMMM yyyy') as Added,'DR' as Type,Amount,Charity, '' as Notes from expense "
                + " union all  select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type,Amount, Charity, Notes from Revenue  order by a1 ";
            string html = UICommon.GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            ByteArrayToFile("c:\\test_pdf.pdf", result);
        }
    }
}