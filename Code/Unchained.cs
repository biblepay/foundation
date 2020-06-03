using Google.Authenticator;
using Microsoft.VisualBasic;
using NBitcoin;
using System;

using Newtonsoft.Json;
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
using static Saved.Code.Common;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Threading.Tasks;
using System.Threading;

namespace Saved.Code
{

    public static class UnchainedDatabase
    {

        public static Dictionary<int, string> dictObjects = new Dictionary<int, string>();

        public static DataTable GetDataTable(string sTable)
        {
            int iSleep = 0;
            DataTable dt = new DataTable();
            string o1 = "";
            Dictionary<int, Thread> dictThreads = new Dictionary<int, Thread>();
            int hiLevel = 312;
            for (int i = 0; i < hiLevel; i++)
            {
                string sPrimaryKey = sTable + "/" + i.ToString() + ".dat";
                ThreadStart starter = delegate { Uplink.Read(sPrimaryKey); };
                dictThreads[i] = new Thread(starter);
                dictThreads[i].Start();

            }
            for (int i = 0; i < hiLevel; i++)
            {
                dictThreads[i].Join();
            }
            string test1 = "";
            return dt;
        }
        private static int nFPK = -1;
        public static int GetFilePrimaryKey(string sTable)
        {
            if (nFPK > 0)
                return nFPK;
            for (int i = 0; i < 1000000; i++)
            {
                string sPrimaryKey = sTable + "/" + i.ToString() + ".dat";
                string data = Uplink.Read(sPrimaryKey);
                if (data == "")
                {
                    nFPK = i;
                    return i;
                }
            }
            return -1;
        }
        public static string Insert(string sTable, string ID, string sJsonObject)
        {
            // We need to know the max(rowid) in this table as we do inserts
            string sFile = GetFilePrimaryKey(sTable) + ".dat";
            string sPrimaryKey = sTable + "/" + sFile;

            string sPath = GetFolderUnchained("DataStaging");
            string sFullpath = Path.Combine(sPath, sFile);
            Unchained.WriteToFile(sFullpath, sJsonObject);
            Task<string> myTask = Uplink.Store2(sPrimaryKey, "", "", sFullpath);
            if (myTask.Result.Length > 0)
            {
                nFPK++;
            }
            return myTask.Result;
        }
    }

    public class UnchainedTransaction
    {
            public string SenderBBPAddress;
            public string RecipientBBPAddress;
            public string KeySection;
            public string KeyName;
            public string KeyValue;
            public double nFee;
            public string Signature;
            public int nTimestamp;

        public string Serialize()
        {
            string js = JsonConvert.SerializeObject(this);
            return js;
        }
    }

    public static class Uplink
    {
        private static IAmazonS3 uplinkClient;

        static bool WriteObject(string sBucket, string sKey, string sFilePath, string sMetadataName, string sMetadataValue)
        {
            try
            {
                // 1. Put object-specify only key name for the new object.
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = sBucket,
                    Key = sKey,
                    FilePath = sFilePath
                };
                // (Future after refactoring): Add some metadata:  putRequest1.Metadata.Add(sMetadataName, sMV);
                uplinkClient.PutObject(putRequest1);
                return true;

            }
            catch (Exception e)
            {
                Log("Error while WritingObject " + e.Message);
            }
            return false;
        }
       
    public static async Task<string> Store2(string sKey, string sMetadataName, string sMetadataValue, string sFilePath)
        {
            try
            {
                uplinkClient = new AmazonS3Client(GetBMSConfigurationKeyValue("s3key"), GetBMSConfigurationKeyValue("s3secret"), Amazon.RegionEndpoint.CACentral1);
                // Open multiple storage threads in parallel for write speed (handled internally):
                var fileTransferUtility = new TransferUtility(uplinkClient);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = "biblepay",
                    FilePath = sFilePath,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 7000000, 
                    Key = sKey,
                    CannedACL = S3CannedACL.PublicRead
                };
                fileTransferUtility.Upload(fileTransferUtilityRequest);
                string out_url = "https://biblepay.s3.ca-central-1.amazonaws.com/" + sKey;
                return out_url;
            }
            catch(Exception ex)
            {
                Log("Failed to Store " + sKey + ":" + ex.Message);
            }
            return "";
        }

        public static string Read(string sKey)
        {
            try
            {
                MyWebClient w = new MyWebClient();
                string sURL = "https://biblepay.s3.ca-central-1.amazonaws.com/" + sKey;
                string sData = w.DownloadString(sURL);
                string sNumerical = ExtractXML(sKey, "/", ".dat").ToString();

                UnchainedDatabase.dictObjects[(int)GetDouble(sNumerical)] = sData;

                return sData;
            }
            catch(WebException)
            {
                // Gives us a chance to return a 403
                return "";
            }
            catch(Exception ex)
            {
                return "";
            }
        }
        public static string Store(string sKey, string sMetadataName, string sMetadataValue, string sFilePath)
        {
            uplinkClient = new AmazonS3Client(GetBMSConfigurationKeyValue("s3key"), GetBMSConfigurationKeyValue("s3secret"), Amazon.RegionEndpoint.CACentral1);
            bool fStored = WriteObject("biblepay", sKey, sFilePath, sMetadataName, sMetadataValue);
            string sURL = fStored ? "https://biblepay.s3.ca-central-1.amazonaws.com/" + sKey : "";
            return sURL;
        }
    }


    public static class Unchained
    {

        // SubmitValue, SendMoney, SubmitFile
        public static void SubmitUnchainedTransaction(UnchainedTransaction tx)
        {

            string data = tx.Serialize();
            string path = GetFolderUnchained("Unprocessed");
            string fn = data.GetHashCode() + ".unc";
            string fullpath = Path.Combine(path, fn);
            WriteToFile(fullpath, data);
        }

        public static void UpdateTimestamp(string path, DateTime ts)
        {
            try
            {
                File.SetLastWriteTimeUtc(path, ts);
            }
            catch (Exception ex)
            {
                Log("Unable to set UTC on " + path);
            }
        }

        public static void WriteToFile(string path, string data)
        {
            
            File.WriteAllText(path, data);
            //File.SetLastWriteTimeUtc(path, FromUnixTimeStamp(TimeStamp));
        }
    }
}
