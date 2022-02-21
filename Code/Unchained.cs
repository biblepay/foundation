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
