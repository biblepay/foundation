using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using static Saved.Code.Common;

namespace Saved.Code
{
    public static class DataOps
    {

        public static User GetUserRecord(string id)
        {
            string sql = "Select * from USERS where id = '" + id + "'";
            DataTable dt = gData.GetDataTable(sql);
            User u = new User();
            if (dt.Rows.Count > 0)
            {
                u.EmailAddress = dt.Rows[0]["EmailAddress"].ToString();
                u.UserId = dt.Rows[0]["id"].ToString();
                u.UserName = dt.Rows[0]["username"].ToString();
                u.CPKAddress = dt.Rows[0]["CPKAddress"].ToString();
            }
            return u;
        }

        public static void AdjBalance2(double nAmount, string sUserId, string sNotes, string TXID)
        {
            string sql = "Insert into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), '', @txid, @userid, getdate(), @amount, @height, @notes)";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", sUserId);
            command.Parameters.AddWithValue("@amount", nAmount);
            command.Parameters.AddWithValue("@txid", TXID);
            command.Parameters.AddWithValue("@height", _pool._template.height);
            command.Parameters.AddWithValue("@notes", sNotes);
            gData.ExecCmd(command, false, true, true);
        }

        public static void AdjBalance(double nAmount, string sUserId, string sNotes)
        {
            string sql = "Insert into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), '', @txid, @userid, getdate(), @amount, @height, @notes)";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", sUserId);
            command.Parameters.AddWithValue("@amount", nAmount);
            command.Parameters.AddWithValue("@txid", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@height", _pool._template.height);
            command.Parameters.AddWithValue("@notes", sNotes);
            gData.ExecCmd(command, false, true, true);
        }
        public static double GetTotalFrom(string userid, string table, string where)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null and " + where;
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }

        public static double GetTotalFrom(string userid, string table)
        {
            string sql = "Select sum(amount) amount from " + table + " where userid=@userid and amount is not null";

            if (userid == "")
            {
                sql = "Select sum(amount) amount from " + table + " where amount is not null";
            }

            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@userid", userid);
            double nBalance = gData.GetScalarDouble(command, "amount");
            return nBalance;
        }
        public static string GetUserIDByAPIKey(string sAPIKEY)
        {
            string sql = "Select ID from Users where APIKEY=@apikey";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@apikey", sAPIKEY);
            string sID = gData.GetScalarString(command, "id");
            return sID;
        }
        public static bool ChargeTransaction(string sAPIKEY, double nBytes, string sTransType, string sFN)
        {
            string sUserId = GetUserIDByAPIKey(sAPIKEY);
            if (sUserId == "")
                return false;

            double dAmount = nBytes / 1000000;
            string sNotes = sTransType + " " + nBytes.ToString() + " " + Left(sFN, 100);
            AdjBalance(-1 * dAmount, sUserId, sNotes);
            return true;
        }

        public static bool UpdateSingleField(string sTable, string field1, string fieldvalue, string fieldkey)
        {
            string sql = "Update " + sTable + " Set " + field1 + "=@fieldvalue WHERE id=@key";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@field1", field1);
            command.Parameters.AddWithValue("@fieldvalue", fieldvalue);
            command.Parameters.AddWithValue("@key", fieldkey);
            gData.ExecCmd(command);
            return true;
        }

        public static void PersistUser(ref User u)
        {
            // Store in database if not there, otherwise pull in ID; set picture=avatar
            string sql0 = "Select * from Users where UserName=@un";
            SqlCommand command0 = new SqlCommand(sql0);
            command0.Parameters.AddWithValue("@un", u.UserName);
            DataTable dt = gData.GetDataTable(command0, false);
            if (dt.Rows.Count == 0)
            {
                //Add the user here
                string sql = "Insert into Users (id,emailaddress,passwordhash,added,updated,picture,username,admin) values (newid(),'','',getdate(),getdate(),@avatar,@username,0)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@avatar", u.AvatarURL);
                command.Parameters.AddWithValue("@username", u.UserName);
                gData.ExecCmd(command);
            }
            else
            {
                // Freshen
                string sql = "Update Users set picture=@avatar,updated=getdate() where username=@username";
                SqlCommand command = new SqlCommand(sql);
                if (u.AvatarURL == "" || u.AvatarURL == null)
                {
                    u.AvatarURL = " ";
                }
                if (u.AvatarURL != " ")
                {
                    command.Parameters.AddWithValue("@avatar", u.AvatarURL);
                    command.Parameters.AddWithValue("@username", u.UserName);
                    gData.ExecCmd(command);
                }
            }
            dt = gData.GetDataTable(command0, false);
            if (dt.Rows.Count > 0)
            {
                u.UserId = dt.Rows[0]["Id"].ToString();
                u.Require2FA = GetDouble(dt.Rows[0]["TwoFactor"]);
                u.Admin = GetDouble(dt.Rows[0]["Admin"].ToString()) == 0 ? false : true;
                u.RandomXBBPAddress = dt.Rows[0]["RandomXBBPAddress"].ToNonNullString();
            }

        }

        public static double GetHPS(string bbp)
        {
            string sql = "Select sum(Hashrate) hr from leaderboard where bbpaddress = '" + bbp + "'";
            double dR = gData.GetScalarDouble(sql, "hr");
            return dR;
        }


        public static string GetNotes(string sPath)
        {
            string sNotesPath = sPath.Replace(".mp4", ".description");
            if (!System.IO.File.Exists(sNotesPath))
                return "";

            System.IO.StreamReader file = new System.IO.StreamReader(sNotesPath);
            string data = file.ReadToEnd();
            data = data.Replace("'", "");
            data = data.Replace("`", "");
            data = data.Replace("\"", "");
            if (data.Length > 7999)
                data = data.Substring(0, 7999);

            return data;
        }

        public static string GetAvatar(object field)
        {
            string sUserPic = NotNull(field);
            if (sUserPic == "")
            {
                sUserPic = "<img src='images/emptyavatar.png' width=50 height=50 >";
            }
            return sUserPic;
        }
        public static void LiquidateAllSanctuaries()
        {
            string sql = "Select * from SanctuaryInvestments";
            DataTable dt = gData.GetDataTable(sql);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                double nReq = Math.Round(GetDouble(dt.Rows[i]["Amount"]), 4);
                string sUserId = dt.Rows[i]["UserId"].ToString();
                IncrementAmountByFloat("SanctuaryInvestments", nReq * -1, sUserId);
                AdjBalance(nReq, sUserId, "Sanctuary Liquidation " + nReq.ToString());
            }
        }
        public static string GetSingleTweet(string id)
        {
            if (id == "")
                return "N/A";
            string sql = "Select * from Tweet left Join Users on Users.ID = Tweet.UserID where Tweet.id = @id";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            DataTable dt = gData.GetDataTable(command);
            if (dt.Rows.Count < 1)
            {
                return "";
            }
            SavedObject s = RowToObject(dt.Rows[0]);
            string sUserPic = GetAvatar(s.Props.Picture);
            string sUserName = NotNull(s.Props.UserName);
            if (sUserName == "")
                sUserName = "N/A";
            string sHTMLBody = ReplaceURLs(s.Props.Body);
            string sBody = "<div style='min-height:300px'><span style=''>" + sHTMLBody + "</span></div>";
            string div = "<table style='padding:10px;' width=73%><tr><td>User:<td>" + sUserPic + "</tr>"
                + "<tr><td>User Name:<td>" + sUserName + "</tr>"
                + "<tr><td>Added:<td>" + s.Props.Added.ToString() + "</td></tr>"
                + "<tr><td>Subject:<td>" + s.Props.Subject + "</td></tr>"
                + "<tr><td>&nbsp;</tr><tr><td width=8%>Body:<td style='border:1px solid lightgrey;min-height:300px' colspan=1 xbgcolor='grey' width=40%>" + sBody + "</td></tr></table>";
            return div;
        }


        public struct UTXO
        {
            public string TXID;
            public int nOrdinal;
            public string Address;
            public double nAmount;
            public bool Spent;
            public bool Found;
        }

        static int nLastClean = 1;
        private static UTXO GetDbUTXO(string txid, int iOrdinal)
        {
            int nElapsed = UnixTimeStamp() - nLastClean;
            if (nElapsed > (60 * 60 * 1))
            {
                nLastClean = UnixTimeStamp();
                string sql1 = "Delete From UTXO where Added < getdate()-1";
                gData.Exec(sql1);
            }
            string sql = "Select * from UTXO where txid='" + BMS.PurifySQL(txid, 100) + "'";
            DataTable dt1 = gData.GetDataTable(sql);
            UTXO db1 = new UTXO();
            if (dt1.Rows.Count > 0)
            {
                db1.Address = dt1.Rows[0]["Address"].ToString();
                db1.TXID = txid;
                db1.nOrdinal = iOrdinal;
                db1.Found = true;
                db1.nAmount = GetDouble(dt1.Rows[0]["Amount"].ToString());
                db1.Spent = ToBool(GetDouble(dt1.Rows[0]["Spent"].ToString()));
                return db1;
            }
            return db1;
        }

        private static string SerializeUTXO(UTXO u)
        {
            string sHash = u.TXID + "-" + u.nOrdinal.ToString();
            string sData = "<utxo><hash>" + sHash + "</hash><address>" + u.Address.ToNonNullString() + "</address><amount>" + u.nAmount.ToString()
                + "</amount><spent>" + GetDouble(u.Spent) + "</spent></utxo><eof></html>";
            return sData;
        }

        private static int nLastUTXOReport = 0;
        private static string sCachedUTXOReport = "";
        public static string GetUTXOReport()
        {
            int nElapsed = UnixTimeStamp() - nLastUTXOReport;
            if (nElapsed < (60 * 60 * 1) && sCachedUTXOReport != "")
            {
                return sCachedUTXOReport;
            }
            nLastUTXOReport = UnixTimeStamp();
            string sql = "Select * from UTXO";
            DataTable dt1 = gData.GetDataTable(sql);
            string sData = "<utxos>";
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                UTXO u = new UTXO();
                u.Address = dt1.Rows[i]["Address"].ToString();
                u.Found = true;
                u.nAmount = GetDouble(dt1.Rows[i]["Amount"]);
                u.Spent = Convert.ToBoolean(GetDouble(dt1.Rows[i]["Spent"]));
                u.TXID = dt1.Rows[i]["TXID"].ToString();
                string sUTXO = SerializeUTXO(u);
                sData += sUTXO;
            }
            sData += "</utxos><eof></html>";
            sCachedUTXOReport = sData;
            return sData;
        }
        private static void PersistUTXO(UTXO u)
        {
            string sql = "delete from UTXO where txid='" + BMS.PurifySQL(u.TXID, 100) + "'\r\nInsert into UTXO (id,txid,ordinal,address,amount,added,spent) values (newid(), '" + BMS.PurifySQL(u.TXID, 100) + "','" + u.nOrdinal + "','" + u.Address + "','"
                + u.nAmount.ToString() + "',getdate(),'" + GetDouble(u.Spent) + "')";
            gData.Exec(sql);
        }
        public static UTXO GetUTXOCache(string sTicker, string txid, int iOrdinal)
        {
            UTXO dbUTXO = GetDbUTXO(txid, iOrdinal);
            if (dbUTXO.Found)
            {
                return dbUTXO;
            }
            dbUTXO = BMS.GetTxOut(sTicker, txid, iOrdinal);
            if (dbUTXO.Found)
                PersistUTXO(dbUTXO);
            return dbUTXO;
        }

        public static void IncSysByFloat(string sKey, double nFloat)
        {
            string sql = "select Value from System where  systemKey = '" + sKey + "'";
            double dAmt = gData.GetScalarDouble(sql, "Value");
            dAmt += nFloat;
            sql = "Update System set Value='" + dAmt.ToString() + "' where SystemKey = '" + sKey + "'";
            gData.Exec(sql);
        }

        public static void IncrementAmountByFloat(string table, double nincrby, string userid)
        {
            try
            {
                string sql = "Select count(*) ct from " + table + " where userid=@userid";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", userid);
                double nCt = gData.GetScalarDouble(command, "ct");
                if (nCt == 0)
                {
                    // Add the shell
                    sql = "insert into " + table + " (id, userid, added, amount) values (newid(), @userid, getdate(), 0)";
                    command = new SqlCommand(sql);
                    command.Parameters.AddWithValue("@userid", userid);
                    gData.ExecCmd(command, false, true, true);
                }
                sql = "Update " + table + " set updated=getdate(),amount=amount+" + nincrby.ToString() + " where userid=@userid and id in (Select top 1 ID from " + table + " where userid=@userid)";
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@userid", userid);
                gData.ExecCmd(command, false, true, true);
            }
            catch (Exception ex)
            {
                Log("IncAmountByFloat::" + ex.Message);
            }
        }







        public static double GetUserBalance(string id)
        {
            return GetTotalFrom(id, "Deposit");
        }

        public static double GetUserBalance(Page p)
        {
            return GetTotalFrom(gUser(p).UserId.ToString(), "Deposit");
        }

        public static string GetTotalSancInvestment(Page p)
        {
            return GetTotalFrom(gUser(p).UserId.ToString(), "SanctuaryInvestments").ToString();
        }



    }
    public class MySQLData
    {
        private static string MySqlConn()
        {
            string connStr = "server=" + GetBMSConfigurationKeyValue("mysqlhost") + ";user=" + GetBMSConfigurationKeyValue("mysqluser") + ";database=" 
                + GetBMSConfigurationKeyValue("mysqldatabase") + ";port=3306;password=" + GetBMSConfigurationKeyValue("mysqlpassword");
            return connStr;
        }

        public static string GetScalarString(string sql, int ordinal)
        {
            try
            {
                MySqlDataReader dr = MySQLData.GetMySqlDataReader(sql);
                while (dr.Read())
                {
                    return dr[ordinal].ToString();
                }
            }
            catch(Exception)
            {
                
            }
            return "";
        }

        public static MySqlDataReader GetMySqlDataReader(string sql)
        { 
            MySqlConnection conn = new MySqlConnection(MySqlConn());
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return rdr;
        }

        public static bool ExecuteNonQuery(string sql)
        {
            MySqlConnection conn = new MySqlConnection(MySqlConn());
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                
                return true;
            }
            catch (Exception ex)
            {
                Log("ExecuteNonQuery[mysql]::" + ex.Message);
                return false;
            }


        }
    }

    public class Data
    {
        public enum SecurityType
        {
            REQ_SA = 10,
            READ_ONLY = 0,
            REPLICATOR = 11,
            UNKNOWN = 99
        }
        
        private string RemoteHostName = "";
        private SecurityType _SecurityType = SecurityType.UNKNOWN;

        public string sSQLConn()
        {
            string sCS = "Database=saved; MultipleActiveResultSets=true;Connection Timeout=7; ";

            if (RemoteHostName != "" && RemoteHostName != null)
            {
                sCS += "Server=" + RemoteHostName + ";";
            }
            else
            {
                sCS += "Server=" + GetBMSConfigurationKeyValue("SavedDatabaseHost") + ";";
            }

            if (_SecurityType == SecurityType.REQ_SA)
            {
                sCS += "Uid=" + GetBMSConfigurationKeyValue("SavedDatabaseUser")
                + ";pwd=" + GetBMSConfigurationKeyValue("SavedDatabasePassword");
            }
            else if (_SecurityType == SecurityType.REPLICATOR)
            {
                sCS = "Database=bms;MultipleActiveResultSets=true;Connection Timeout=7;";
                sCS += "Server=" + GetBMSConfigurationKeyValue("ReplicatorDatabaseHost") + ";";
                sCS += "Uid=" + GetBMSConfigurationKeyValue("ReplicatorDatabaseUser")
                + ";pwd=" + GetBMSConfigurationKeyValue("ReplicatorDatabasePassword");
                return sCS;
            }
            else
            {
                throw new Exception("Unknown Security Type");
            }

            return sCS;
        }

        public Data(SecurityType sa, string _RemoteHostName = "")
        {
            // Constructor goes here; since we use SQL Server connection pooling, dont create connection here, for best practices create connection at usage point and destroy connection after Using goes out of scope - see GetDataTable
            // This keeps the pool->databaseserver connection count < 10.  
            _SecurityType = sa;
            RemoteHostName = _RemoteHostName;
        }

        public SqlConnection GetSqlConn()
        {
            SqlConnection con = new SqlConnection(sSQLConn());
            return con;
        }

        public void ExecWithThrow(string sql, bool bLogErr, bool bLog = true)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    con.Open();
                    SqlCommand myCommand = new SqlCommand(sql, con);
                    myCommand.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                if (bLogErr) Log("EXECWithThrow: " + sql + "," + ex.Message);
                throw (ex);
            }
        }


        public void ExecCmd(SqlCommand cmd, bool bLog = true, bool bLogError = true, bool bThrow = false)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    if (PoolCommon.fLogSql)
                    {
                        Log("ExecCmd1: " + cmd.CommandText);
                    }

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                if (bThrow)
                    throw (ex);
                
                if (bLogError)
                    Log(" EXEC: " + cmd.CommandText + "," + ex.Message);
            }

        }
        public void Exec(string sql, bool bLog = true, bool bLogError = true)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    con.Open();
                    SqlCommand myCommand = new SqlCommand(sql, con);
                    if (PoolCommon.fLogSql)
                    {
                        Log("ExecCmd2: " + sql);
                    }

                    myCommand.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {

                if (bLogError)
                    Log(" EXEC: " + sql + "," + ex.Message);
            }

        }
        public void ExecWithTimeout(string sql, double lTimeout)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    con.Open();
                    if (PoolCommon.fLogSql)
                    {
                        Log("ExecCmd3: " + sql);
                    }

                    SqlCommand myCommand = new SqlCommand(sql, con);
                    myCommand.CommandTimeout = (int)lTimeout;
                    myCommand.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                Log(" EXECWithTimeout: " + sql + "," + ex.Message);
            }

        }

        public DataTable GetDataTable(SqlCommand sqlCommand, bool bLog = true, bool bThrow = false)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    int nConnectionTimeout = con.ConnectionTimeout;
                    con.Open();
                    sqlCommand.Connection = con;
                    SqlDataAdapter a = new SqlDataAdapter(sqlCommand);
                    if (PoolCommon.fLogSql)
                    {
                        Log("ExecCmd4: " + sqlCommand.CommandText);
                    }

                    DataTable t = new DataTable();
                    a.Fill(t);
                    return t;
                }
            }
            catch (Exception ex)
            {
                Log("GetDataTableViaSqlCommand:" + sqlCommand.CommandText + "," + ex.Message);
                if (bThrow) throw(ex);
            }
            DataTable dt = new DataTable();
            return dt;
        }


        public DataTable GetDataTable(string sql, bool bLog = true)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    int nConnectionTimeout = con.ConnectionTimeout;
                    con.Open();
                    SqlDataAdapter a = new SqlDataAdapter(sql, con);
                    DataTable t = new DataTable();
                    a.Fill(t);
                    return t;
                }
            }
            catch (Exception ex)
            {
                Log("GetDataTable:" + sql + "," + ex.Message);
            }
            DataTable dt = new DataTable();
            return dt;
        }


        public double GetScalarDouble(SqlCommand command, object vCol, bool bLog = true)
        {
            DataTable dt1 = GetDataTable(command, bLog);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    object oOut = null;
                    if (vCol.GetType().ToString() == "System.String")
                    {
                        oOut = dt1.Rows[0][vCol.ToString()];
                    }
                    else
                    {
                        oOut = dt1.Rows[0][Convert.ToInt32(vCol)];
                    }
                    double dOut = GetDouble(oOut.ToString());

                    return dOut;
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public double GetScalarAge(string sql, object vCol, bool bLog = true)
        {
            DataTable dt1 = GetDataTable(sql, bLog);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    object oOut = null;
                    if (vCol.GetType().ToString() == "System.String")
                    {
                        oOut = dt1.Rows[0][vCol.ToString()];
                    }
                    else
                    {
                        oOut = dt1.Rows[0][Convert.ToInt32(vCol)];
                    }
                    DateTime d1 = Convert.ToDateTime(oOut);
                    TimeSpan vAge = DateTime.Now - d1;
                    return vAge.TotalSeconds;

                }
            }
            catch (Exception)
            {
            }
            return 0;
        }


        public double GetScalarDouble(string sql, object vCol, bool bLog = true)
        {
            DataTable dt1 = GetDataTable(sql, bLog);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    object oOut = null;
                    if (vCol.GetType().ToString() == "System.String")
                    {
                        oOut = dt1.Rows[0][vCol.ToString()];
                    }
                    else
                    {
                        oOut = dt1.Rows[0][Convert.ToInt32(vCol)];
                    }
                    double dOut = GetDouble(oOut.ToString());

                    return dOut;
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }


        public DataRow GetScalarRow(SqlCommand c)
        {
            DataRow dRow;

            DataTable dt1 = GetDataTable(c);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    dRow = dt1.Rows[0];
                    return dRow;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
        public DataRow GetScalarRow(string sql)
        {
            DataRow dRow;

            DataTable dt1 = GetDataTable(sql);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    dRow = dt1.Rows[0];
                    return dRow;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public string GetScalarString(SqlCommand sqlCommand, object vCol, bool bLog = true)
        {
            DataTable dt1 = GetDataTable(sqlCommand);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    object oOut = null;
                    if (vCol.GetType().ToString() == "System.String")
                    {
                        oOut = dt1.Rows[0][vCol.ToString()];
                    }
                    else
                    {
                        oOut = dt1.Rows[0][Convert.ToInt32(vCol)];
                    }
                    return oOut.ToString();
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        public string GetScalarString(string sql, object vCol, bool bLog = true)
        {
            DataTable dt1 = GetDataTable(sql);
            try
            {
                if (dt1.Rows.Count > 0)
                {
                    object oOut = null;
                    if (vCol.GetType().ToString() == "System.String")
                    {
                        oOut = dt1.Rows[0][vCol.ToString()];
                    }
                    else
                    {
                        oOut = dt1.Rows[0][Convert.ToInt32(vCol)];
                    }
                    return oOut.ToString();
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        public SqlDataReader GetDataReader(string sql)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    con.Open();
                    SqlDataReader myReader = default(SqlDataReader);
                    SqlCommand myCommand = new SqlCommand(sql, con);
                    myReader = myCommand.ExecuteReader();
                    return myReader;
                }
            }
            catch (Exception ex)
            {
                Log("GetDataReader:" + ex.Message + "," + sql);
            }
            SqlDataReader dr = default(SqlDataReader);
            return dr;
        }

        public string ReadFirstRow(string sql, object vCol, bool bLog = true)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {

                        SqlDataReader dr = cmd.ExecuteReader();
                        if (!dr.HasRows | dr.FieldCount == 0) return string.Empty;
                        while (dr.Read())
                        {
                            if (vCol is String)
                            {
                                return dr[(string)vCol].ToString();
                            }
                            else
                            {
                                return dr[(int)vCol].ToString();
                            }
                        }
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log("Readfirstrow: " + sql + ", " + ex.Message);
            }
            return "";
        }

    }
}
