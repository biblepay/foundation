using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class UnchainedUpload : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

           if (!gUser(this).Admin)
           {
                    MsgBox("Restricted", "Sorry this page is for admins only.", this);
                    return;
           }

            if (!IsPostBack)
                FileUpload1.Attributes["onchange"] = "UploadFile(this)";
        }


        bool IsAllowableExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext.Length < 1) return false;
            ext = ext.Substring(1, ext.Length - 1);
            string allowed = "jpg;jpeg;gif;png;pdf;txt;csv";
            string[] vallowed = allowed.Split(";");
            for (int i = 0; i < vallowed.Length; i++)
            {
                if (vallowed[i] == ext)
                    return true;
            }
            return false;
        }
        protected void btnUnchainedSave_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            if (FileUpload1.HasFile)
            {
                    sb.AppendFormat(" Uploading file: {0}", FileUpload1.FileName);

                    string path = GetFolderUnchained("Uploads");
                    string extension = Path.GetExtension(FileUpload1.FileName);
                    string newName = Guid.NewGuid().ToString() + extension;
                    string fullpath = Path.Combine(path, newName);
                    if (IsAllowableExtension(fullpath))
                    {
                        FileUpload1.SaveAs(fullpath);

                        //Showing the file information
                        sb.AppendFormat("<br/> Save As: {0}", FileUpload1.PostedFile.FileName);
                        sb.AppendFormat("<br/> File type: {0}", FileUpload1.PostedFile.ContentType);
                        sb.AppendFormat("<br/> File length: {0}", FileUpload1.PostedFile.ContentLength);
                        sb.AppendFormat("<br/> File name: {0}", FileUpload1.PostedFile.FileName);

                        // Make an Unchained Object
                        UnchainedTransaction u = new UnchainedTransaction();
                        u.SenderBBPAddress = "cqtp";
                        u.RecipientBBPAddress = "toaddress";
                        u.nTimestamp = UnixTimeStamp(DateTime.Now);

                        Unchained.SubmitUnchainedTransaction(u);
                        // Submit to S3 and Submit to JStor
                        string sURL = Uplink.Store(newName, "", "", fullpath);

                    if (sURL == "")
                    {
                        MsgBox("Object Storage Failed", "The server could not process the request.", this);
                        return;
                    }
                        string narr = "Thank you for using BiblePay Object Storage.  <br><br><a href=" + sURL + ">Your URL is<br>" + sURL + "<br></a>";
                        //Response.Clear();
                        MsgBox("Object Storage Successful", narr, this);
                        return;

                    }
                    else
                    {
                        MsgBox("Object Storage Failed", "The file extension provided is not allowed.", this);
                        return;
                    }
            }
            else
            {
                lblmessage.Text = sb.ToString();
            }
        }
    }
}