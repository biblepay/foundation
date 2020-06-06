using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Saved.Code.Common;

namespace Saved.Code
{

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
            catch(Exception ex)
            {
                // No need to spam the satellite pool(s):
                if (false)
                    Log("GSS:" + ex.Message);
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

        public DataTable GetDataTable(SqlCommand sqlCommand, bool bLog = true)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(sSQLConn()))
                {
                    int nConnectionTimeout = con.ConnectionTimeout;
                    con.Open();
                    sqlCommand.Connection = con;
                    SqlDataAdapter a = new SqlDataAdapter(sqlCommand);
                    DataTable t = new DataTable();
                    a.Fill(t);
                    return t;
                }
            }
            catch (Exception ex)
            {
                Log("GetDataTableViaSqlCommand:" + sqlCommand.CommandText + "," + ex.Message);
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
