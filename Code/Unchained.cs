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

        /*
        public static string CreateSchema(string sTableName, string colnames, string datatypes)
        {
            string[] vcolnames = colnames.Split(",");
            if (vcolnames.Length < 2)
                return "column names list must contain at least 2 items.";
            for (int i = 0; i < vcolnames.Count(); i++)
            {
                if (vcolnames[i].Contains(" "))
                    return "Cannot contain spaces.";
            }
            string[] vdatatypes = datatypes.Split(",");
            if (vdatatypes.Length < 2)
                return "Datatypes list must contain at least 2 items.";
            for (int i = 0; i < vdatatypes.Length; i++)
            {
                string dt = vdatatypes[i];
                bool bFound = false;
                if (dt == "datetime" || dt == "guid" || dt == "float" || dt == "string")
                    bFound = true;
                if (!bFound)
                    return "Invalid datatype";
            }
            string sSchema = "<TABLE>" + sTableName + "</TABLE><COLUMNNAMES>" + colnames + "</COLUMNNAMES><DATATYPES>" + datatypes + "</DATATYPES>";
            Saved.Code.Uplink.StoreAndDelete("schema", sTableName, sSchema);
            return "";
        }
        */


        public static string SerializeDataTable(DataTable dt)
        {
            string sData = "";
            string sRowDelimiter = "[~]";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sRow = SerializeDataRow(dt.Rows[i]);
                sData += sRow + sRowDelimiter;
            }
            return sData;
        }

        public static DataTable DeserializeDataTable(string sData)
        {
            string sRowDelimiter = "[~]";
            DataTable dt = new DataTable();
            if (sData == "" || sData == null)
                return dt;

            dt.Clear();
            string[] vRows = sData.Split(sRowDelimiter);

            if (vRows.Length >= 1)
            {
                DataRow dr = DeserializeDataRow(vRows[0]);
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    dt.Columns.Add(dr.Table.Columns[i].ColumnName);
                }
            }
            for (int i = 0; i < vRows.Length; i++)
            {
                DataRow dr = DeserializeDataRow(vRows[i]);
                dt.ImportRow(dr);
            }
            return dt;
        }

        public static string SerializeDataRow(DataRow dr)
        {
            string sSchema = "<table>" + dr.Table.TableName + "</table>";
            string sDelimiter = "[|]";
            string sColDelimiter = "[-]";
            string sData = "";
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string colName = dr.Table.Columns[i].ColumnName;
                object colValue = dr[i];
                string sType = dr.Table.Columns[i].DataType.ToString();
                string sRow = sType + sDelimiter + i.ToString() + sDelimiter + colName + sDelimiter + colValue.ToNonNullString() + sDelimiter;
                sData += sRow + sColDelimiter;
            }
            sData += sSchema;
            return sData;
        }
        public static DataRow DeserializeDataRow(string sData)
        {

            if (sData.Length < 10)
                return null;

            DataTable dt = new DataTable();
            dt.Clear();

            DataRow _datarow = null;

            // Data Row requires schema
            string sDelimiter = "[|]";
            string sColDelimiter = "[-]";
            string[] vCols = sData.Split(sColDelimiter);
            for (int zOp = 0; zOp <= 1; zOp++)
            {
                for (int i = 0; i < vCols.Length; i++)
                {
                    string sColumn = vCols[i];
                    string[] vData = sColumn.Split(sDelimiter);
                    if (vData.Length >= 3)
                    {
                        string sType = vData[0];
                        double nColOrdinal = GetDouble(vData[1]);
                        string sColName = vData[2];
                        string sValue = vData[3];
                        if (zOp == 0)
                        {
                            dt.Columns.Add(sColName);
                        }
                        else if (zOp == 1)
                        {
                            _datarow[i] = sValue;
                        }

                    }
                }
                if (zOp == 0)
                    _datarow = dt.NewRow();
                if (zOp == 1)
                    dt.Rows.Add(_datarow);
            }
            string sTable = ExtractXML(sData, "<table>", "</table>").ToString();
            _datarow.Table.TableName = sTable;
            return _datarow;
        }

        /* Replaced by DSQL
        public static string InsertSQL(string sTable, string sPrimaryKey, DataRow dr)
        {
            string sFullKey = "table-" + sTable + "/" + sPrimaryKey;
            string sFile = sFullKey.GetHashCode().ToString() + ".dat";
            string sPath = GetFolderUnchained("DataStaging");
            string sFullpath = Path.Combine(sPath, sFile);
            dr.Table.TableName = sTable;
            string sData = SerializeDataRow(dr);
            Unchained.WriteToFile(sFullpath, sData);
            DataRow dr1000 = DeserializeDataRow(sData);
            // SQL Storage (Non-File)
            Task<List<string>> myTask = Uplink.Store2(sFullKey, "", "", sFullpath);
            if (System.IO.File.Exists(sFullpath))
                System.IO.File.Delete(sFullpath);
            return myTask.Result[0];
        }
        */

        /*
        public static string InsertJSON(string sTable, string ID, string sJsonObject)
        {
            string sFile = sJsonObject.GetHashCode() + ".dat";
            string sPrimaryKey = sTable + "/" + sFile;
            string sPath = GetFolderUnchained("DataStaging");
            string sFullpath = Path.Combine(sPath, sFile);
            Unchained.WriteToFile(sFullpath, sJsonObject);
            // Non-File
            Task<List<string>> myTask = Uplink.Store2(sPrimaryKey, "", "", sFullpath);
            return myTask.Result[0];
        }
        */

        public static string ConvertDataType(Type t)
        {
            string t1 = t.ToString();
            string tout = "";
            if (t1 == "System.String")
            {
                tout = "string";
                return tout;
            }
            else if (t1 == "System.DateTime")
            {
                tout = "datetime";
                return tout;

            }
            else if (t1 == "System.Guid")
            {
                tout = "guid";
                return tout;
            }
            else if (t1 == "MONEY" || t1 == "FLOAT" || t1 == "System.Double")
            {
                tout = "float";
                return tout;
            }
            else
            {
                throw new Exception("Unknown Type");
            }
        }

        public static Tuple<string,string> DescribeDataTableSchema(DataTable dt)
        {
            string colnames = "";
            string datatypes = "";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colnames += dt.Columns[i].ColumnName + ",";
                Type t1 = dt.Columns[i].DataType;
                string sDataType = ConvertDataType(t1);
                datatypes += sDataType + ",";

            }
            colnames = Left(colnames, colnames.Length - 1);
            datatypes = Left(datatypes, datatypes.Length - 1);
            Tuple<string, string> t = Tuple.Create(colnames, datatypes);
            return t;
        }
        /*
        public static void ReplicateTable(string tablename)
        {
            string sql = "Select * from " + tablename + " order by added";
            DataTable dt = gData.GetDataTable(sql);
            Tuple<string, string> sSchema = DescribeDataTableSchema(dt);
            CreateSchema(tablename, sSchema.Item1, sSchema.Item2);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                InsertSQL(tablename, dt.Rows[i]["id"].ToString(), dt.Rows[i]);
            }
        }
        */

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

        public struct DataRowObject
        {
            public string Key;
            public string TableName;
        }
        /*
        public static async Task<List<DataRowObject>> ListContentsOfTable(string sTableName)
        {
            uplinkClient = new AmazonS3Client(GetBMSConfigurationKeyValue("s3key"), GetBMSConfigurationKeyValue("s3secret"), Amazon.RegionEndpoint.CACentral1);
            var lor = new ListObjectsRequest();
            lor.BucketName = "biblepay";
            lor.Prefix = sTableName;
            List<DataRowObject> lDROs = new List<DataRowObject>();
            var listResponse = uplinkClient.ListObjects(lor);
           
            foreach (S3Object obj in listResponse.S3Objects)
            {
                    DataRowObject dro = new DataRowObject();
                    dro.Key = obj.Key;
                    lDROs.Add(dro);
            }
            return lDROs;
        }
        */


        /*
        public static string ReplicateIntoAWS(string sURL)
        {
            Saved.Code.MyWebClient w = new MyWebClient();
            string sFileName = Guid.NewGuid().ToString();
            string sPath = "c:\\inetpub\\wwwroot\\Saved\\Uploads\\" + sFileName;
            w.DownloadFile(sURL, sPath);
            string[] vChop = sURL.Split("/");
            string sChop = vChop[vChop.Length - 1];
            // File
            Task<List<string>> t = Store2("san-" + sChop, "", "", sPath);
            System.IO.File.Delete(sPath);
            return t.Result[0];
        }
        */


        private static Amazon.RegionEndpoint GetEndpoint(int iDensity, out string sBucketName)
        {
            
            if (iDensity == 0)
            {
                sBucketName = "biblepay";
                return Amazon.RegionEndpoint.CACentral1;
            }
            else if (iDensity == 1)
            {
                sBucketName = "biblepay-useast";
                return Amazon.RegionEndpoint.USEast1;
            }
            else if (iDensity == 2)
            {
                sBucketName = "biblepay-eucentral";
                return Amazon.RegionEndpoint.EUCentral1;
            }
            else if (iDensity == 3)
            {
                sBucketName = "biblepay-uswest";
                return Amazon.RegionEndpoint.USWest1;
            }
            else
            {
                sBucketName = "biblepay";
                throw new Exception("Invalid endpoint");
            }
        }

        private static string GetURLPrefix(Amazon.RegionEndpoint r, string sBucketName)
        {
            //string sURL = "https://" + sBucketName + ".s3." + r.SystemName + "." + r.PartitionDnsSuffix;
            string sURL = "https://media.biblepay.org";
            return sURL;
        }

        // NOTE:  This is a proof-of-concept for devnet (not yet ready for testnet).  We are going to investigate neural networks, and quantum transactions before deciding on the final sidechain struct.
        public struct SidechainTransaction
        {
            public string TXID;
            public double nFee;
            public string BlockHash;
            public string FileName;
            public double nHeight;
            public bool fFile;
            public string URL;
            public string CPK;
            public double nDuration;
            public double nDensity;
            public double nSize;
            public string Network; // Testnet or MainNet etc.
        }

        public static string SerializeTransaction(SidechainTransaction u)
        {
            string delim = "[~]";
            string data = u.BlockHash + delim + u.TXID + delim + u.FileName + delim + u.nFee.ToString() + delim + u.URL + delim + u.CPK + delim + u.nDuration.ToString() + delim + u.nDensity.ToString() + delim + u.Network + delim + u.nHeight.ToString() + delim + u.nSize.ToString() + delim;
            return data;
        }

        public static bool StoreTransaction(SidechainTransaction u)
        {
            string sData = SerializeTransaction(u);
            string sKey = "table-blocks-" + u.BlockHash + "-" + u.TXID;
            string sFile = sKey.GetHashCode().ToString() + ".dat";
            string sPath = GetFolderUnchained("DataStaging");
            string sFullpath = Path.Combine(sPath, sFile);
            Unchained.WriteToFile(sFullpath, sData);
            Store3(sKey, u.CPK, sFullpath);
            //Task<List<string>> myTask = Store2(sKey, "", "", sFullpath);
            fDirtyBlockData = true;
            return true;
        }

        public struct Response
        {
            public string Results;
            public string ErrorCode;
        };

        public static Response Store3(string sOriginalName, string CPK, string sSourcePath, int iDensityLevel = 1)
        {
            // This stores the file in the local file store - which be have replicated to either storj or the biblepay SAN.  This file store is connected to our CDN.
            // Insert the file in SQL also.
            Response r = new Response();
            try
            {
                FileInfo fi = new FileInfo(sSourcePath);
                string sDir = GetFolderUnchained(CPK);
                string sOutPath = sDir + "\\" + sOriginalName;
                if (!Directory.Exists(sDir))
                {
                    Directory.CreateDirectory(sDir);
                }
                BOINC.BBPCopyFile(sSourcePath, sOutPath);
                
                string sURL = "https://unknown.org/Unchained/" + CPK + "/" + sOriginalName;
                r.Results = sURL;

                return r;
            }
            catch (Exception ex)
            {
                Log("Store3::" + ex.Message);
                r.ErrorCode = ex.Message;
                return r;
            }
        }
        // These endpoints are disabled.  We no longer deal with AWS since they appear to be abusive big tech.
        // In addition, AWS has no easy way to throttle bandwidth (without putting a CDN in front of it), so whats the use.
        /*
        public static async Task<List<string>> Store2(string sKey, string sMetadataName, string sMetadataValue, string sFilePath, int iDensityLevel = 1)
        {
            List<string> out_url = new List<string>();

            for (int i = 0; i < iDensityLevel; i++)
            {
                string sBucketName = "";
                Amazon.RegionEndpoint EP1 = GetEndpoint(i, out sBucketName);
                try
                {
                    uplinkClient = new AmazonS3Client(GetBMSConfigurationKeyValue("s3key"), GetBMSConfigurationKeyValue("s3secret"), EP1);
                    // Open multiple storage threads in parallel for write speed (handled internally):
                    var fileTransferUtility = new TransferUtility(uplinkClient);
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = sBucketName,
                        FilePath = sFilePath,
                        StorageClass = S3StorageClass.Standard,
                        PartSize = 7000000,
                        Key = sKey,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    string sURL = GetURLPrefix(EP1, sBucketName) + "/" + sKey;
                    out_url.Add(sURL);
                }
                catch (Exception ex)
                {
                    Log("Failed to Store " + sKey + ":" + ex.Message);
                }
            }
            return out_url;
        }
        */


        public static DataTable GetFakeDataset()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Col1");
            dt.Columns.Add("Col2");
            for (int i = 0; i < 1000; i++)
            {
                DataRow _ravi = dt.NewRow();
                _ravi["Col1"] = "value_one_" + i.ToString() + "_1";
                _ravi["Col2"] = "value_two_" + i.ToString() + "_2";
                dt.Rows.Add(_ravi);
            }
            //            dt.WriteXML("dtDataxml");
            return dt;

        }
        public static string Read(string sKey)
        {
            try
            {
                MyWebClient w = new MyWebClient();
                string sURL = "https://media.biblepay.org/" + sKey;
                string sData = w.DownloadString(sURL);
                string sNumerical = ExtractXML(sKey, "/", ".dat").ToString();
                return sData;
            }
            catch(WebException)
            {
                // Gives us a chance to return a 403
                return "";
            }
            catch(Exception)
            {
                return "";
            }
        }

        public static DataRow GetDataRow(string sKey)
        {
            string sULData = Uplink.Read(sKey);
            DataRow dr1 = UnchainedDatabase.DeserializeDataRow(sULData);
            return dr1;
        }

        public static DataTable GetDataTableByView(string sTableName)
        {

            string sData = Uplink.Read("view-" + sTableName);
            DataTable dt1 = UnchainedDatabase.DeserializeDataTable(sData);
            return dt1;

        }

        /*
        public static void StoreAndDelete(string sTableType, string sTableName, string sData)
        {
            string sFullKey = sTableType + "-" + sTableName;
            string sFile = sFullKey.GetHashCode().ToString() + ".dat";
            string sPath = GetFolderUnchained("DataStaging");
            string sFullpath = Path.Combine(sPath, sFile);
            Unchained.WriteToFile(sFullpath, sData);
            Task<List<string>> myTask = Uplink.Store2(sFullKey, "", "", sFullpath);
            if (System.IO.File.Exists(sFullpath))
                System.IO.File.Delete(sFullpath);
        }
        */



        private static ReaderWriterLockSlim dictLock = new ReaderWriterLockSlim();

        public static Dictionary<string, string> dictSideChain = new Dictionary<string, string>();
        public static object cs_block = new object();

        public static double KeyToBlockHeight(string sKey)
        {
            string[] myKey = sKey.Split("-");
            if (myKey.Length > 1)
            {
                double dHeight = GetDouble(myKey[0]);
                return dHeight;
            }
            return 0;
        }

        public static int nLastBlockData = 0;
        public static string msBlockData = "";
        public static int nHitCount = 0;
        public static bool fDirtyBlockData = false;
 
        public static void GetRawBlockData()
        {
            /*
            string sBlockPrefix = "table-blocks-";
            lock (cs_block)
            {
                nLastBlockData = UnixTimeStamp();
                Task<List<DataRowObject>> taskDROS = Uplink.ListContentsOfTable(sBlockPrefix);
                foreach (DataRowObject dro in taskDROS.Result)
                {
                    string sData = Read(dro.Key);
                    string[] vData = sData.Split("[~]");
                    if (vData.Length > 9)
                    {
                        double nHeight = GetDouble(vData[9]);
                        string txid = vData[1];
                        string sKey = nHeight.ToString() + "-" + txid;
                        {
                            dictSideChain[sKey] = sData;
                        }
                    }
                }
            }
            */
        }
        
        public static void CreateView(string sTableName)
        {
            /*
            DataTable dt = new DataTable();
            dt.Clear();
            Task<List<DataRowObject>> taskDROS = Uplink.ListContentsOfTable(sTableName);
            // Schema
            if (taskDROS.Result.Count > 0)
            {
                DataRow drFirst = GetDataRow(taskDROS.Result[0].Key);
                for (int i = 0; i < drFirst.Table.Columns.Count; i++)
                {
                    dt.Columns.Add(drFirst.Table.Columns[i].ColumnName);
                }
            }
            foreach (DataRowObject dro in taskDROS.Result)
            {
                DataRow dr1 = GetDataRow(dro.Key);
                if (dr1 != null)
                {
                    dt.ImportRow(dr1);
                }
            }
            // Serialize the View
            dt.TableName = sTableName;
            string sView = UnchainedDatabase.SerializeDataTable(dt);
            StoreAndDelete("view", sTableName, sView);
            // Deserialize the View
            DataTable dt999 = UnchainedDatabase.DeserializeDataTable(sView);
            */
        }

        public static string Store(string sKey, string sMetadataName, string sMetadataValue, string sFilePath)
        {
            uplinkClient = new AmazonS3Client(GetBMSConfigurationKeyValue("s3key"), GetBMSConfigurationKeyValue("s3secret"), Amazon.RegionEndpoint.CACentral1);
            bool fStored = WriteObject("biblepay", sKey, sFilePath, sMetadataName, sMetadataValue);
            string sURL = fStored ? "https://media.biblepay.org/" + sKey : "";
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
            catch (Exception)
            {
                Log("Unable to set UTC on " + path);
            }
        }

        public static void WriteToFile(string path, string data)
        {
            File.WriteAllText(path, data);
            // This function is here in case we need to update the write time to UTC: (a port from BMS)
            // File.SetLastWriteTimeUtc(path, FromUnixTimeStamp(TimeStamp));
        }
    }
}
