using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (Debugger.IsAttached)
                CoerceUser(Session);

            // string sAPI = Request.Headers["apikey"] ?? "";
            // string sID = GetUserIDByAPIKey(sAPI);
            // double nBalance = GetUserBalance(sID);
            // ToDo: Implement API

            string sOriginalName = Request.Headers["OriginalName"] ?? "";
            int nDuration = (int)GetDouble(Request.Headers["Duration"] ?? "");
            int nDensityLevel = (int)GetDouble(Request.Headers["Density"] ?? "");
            string sBlockHash = Request.Headers["BlockHash"] ?? "";
            int nHeight = (int)GetDouble(Request.Headers["BlockHeight"] ?? "");

            if (sOriginalName != "" && sBlockHash != "")
            {
                int iPartNo = (int)GetDouble(Request.Headers["PartNumber"] ?? "");
                int nTotalParts = (int)GetDouble(Request.Headers["TotalParts"] ?? "");
                string sLocalFileName = iPartNo.ToString() + ".dat";
                string sWebPath = Request.Headers["WebPath"] ?? "";
                string sCPK = Request.Headers["CPK"] ?? "";
                string sNetwork = Request.Headers["NetworkID"] ?? "";
                string sTempArea = "SVR" + sOriginalName.GetHashCode().ToString();
                string sCompleteTempPath = Path.Combine(Common.GetFolderUnchained("Temp"), sTempArea);
                string sCompleteWritePath = Path.Combine(sCompleteTempPath, sLocalFileName);
                double nFee = GetDouble(Request.Headers["Fee"] ?? "");
                string sTXID = Request.Headers["TXID"] ?? "";
                string sXML = "";
                if (!Directory.Exists(sCompleteTempPath))
                    Directory.CreateDirectory(sCompleteTempPath);
                
                using (Stream output = File.OpenWrite(sCompleteWritePath))
                using (Stream input = Request.InputStream)
                {
                     input.CopyTo(output);
                }

                bool f64 = true;
                if (f64)
                {
                    string data = System.IO.File.ReadAllText(sCompleteWritePath);
                    byte[] b64 = System.Convert.FromBase64String(data);
                    System.IO.File.WriteAllBytes(sCompleteWritePath, b64);
                }

                if (nTotalParts == iPartNo)
                {
                    string sResurrectionPath = Path.Combine(Common.GetFolderUnchained("Temp"), Guid.NewGuid().ToString() + ".dat");
                    FileSplitter.ResurrectFile(sCompleteTempPath, sResurrectionPath);
                    FileInfo fi = new FileInfo(sResurrectionPath);

                    long iLen = fi.Length;
                    FileSplitter.RelinquishSpace(sOriginalName);
                    // Make a temp file for the resurrected version here:
                    string sStoreKey = sCPK + "/" + sOriginalName;
                    string sURL = "";

                    System.IO.File.Delete(sResurrectionPath);
                    if (sURL != "")
                    {

                        sXML = "<status>1</status><complete>1</complete><url>" + sURL +  "</url><bytes>" + iLen.ToString() + "</bytes>";
                        sXML += "<url0>" + sURL + "</url0>";
                        Response.Write(sXML);
                        Response.End();
                        return;
                    }
                    else
                    {
                        sXML = "<status>0</status>";
                        Log("Uplink failed ");
                        Response.Write(sXML);
                        Response.End();
                        return;
                    }
                }
                // Error Codes
                sXML = "<status>1</status>";
                Response.Write(sXML);
                Response.End();
                return;
            }

            if (true)
            {
                if (!gUser(this).Admin)
                {
                    MsgBox("Restricted", "Sorry this page is for admins only.", this);
                    return;
                }
            }

            if (!IsPostBack)
                FileUpload1.Attributes["onchange"] = "UploadFile(this)";
        }

        bool IsAllowableExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext.Length < 1) return false;
            ext = ext.Substring(1, ext.Length - 1);
            string allowed = "jpg;jpeg;gif;png;pdf;txt;htm;html;csv";
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
                    string sPath = Common.GetFolderUnchained("Temp");
                    string extension = Path.GetExtension(FileUpload1.FileName);
                    string newName = Guid.NewGuid().ToString() + extension;
                    string fullpath = Path.Combine(sPath, newName);
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


                    /*
                    Unchained.SubmitUnchainedTransaction(u);
                    // File storage
                    Task<List<string>> myTask = Uplink.Store2(newName, "MDN", "MV", fullpath, 3);
                    */

                    MsgBox("Object Storage Failed", "The server could not process the request.", this);

                    /*
                     string narr = "Thank you for using BiblePay Object Storage.  <br><br><a href=" + myTask.Result + ">Your URL is<br>" + myTask.Result[0] + "<br></a>";
                     MsgBox("Object Storage Successful", narr, this);
                     return;
                     }
                     else
                     {
                          MsgBox("Object Storage Failed", "The file extension provided is not allowed.", this);
                          return;
                     }
                     */
                }
            }
            else
            {
                lblmessage.Text = sb.ToString();
            }
        }
    }
}
