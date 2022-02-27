using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.Common;

namespace Saved.Code
{
    public static class PoolCommon
    {
        public static Dictionary<string, WorkerInfo> dictWorker = new Dictionary<string, WorkerInfo>();
        public static Dictionary<string, WorkerInfo> dictBan = new Dictionary<string, WorkerInfo>();
        public static Dictionary<string, XMRJob> dictJobs = new Dictionary<string, XMRJob>();
        public static List<string> listBoughtNFT = new List<string>();

        public static int iThreadCount = 0;
        public static int nGlobalHeight = 0;
        public static int pool_version = 1020;
        public static int iXMRThreadID = 0;
        public static double iXMRThreadCount = 0;
        public static int iTitheNumber = 0;
        public static int iTitheModulus = 0;
        public static int BXMRC = 0;
        public static int SHARES = 0;
        public static List<SqlCommand> lSQL = new List<SqlCommand>();
        public static DateTime start_date = DateTime.Now;
        public static int MIN_DIFF = 1;
        public static object cs_p = new object();
        public static bool fMonero2000 = true;

        public struct XMRJob
        {
            public string blob;
            public double jobid0;
            public string socketid;
            public int timestamp;
            public string target;
            public double difficulty;
            public string seed;
            public string solution;
            public string nonce;
            public string hash;
            public string hashreversed;
            public string bbpaddress;
            public string moneroaddress;
            public bool fNeedsSubmitted;
        }

        public static double GetTithePercent()
        {
            double nPerc = Math.Round((PoolCommon.BXMRC / (PoolCommon.SHARES + .01)) * 100, 4);
            return nPerc;
        }
        public static void SetWorker(WorkerInfo worker, string sKey)
        {
            try
            {
                if (!dictWorker.ContainsKey(sKey))
                {
                    worker = GetWorker(sKey);
                }
                dictWorker[sKey] = worker;
            }
            catch (Exception ex)
            {
                Log("SetWorker" + ex.Message);
            }
        }
        public static WorkerInfo GetWorker(string socketid)
        {
            try
            {
                WorkerInfo w = new WorkerInfo();
                if (!dictWorker.ContainsKey(socketid))
                {
                    w.receivedtime = UnixTimeStamp();
                    dictWorker[socketid] = w;
                }
                w = dictWorker[socketid];
                return w;
            }
            catch (Exception)
            {
                // This is not supposed to happen, but I see this error in the log... 
                WorkerInfo w = new WorkerInfo();
                SetWorker(w, socketid);
                return w;
            }
        }

        public static string GetPoolValue(string sKey)
        {
            string sql = "Select value from System where systemkey='" + BMS.PurifySQL(sKey,30) + "'";
            string value = gData.GetScalarString2(sql, "value", false);
            return value;
        }
        public static void RemoveWorker(string socketid)
        {
            try
            {
                dictWorker.Remove(socketid);
            }
            catch (Exception ex)
            {
                Log("Rem w" + ex.Message);
            }
        }
        public static WorkerInfo GetWorkerBan(string socketid)
        {
            WorkerInfo w = new WorkerInfo();
            if (!dictBan.ContainsKey(socketid))
            {
                w.receivedtime = UnixTimeStamp();
                dictBan[socketid] = w;
            }
            w = dictBan[socketid];
            w.lastreceived = w.receivedtime;
            w.receivedtime = UnixTimeStamp();
            dictBan[socketid] = w;
            return w;
        }
        public static void CloseSocket(Socket c)
        {
            try
            {
                c.Close();
            }
            catch (Exception)
            {

            }
        }

        public static void insBanDetails(string IP, string sWHY, double iLevel)
        {
            if (!fUseBanTable) return;

            try
            {
                string sql = "Insert into BanDetails (id,IP,Notes,Added,Level) values (newid(), '" + IP + "','" + sWHY + "',getdate(),'" + iLevel.ToString() + "')";
                gData.Exec(sql);
            }
            catch (Exception x)
            {
                string test = x.Message;
            }
        }

        private static int BAN_THRESHHOLD = 356;
        public static WorkerInfo Ban(string socketid, double iHowMuch, string sWhy)
        {
            string sKey = GetIPOnly(socketid);
            bool fIsBanned = lBanList.Contains(sKey);
            WorkerInfo w = GetWorkerBan(sKey);


            w.banlevel += iHowMuch;
            if (w.banlevel > BAN_THRESHHOLD)
            {
                if (!w.logged)
                {
                    //Log("Banned " + GetIPOnly(socketid));
                    w.logged = true;
                }
                w.banlevel = BAN_THRESHHOLD + 1;
            }
            if (fIsBanned)
            {
                w.banlevel = 512;
            }
            if (w.banlevel < 0)
                w.banlevel = 0;
            w.banned = w.banlevel >= BAN_THRESHHOLD ? true : false;
            dictBan[sKey] = w;
            if (w.banlevel > 0 && (w.banlevel < 10 || w.banlevel % 10 == 0))
            {
                insBanDetails(sKey, sWhy, w.banlevel);
            }
            return w;
        }

        public static string GetIPOnly(string fullendpoint)
        {
            string[] vData = fullendpoint.Split(":");
            if (vData.Length > 1)
            {
                return vData[0];
            }
            return fullendpoint;
        }

        private static ReaderWriterLockSlim dictLock = new ReaderWriterLockSlim();
        public static XMRJob RetrieveXMRJob(string socketid)
        {
            try
            {
                dictLock.EnterReadLock();
                if (dictJobs.ContainsKey(socketid))
                {
                    return dictJobs[socketid];
                }
                XMRJob x = new XMRJob();
                x.timestamp = UnixTimeStamp();
                x.socketid = socketid;
                return x;
            }
            finally
            {
                dictLock.ExitReadLock();
            }
        }

        public static void PutXMRJob(XMRJob x)
        {
            if (x.socketid == "")
                return;
            dictLock.EnterWriteLock();
            try
            {
                dictJobs[x.socketid] = x;
            }
            finally
            {
                dictLock.ExitWriteLock();
            }
        }

        public static void PurgeJobs()
        {
            lock (cs_stratum)
            {

                try
                {
                    dictLock.EnterWriteLock();

                    foreach (KeyValuePair<string, XMRJob> entry in dictJobs.ToArray())
                    {
                        if (dictJobs.ContainsKey(entry.Key))
                        {
                            XMRJob w1 = dictJobs[entry.Key];
                            int nElapsed = UnixTimeStamp() - w1.timestamp;
                            bool fRemove = (nElapsed > (60 * 15) && w1.timestamp > 0);
                            if (fRemove)
                            {
                                try
                                {
                                    dictJobs.Remove(entry.Key);
                                }
                                catch (Exception ex)
                                {
                                    Log("PJ " + ex.Message);
                                }
                            }
                        }
                        if (dictJobs.Count < 1000)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Log("Purge Jobs: " + ex.Message);
                }
                finally
                {
                    dictLock.ExitWriteLock();
                }
            }
        }


        public static void PurgeSockets(bool fClearBans)
        {
            try
            {
                foreach (KeyValuePair<string, WorkerInfo> entry in dictWorker.ToArray())
                {
                    if (entry.Key != null)
                    {
                        if (dictWorker.ContainsKey(entry.Key))
                        {
                            WorkerInfo w1 = GetWorker(entry.Key);
                            int nElapsed = UnixTimeStamp() - w1.receivedtime;
                            bool fRemove = false;
                            fRemove = (nElapsed > (60 * 15) && w1.receivedtime > 0)
                                || (nElapsed > (60 * 15) && (w1.bbpaddress == null || w1.bbpaddress == ""));

                            if (fClearBans)
                            {
                                w1.banlevel = 0;
                                SetWorker(w1, entry.Key);
                            }

                            if (fRemove)
                            {
                                RemoveWorker(entry.Key);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Purge Sockets: " + ex.Message);
            }
        }


        public static void InsShare(string bbpaddress, double nShareAdj, double nFailAdj, int height, double nBXMR, double nBXMRC, string moneroaddress)
        {
            string sql = "exec insShare @bbpid,@shareAdj,@failAdj,@height,@sxmr,@fxmr,@sxmrc,@fxmrc,@bxmr,@bxmrc";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@bbpid", bbpaddress);
            command.Parameters.AddWithValue("@shareAdj", nShareAdj);
            command.Parameters.AddWithValue("@failAdj", nFailAdj);
            command.Parameters.AddWithValue("@height", height);
            command.Parameters.AddWithValue("@sxmr", 0);
            command.Parameters.AddWithValue("@fxmr", 0);
            command.Parameters.AddWithValue("@sxmrc", 0);
            command.Parameters.AddWithValue("@fxmrc", 0);
            command.Parameters.AddWithValue("@bxmr", nBXMR);
            command.Parameters.AddWithValue("@bxmrc", nBXMRC);
            if (bbpaddress == "" || height == 0)
            {
                if (moneroaddress == GetBMSConfigurationKeyValue("moneroaddress"))
                    return;
                return;
            }
            try
            {
                lSQL.Add(command);
            }
            catch (Exception ex)
            {

                Log("insShare: " + ex.Message);
            }
        }

        public static string GetBBPAddress(string sMoneroAddress)
        {
            try
            {

                foreach (KeyValuePair<string, WorkerInfo> item in dictWorker.ToArray())
                {
                    if (item.Value.moneroaddress == sMoneroAddress && item.Value.bbpaddress.Length > 10)
                    {
                        return item.Value.bbpaddress;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("FMB: " + ex.Message);
            }
            // Retrieve from the database instead
            string sql = "Select bbpaddress from worker (nolock) where moneroaddress='" + BMS.PurifySQL(sMoneroAddress,100) + "'";
            string bbp = gData.GetScalarString2(sql, "bbpaddress");
            return bbp;
        }


        public static void MarkForBroadcast()
        {
            try
            {
                foreach (KeyValuePair<string, WorkerInfo> entry in dictWorker.ToArray())
                {
                    if (dictWorker.ContainsKey(entry.Key))
                    {
                        WorkerInfo w1 = GetWorker(entry.Key);
                        w1.Broadcast = true;
                        SetWorker(w1, entry.Key);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void IncThreadCount(int iHowMuch)
        {
            iThreadCount += iHowMuch;
            try
            {
                if (iThreadCount > dictWorker.Count * 2)
                {
                    iThreadCount = dictWorker.Count;
                }
                if (iThreadCount < 0)
                    iThreadCount = 0;
                if (iThreadCount < dictWorker.Count)
                    iThreadCount = dictWorker.Count;
            }
            catch (Exception ex)
            {
                Log("IncThreadCount: " + ex.Message);
            }
        }

        public static double FullTest(byte[] h)
        {
            // Converts the RandomX solution hash over to the original bitcoin difficulty level
            UInt64 nAdjHash = BitConverter.ToUInt64(h, 24);
            double nDiff = 0xFFFFFFFFFFFFUL / (nAdjHash + .01);
            return nDiff;
        }

        public static string GetChartOfSancs()
        {
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.White;
            System.Drawing.Color primaryColor = System.Drawing.Color.Brown;
            System.Drawing.Color textColor = System.Drawing.Color.Black;
            c.Width = 1500;
            //c.Height = 1000;

            string sChartName = "Number of Sanctuaries vs Monthly Reward";
            Series s = new Series(sChartName);
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;

            s.LabelForeColor = textColor;
            s.Color = primaryColor;
            s.BackSecondaryColor = bg;
            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;
            c.Series.Add(s);

            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;
            c.Legends.Add("Monthly Reward");

            for (double iSancs = 20; iSancs < 104; iSancs += 1)
            {
                double dRevenue = (205 / iSancs) * 3700 * 30.01; //3700 = reward per block currently
                s.Points.AddXY(iSancs, dRevenue);
            }
            //c.ChartAreas[0].AxisY.ScaleBreakStyle.Spacing = 2;
            //c.ChartAreas[0].AxisX.ScaleBreakStyle.Spacing = 2;
            //c.ChartAreas[0].AxisX.ScaleBreakStyle.
            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;

            c.BackColor = bg;
            c.ForeColor = primaryColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/sanctuaries.png");
            c.SaveImage(sSan);
            return sSan;

        }



        public static string GetChartOfHashRate()
        {
            int nMax = _pool._template.height - 1;

            int nMin = nMax - 205;

            string sql = "select  HashRate,Height From HashRate where height > "
                + nMin.ToString() + " and height < " + nMax.ToString() + " order by height";

            DataTable dt = gData.GetDataTable2(sql, false);
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.White;
            System.Drawing.Color primaryColor = System.Drawing.Color.Blue;
            System.Drawing.Color textColor = System.Drawing.Color.Black;
            c.Width = 1500;
            string sChartName = "Hashrate over 24 hours";
            Series s = new Series(sChartName);
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;

            s.LabelForeColor = textColor;
            s.Color = primaryColor;
            s.BackSecondaryColor = bg;
            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;
            c.Series.Add(s);

            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;

            for (int i = 0; i < dt.Rows.Count; i += 1)
            {
                double Height = GetDouble(dt.Rows[i]["height"]);
                double dR = GetDouble(dt.Rows[i]["hashrate"]);

                s.Points.AddXY(Height, dR);
            }

            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;

            c.BackColor = bg;
            c.ForeColor = primaryColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/hashrate.png");
            c.SaveImage(sSan);
            return sSan;

        }


        public static string GetChartOfWorkers()
        {
            int nMax = _pool._template.height - 1;
            int nMin = nMax - 205;

            string sql = "select minercount, height From HashRate Where height > " + nMin.ToString() + " and height < " + nMax.ToString() + " order by height";
            DataTable dt = gData.GetDataTable2(sql, false);
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.White;
            System.Drawing.Color primaryColor = System.Drawing.Color.Blue;
            System.Drawing.Color textColor = System.Drawing.Color.Black;

            c.Width = 1500;

            string sChartName = "Workers over 24 hours";
            Series s = new Series(sChartName);
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;
            s.LabelForeColor = textColor;

            s.Color = primaryColor;

            s.BackSecondaryColor = bg;

            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;
            c.Series.Add(s);

            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;
            for (int i = 0; i < dt.Rows.Count; i += 1)
            {
                double Height = GetDouble(dt.Rows[i]["height"]);
                double dWorkers = GetDouble(dt.Rows[i]["MinerCount"]);
                string sNarr = Height.ToString();
                s.Points.AddXY(Height, dWorkers);

            }

            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;
            c.BackColor = bg;
            c.ForeColor = textColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/workers.png");
            c.SaveImage(sSan);
            return sSan;
        }

        public static string GetChartOfBlocks()
        {
            int nMax = _pool._template.height - 1;
            int nMin = nMax - 205;

            string sql = "select SolvedCount, height From HashRate Where height > " + nMin.ToString() + " and height < " + nMax.ToString() + " order by height";
            DataTable dt = gData.GetDataTable2(sql, false);
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.White;
            System.Drawing.Color primaryColor = System.Drawing.Color.Blue;
            System.Drawing.Color textColor = System.Drawing.Color.Black;

            c.Width = 1500;

            string sChartName = "Blocks Solved over 24 hours";
            Series s = new Series(sChartName);
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.StackedArea;
            s.LabelForeColor = textColor;

            s.Color = primaryColor;

            s.BackSecondaryColor = bg;

            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;
            c.Series.Add(s);

            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;
            for (int i = 0; i < dt.Rows.Count; i += 1)
            {
                double Height = GetDouble(dt.Rows[i]["height"]);
                double dWorkers = GetDouble(dt.Rows[i]["SolvedCount"]);
                string sNarr = Height.ToString();
                s.Points.AddXY(Height, dWorkers);

            }

            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;
            c.BackColor = bg;
            c.ForeColor = textColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/blockssolved.png");
            c.SaveImage(sSan);
            return sSan;
        }

        public struct Payment
        {
            public string bbpaddress;
            public double amount;
        }

        public struct WorkerInfo
        {
            public string bbpaddress;
            public string moneroaddress;
            public int difficulty;
            public int nextdifficulty;
            public int height;
            public int jobid;
            public int updated;
            public bool Broadcast;
            public int receivedtime;
            public int lastreceived;
            public string IP;
            public bool reset;
            public int solvetime;
            public int priorsolvetime;
            public double banlevel;
            public int starttime;
            public bool logged;
            public bool banned;
        }
        public struct BlockTemplate
        {
            public string hex;
            public string curtime;
            public string prevhash;
            public string prevblocktime;
            public string bits;
            public string target;
            public int height;
            public int updated;
        }


        public static bool ValidateBiblepayAddress(bool fTestNet, string sAddress)
        {
            try
            {
                object[] oParams = new object[1];
                oParams[0] = sAddress;
                NBitcoin.RPC.RPCClient n = fTestNet ? WebRPC.GetTestNetRPCClient() : WebRPC.GetLocalRPCClient();

                dynamic oOut = n.SendCommand("validateaddress", oParams);
                string sResult = oOut.Result["isvalid"].ToString();
                if (sResult.ToLower().Contains("true")) return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static string SendMany(List<Payment> p, string sFromAccount, string sComment)
        {
            string sPack = "";
            for (int i = 0; i < p.Count; i++)
            {
                string sAmount = string.Format("{0:#.00}", p[i].amount);
                string sRowOld = "\"" + p[i].bbpaddress + "\"" + ":" + sAmount;
                string sRow = "<RECIPIENT>" + p[i].bbpaddress + "</RECIPIENT><AMOUNT>" + sAmount + "</AMOUNT><ROW>";
                sPack += sRow;
            }

            string sXML = "<RECIPIENTS>" + sPack + "</RECIPIENTS>";

            try
            {

                object[] oParams = new object[4];
                oParams[0] = "sendmanyxml";
                oParams[1] = sFromAccount;
                oParams[2] = sXML;
                oParams[3] = sComment;
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

                dynamic oOut = n.SendCommand("exec", oParams);
                string sTX = oOut.Result["txid"].ToString();
                return sTX;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
                Log(" Error while transmitting : " + ex.Message);
                return "";
            }
        }

        public static void PaySanctuaryInvestors()
        {

            double nOrphanSancPct = GetOrphanFracSancPercentage();

            try
            {
                string sql = "SELECT   amount, height, txid, address, id      FROM       sanctuaryPayment      WHERE paid is null and amount > 100";
                DataTable dt = gData.GetDataTable2(sql, false);
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string sql2 = "Select * From SanctuaryInvestments Where amount > 10 order by Amount";
                    double nSancReward = GetDouble(dt.Rows[i]["Amount"]);
                    double nOrphanCharges = nSancReward * nOrphanSancPct;
                    double nNetSancReward = nSancReward - nOrphanCharges;
                    double nHeight = GetDouble(dt.Rows[i]["height"]);
                    string sId = dt.Rows[i]["id"].ToString();
                    string sAddress = dt.Rows[i]["address"].ToString();
                    string sTXID = dt.Rows[i]["txid"].ToString();

                    DataTable dt2 = gData.GetDataTable2(sql2, false);
                    for (int j = 0; j < dt2.Rows.Count; j++)
                    {
                        string sql3 = "Insert Into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), @address, @txid, @userid, getdate(), @amount, @height, @notes)";
                        SqlCommand command = new SqlCommand(sql3);
                        command.Parameters.AddWithValue("@address", sAddress);
                        command.Parameters.AddWithValue("@txid", sTXID + "-" + j.ToString());
                        command.Parameters.AddWithValue("@userid", dt2.Rows[j]["userid"]);
                        double nAmount = GetDouble(dt2.Rows[j]["amount"]);
                        if (nAmount > .25)
                        {
                            //8-12-2020  POOS 
                            double nReward = nAmount / 4500000 * nNetSancReward;
                            command.Parameters.AddWithValue("@amount", Math.Round(nReward, 2));
                            command.Parameters.AddWithValue("@height", nHeight);
                            string sNarr = "Sanctuary Payment [for " + Math.Round(nAmount, 2).ToString() + " (Orphan Rate=" + Math.Round(nOrphanSancPct, 2) + "%]";
                            command.Parameters.AddWithValue("@notes", sNarr);
                            gData.ExecCmd(command, false, false, false);
                        }
                    }

                    string sql4 = "Update SanctuaryPayment set Paid = getdate() where id = @id";
                    SqlCommand command2 = new SqlCommand(sql4);
                    command2.Parameters.AddWithValue("@id", sId);
                    gData.ExecCmd(command2, false, false, false);

                }
            }
            catch (Exception ex)
            {
                Log("Pay sanctuary investors " + ex.Message);
            }
        }

        public static void clearbans()
        {
            // Clear banned pool users
            try
            {
                dictBan.Clear();
                //Memorize the excess banlist
                string sql = "Select distinct dbo.iponly(ip) ip from Worker where bbpaddress in (select bbpaddress from leaderboard where efficiency < .20) UNION ALL Select IP from Bans";
                DataTable dt = gData.GetDataTable2(sql);
                lBanList.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["ip"].ToString().Length > 1)
                        lBanList.Add(dt.Rows[i]["ip"].ToString());
                }
            }
            catch (Exception ex)
            {
                Log("Clearing ban " + ex.Message);
            }
        }

        static int nLastMailed = 0;
        public static bool MailOut()
        {
            int nElapsed = UnixTimeStamp() - nLastMailed;
            if (nElapsed < (60 * 60 * 12))
                return false;
            nLastMailed = UnixTimeStamp();
            try
            {
                UICommon.SendMassDailyTweetReport();
            }
            catch (Exception ex)
            {
                Log("Mail out " + ex.Message);
            }
            return true;
        }




        static int nLastPaid = Common.UnixTimeStamp();
        public static bool Pay()
        {
            int nElapsed = UnixTimeStamp() - nLastPaid;
            if (nElapsed < (60 * 60 * 8))
                return false;
            nLastPaid = UnixTimeStamp();
            try
            {
                if (GetBMSConfigurationKeyValue("satellitepool") != "1")
                {
                    GetSancTXIDList();
                    PaySanctuaryInvestors();
                    MailOut();
                    Saved.Code.WebServices.PayVideos("");
                    UserActivityRewards();
                    StoreQuotes(0);
                    GetChartOfIndex();
                }

                RecordParticipants();
                randomxhashes.Clear();
                clearbans();

            }
            catch (Exception ex2)
            {
                Log("GetSancTXIDList: " + ex2.Message);
            }
            try
            {

                // Create a batchid
                string batchid = Guid.NewGuid().ToString();
                double nMaturityDuration = GetDouble(GetBMSConfigurationKeyValue("maturityduration")); // this is a float with a duration in days
                if (nMaturityDuration == 0)
                    nMaturityDuration = .20;

                string sql = "Update share set txid=@batchid where Paid is null and subsidy > 1 and updated < getdate() - " + nMaturityDuration.ToString();
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@batchid", batchid);
                gData.ExecCmd(command, false, false, false);
                sql = "Select bbpaddress, sum(Reward) reward from Share where txid = @batchid and paid is null group by bbpaddress";
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@batchid", batchid);

                DataTable dt = gData.GetDataTable(command, false);
                List<Payment> Payments = new List<Payment>();
                double nTotal = 0;
                double nMinPaymentThreshhold = GetDouble(GetBMSConfigurationKeyValue("minimumpaymentthreshhold"));
                if (nMinPaymentThreshhold == 0)
                    nMinPaymentThreshhold = .01;
                
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string address = dt.Rows[i]["bbpaddress"].ToString();
                    double nReward = GetDouble(dt.Rows[i]["Reward"]);

                    bool bValid = ValidateBiblepayAddress(false,address);

                    if (bValid && nReward > nMinPaymentThreshhold)
                    {
                        nTotal += nReward;
                        Payment p = new Payment();
                        p.bbpaddress = address;
                        p.amount = nReward;
                        Payments.Add(p);
                    }
                }

                string poolAccount = GetBMSConfigurationKeyValue("PoolPayAccount");
                if (poolAccount == "")
                {
                    Log("Distress:  Unable to pay workers because pool account is not set.  Set [PoolPayAccount=poolname] in bms.conf.  Where poolname is the name of the address book entry receiving the rewards. ");

                }

                if (Payments.Count > 0)
                {
                    string txid = SendMany(Payments, poolAccount, "PoolPayments " + _pool._template.height.ToString());

                    // send

                    if (txid != "")
                    {
                        sql = "Update share set paid = getdate(), txid = @txid where txid = @batchid";
                        command = new SqlCommand(sql);

                        command.Parameters.AddWithValue("@batchid", batchid);
                        command.Parameters.AddWithValue("@txid", txid);

                        gData.ExecCmd(command, false, false, false);
                        return true;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                Log("PayPool: " + ex.Message);
            }

            return false;
        }

        public struct NFT
        {
            public string Name;
            public string Description;
            public string LoQualityURL;
            public string HiQualityURL;
            public double nMinimumBidAmount;
            public double nReserveAmount;
            public double nBuyItNowAmount;
            public double nIteration;
            public double nLowestAcceptableAmount;
            public bool fMarketable;
            public bool fDeleted;
            public string CPK;
            public string Hash;
            public bool found;
            public string Type;
        };

        public static dynamic GetStatement(string sBusinessAddress, string sCustomerAddress, int nStartTime, int nEndTime)
        {
            try
            {
                NBitcoin.RPC.RPCClient n = WebRPC.GetTestNetRPCClient();
                object[] oParams = new object[4];
                oParams[0] = sBusinessAddress;
                oParams[1] = sCustomerAddress;
                oParams[2] = nStartTime.ToString();
                oParams[3] = nEndTime.ToString();
                dynamic oOut = n.SendCommand("getstatement", oParams);
                return oOut;
            }
            catch (Exception x)
            {
                Log("Unable to get statement " + x.Message);
            }
            dynamic xz = null;
            return xz;
        }

        public static bool InList(string sTypes, string sType)
        {
            if (sTypes == "all")
            {
                return true;
            }
            string[] vTypes = sTypes.Split(",");
            for (int i = 0; i < vTypes.Length; i++)
            {
                if (vTypes[i] == sType)
                    return true;
            }
            return false;
        }
        public static string GV(dynamic o) 
        {
            try
            {
                if (o == null)
                    return "";
                string sStr = o.Value.ToString() ?? "";
                if (sStr == null)
                    return "";
                return sStr;
            }
            catch(Exception)
            {
                return "";
            }
        }
        public static bool GVB(dynamic o)
        {
            string data = GV(o);
            if (data.ToLower() == "true" || data == "1")
                return true;
            return false;
        }


        public static string SerializeNFT(string sHWID, string sNFTID, string sAction)
        {
            Code.Fastly.KeyType k = Code.Fastly.DeriveRokuKeypair(sHWID);
            string sBuyerCPK = k.PubKey;
            bool fTestNet = !IsProdChain(k.PubKey);
            Code.PoolCommon.NFT myNFT = GetSpecificNFT(sNFTID, fTestNet);
            string sPK = "NFT-" + myNFT.Hash;
            string sSignature = ""; //No need for a buyer to have one
            string sPayload = "<MT>NFT</MT><MK>" + sPK + "</MK><MV><nft><cpk>" + sBuyerCPK + "</cpk><name>"
                + myNFT.Name + "</name><description>" + myNFT.Description + "</description><loqualityurl>"
                + myNFT.LoQualityURL + "</loqualityurl><hiqualityurl>" + myNFT.LoQualityURL
                + "</hiqualityurl><deleted>" + (myNFT.fDeleted ? "1" : "0") + "</deleted><marketable>0</marketable><time>"
                + UnixTimeStamp().ToString()
                + "</time><type>" + myNFT.Type + "</type><iteration>"
                + (myNFT.nIteration + 1).ToString() + "</iteration><minbidamount>0</minbidamount>"
                + "<reserveamount>" + myNFT.nReserveAmount.ToString() + "</reserveamount><buyitnowamount>"
                + myNFT.nBuyItNowAmount.ToString() + "</buyitnowamount><lastcpk>" + myNFT.CPK + "</lastcpk>"
                + "</nft><BOACTION>" + sAction + "</BOACTION><BOSIGNER>" + sBuyerCPK + "</BOSIGNER><BOSIG>"
                + sSignature + "</BOSIG><BOMSG>" + myNFT.Hash + "</BOMSG></MV>";
            sPayload = sPayload.Replace("\"", "");
            return sPayload;
        }
        public static List<NFT> GetNFTList(string sTypes, bool fTestNet, string sCPKOnly)
        {

            string sql = "Select NFTID from nftblacklist";
            DataTable dt1 = gData.GetDataTable2(sql, false);
            List<string> lIDs = new List<string>();
            for (int i = 0; i < dt1.Rows.Count; i++)
            {
                lIDs.Add(dt1.Rows[i]["nftid"].ToString());
            }
            // Type = Orphan or Digital Goods
            List<NFT> lNFTs= new List<NFT>();
            try
            {
                NBitcoin.RPC.RPCClient n = fTestNet ? WebRPC.GetTestNetRPCClient() : WebRPC.GetLocalRPCClient();

                object[] oParams = new object[2];
                oParams[0] = "1";
                oParams[1] = "1";
                dynamic oOut = n.SendCommand("listnfts", oParams);
                // Loop Through the Vouts and get the recip ids and the amounts
                foreach (var jNFT in oOut.Result)
                {
                    NFT n1 = new NFT();
                    n1.Name = GV(jNFT.Value["Name"]);
                    n1.Type = GV(jNFT.Value["Type"]);
                    n1.Type = n1.Type.ToLower();
                    bool fOrphan = n1.Type.Contains("orphan");

                    n1.Description = GV(jNFT.Value["Description"]);
                    n1.nMinimumBidAmount = GetDouble(GV(jNFT.Value["MinimumBidAmount"]));
                    n1.nReserveAmount = GetDouble(GV(jNFT.Value["ReserveAmount"]));
                    n1.nBuyItNowAmount = fOrphan ? GetDouble(GV(jNFT.Value["SponsorshipAmount"])) : GetDouble(GV(jNFT.Value["BuyItNowAmount"]));
                    n1.nIteration = GetDouble(GV(jNFT.Value["Iteration"]));
                    n1.CPK = GV(jNFT.Value["CPK"]);

                    n1.nLowestAcceptableAmount = GetDouble(GV(jNFT.Value["LowestAcceptableAmount"]));
                    n1.Hash = GV(jNFT.Value["Hash"]);
                    n1.found = true;
                    n1.fMarketable = GVB(jNFT.Value["Marketable"]);
                    if (fOrphan)
                        n1.fMarketable = GVB(jNFT.Value["Sponsorable"]);

                    n1.fDeleted = GVB(jNFT.Value["Deleted"]);
                    n1.LoQualityURL = GV(jNFT.Value["Lo Quality URL"]);
                    n1.HiQualityURL = GV(jNFT.Value["Hi Quality URL"]);

                    bool fTest = n1.Name.ToLower().Contains("test");

                    string[] vType = n1.Type.Split(" ");

                    string sShortID = n1.Hash.Substring(0, 8);
                    if (lIDs.Contains(sShortID) || lIDs.Contains(n1.Hash.ToString()))
                    {
                        n1.fMarketable = false;
                    }

                    if ((sCPKOnly != "" && n1.CPK == sCPKOnly) || (sTypes == "all"))
                    {
                        lNFTs.Add(n1);
                    }
                    if (sCPKOnly == "" && !fTest && n1.fMarketable && n1.nLowestAcceptableAmount > 0)
                    {
                        if (InList(sTypes, vType[0]))
                        {
                            if (!listBoughtNFT.Contains(n1.Hash))
                            {
                                lNFTs.Add(n1);
                            }
                            else
                            {
                                if (!n1.fMarketable)
                                {
                                    // The nft is in the bought list but is now not marketable, so remove it from the bought list
                                    listBoughtNFT.Remove(n1.Hash);
                                }
                            }
                        }
                    }
                }
                
                return lNFTs;
            }
            catch (Exception ex)
            {
                Log("GetNFTList: " + ex.Message);
                return lNFTs;
            }
        }

        public static string BuyNFT(string hash, string sNewBuyerCPK, double nTotal, bool fTestNet)
        {
            try
            {
                NBitcoin.RPC.RPCClient n = fTestNet ? WebRPC.GetTestNetRPCClient() : WebRPC.GetLocalRPCClient();
                object[] oParams = new object[3];
                oParams[0] = sNewBuyerCPK;
                oParams[1] = hash;
                oParams[2] = nTotal.ToString();
                dynamic oOut = n.SendCommand("buynft", oParams);
                if (oOut.Result["Result"] == null)
                {
                    string sErr = oOut.Result["Error"].Value;
                    return sErr;
                }

                string sOut = oOut.Result["Result"].Value;
                if (sOut == "Success")
                {
                    return "";
                }
                return sOut;
            }
            catch(Exception ex)
            {
                Log("buynft error " + ex.Message);
                return ex.Message;
            }
        }
        
        public static string GetRawTransaction(string sTxid, bool fTestNet)
        {
            try
            {
                NBitcoin.RPC.RPCClient n;
                if (fTestNet)
                {
                    n = WebRPC.GetTestNetRPCClient();
                }
                else
                {
                    n = WebRPC.GetLocalRPCClient();
                }
                object[] oParams = new object[2];
                oParams[0] = sTxid;
                oParams[1] = 1;
                //Log("Connected to " + n.Address.OriginalString);

                dynamic oOut = n.SendCommand("getrawtransaction", oParams);
                // Loop Through the Vouts and get the recip ids and the amounts
                string sOut = "";
                double locktime = oOut.Result["locktime"] == null ? 0 : GetDouble(oOut.Result["locktime"].ToString());
                double height1 = oOut.Result["height"] == null ? 0 : GetDouble(oOut.Result["height"].ToString());

                double height = 0;
                height = height1 > 0 ? height1 : locktime;

               
                for (int y = 0; y < oOut.Result["vout"].Count; y++)
                {
                    string sPtr = "";
              
                    try
                    {
                        sPtr = (oOut.Result["vout"][y] ?? "").ToString();
                    }
                    catch (Exception ey)
                    {

                        Log("Strange error in GetRawTransaction=" + ey.Message);
                        
                    }

                    if (sPtr != "")
                    {
                        string sAmount = oOut.Result["vout"][y]["value"].ToString();
                        string sAddress = "";
                        if (oOut.Result["vout"][y]["scriptPubKey"]["addresses"] != null)
                        {
                            sAddress = oOut.Result["vout"][y]["scriptPubKey"]["addresses"][0].ToString();
                        }
                        else 
                        { 
                            sAddress = "?";
                        } //Happens when pool pays itself
                        sOut += sAmount + "," + sAddress + "," + height + "|";
                    }
                    else
                    {
                        break;
                    }
                }
                return sOut;
            }
            //Harvest Mission Critical todo:  Pass back the instant send lock bool here as an object!

            catch (Exception ex)
            {
                Log("GetRawTransaction1: for " + sTxid + " " + ex.Message);
                return "";
            }
        }

        public static double GetAmtFromRawTx(string sRaw, string sAddress, out int nHeight)
        {
            string[] vData = sRaw.Split(new string[] { "|" }, StringSplitOptions.None);
            for (int i = 0; i < vData.Length; i++)
            {
                string d = vData[i];
                if (d.Length > 1)
                {
                    string[] vRow = d.Split(new string[] { "," }, StringSplitOptions.None);
                    if (vRow.Length > 1)
                    {
                        string sAddr = vRow[1];
                        string sAmt = vRow[0];
                        string sHeight = vRow[2];
                        nHeight = (int)GetDouble(sHeight);

                        if (sAddr == sAddress && nHeight > 0)
                        {
                            return Convert.ToDouble(sAmt);
                        }

                    }
                }
            }
            nHeight = 0;
            return 0;
        }


        public static void ScanForSanctuaryPayments()
        {
            string sql = "Select * from SanctuaryPayment where Amount is null";
            DataTable d1 = gData.GetDataTable2(sql, false);
            NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string id = d1.Rows[i]["id"].ToString();
                string address = d1.Rows[i]["Address"].ToString();
                string sTxId = d1.Rows[i]["txid"].ToString();

                string sRawTx = GetRawTransaction(sTxId, false);
                int nHeight = 0;

                double amt = GetAmtFromRawTx(sRawTx, address, out nHeight);

                double depth = _pool._template.height - nHeight;


                if (amt > 0 && depth > 2)
                {
                    sql = "Update SanctuaryPayment set Amount = '" + amt.ToString() + "',height='" + nHeight.ToString() + "' where id = '" + id + "'";
                    gData.Exec(sql, false, true);
                }
            }
        }
        

        public static void ScanForAmounts()
        {
            string sql = "Select * from Deposit where Amount is null";
            DataTable d1 = gData.GetDataTable2(sql, false);
            NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string id = d1.Rows[i]["id"].ToString();
                string address = d1.Rows[i]["Address"].ToString();
                string sTxId = d1.Rows[i]["txid"].ToString();

                string sRawTx = GetRawTransaction(sTxId, false);
                int nHeight = 0;

                double amt = GetAmtFromRawTx(sRawTx, address, out nHeight);
                double depth = _pool._template.height - nHeight;

                if (amt > 0 && depth <= 2)
                {
                    sql = "Update Deposit set pending=1 where id = '" + id + "'";
                    gData.Exec(sql, false, true);
                }

                if (amt > 0 && depth > 2)
                {
                    sql = "Update Deposit set Pending=2, Amount='" + amt.ToString() + "',height='" + nHeight.ToString() + "' where id = '" + id + "'";
                    gData.Exec(sql, false, true);
                }
            }
        }

        public static string GetDepositTXIDList()
        {

            try
            {
                string sql = "Select distinct id,depositaddress from Users where depositaddress is not null";
                DataTable d1 = gData.GetDataTable2(sql, false);

                JObject joe = new JObject();
                joe.Add(new JProperty("start", _pool._template.height - 20));
                joe.Add(new JProperty("end", _pool._template.height + 5));
                JArray jk = new JArray();
                for (int i = 0; i < d1.Rows.Count; i++)
                {
                    jk.Add(d1.Rows[i]["depositaddress"].ToString());
                }


                // Example:  getaddresstxids '{"addresses": ["BT2E77hVnJeahbZUU2gFoq2XC4UXZkQ7ft"], "start":210000, "end":300000 }'

                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                object[] oParams1 = new object[1];
                joe.Add(new JProperty("addresses", jk));
                oParams1[0] = joe;
                dynamic jOut1 = n.SendCommand("getaddresstxids", oParams1);
                int nResponseCount = jOut1.Result.Count;
                if (nResponseCount == 0)
                    return "";

                string sOut = "";
                for (int i = 0; i < d1.Rows.Count; i++)
                {
                    string address = d1.Rows[i]["depositaddress"].ToString();
                    string sUserId = d1.Rows[i]["id"].ToString();
                    try
                    {
                        JObject joe1 = new JObject();
                        joe1.Add(new JProperty("start", _pool._template.height - 20));
                        joe1.Add(new JProperty("end", _pool._template.height + 5));
                        JArray jk1 = new JArray();
                        jk1.Add(address);
                        object[] oParams2 = new object[1];
                        joe1.Add(new JProperty("addresses", jk1));
                        oParams2[0] = joe1;
                        dynamic jOut = n.SendCommand("getaddresstxids", oParams2);
                        dynamic o = jOut.Result;
                        if (o != null)
                        {
                            if (o.Count > 0)
                            {
                                for (int j = 0; j < o.Count; j++)
                                {
                                    string sTxId = o[j].ToString();
                                    sql = " IF NOT EXISTS (SELECT TXID FROM Deposit WHERE deposit.txid='"
                                        + sTxId + "') BEGIN \r\n INSERT INTO Deposit (id,notes,address,txid,userid,added,pending) values (newid(),'Deposit','"
                                        + address + "','" + sTxId + "','" + sUserId + "',getdate(),0) END";
                                    gData.Exec(sql, false, true);
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        Log("GetDepositTxidList " + x.Message);

                    }
                }
                ScanForAmounts();

                return sOut;
            }
            catch(Exception ex)
            {
                Log("GetDepositTXIDList " + ex.Message);
                return "";
            }
        }

        public static string GetSancTXIDList()
        {
            string sql = "Select distinct id,paymentaddress from Sancs where paymentaddress is not null";
            DataTable d1 = gData.GetDataTable2(sql, false);
            NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

            string sOut = "";
            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string address = d1.Rows[i]["paymentaddress"].ToString();
                string sUserId = d1.Rows[i]["id"].ToString();
                object[] oParams = new object[1];
                oParams[0] = address;

                try
                {
                    dynamic jOut = n.SendCommand("getaddresstxids", oParams);
                    dynamic o = jOut.Result;           
                    if (o != null)
                    {
                        for (int j = 0; j < o.Count; j++)
                        {
                            string sTxId = o[j].ToString();
                            sql = " IF NOT EXISTS (SELECT TXID FROM SanctuaryPayment WHERE txid='"
                                + sTxId + "') BEGIN \r\n INSERT INTO SanctuaryPayment (id,address,txid,added) values (newid(),'"
                                + address + "','" + sTxId + "',getdate()) END";
                            gData.Exec(sql, false, true);
                        }
                    }
                }
                catch (Exception x)
                {
                    Log("GetSancTxidList " + x.Message);

                }
            }
            ScanForSanctuaryPayments();

            return sOut;
        }

        /*

        public static void SendMarketingEmail()
        {
            // Ensure we comply with this:  https://www.ftc.gov/tips-advice/business-center/guidance/can-spam-act-compliance-guide-business
            try
            {
                string sql = "Select top 100 * from Leads where Advertised is null";
                DataTable dt = gData.GetDataTable2(sql);
                int nMax = 10;
                int nSent = 0;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string email = dt.Rows[i]["email"].ToString();
                        string username = dt.Rows[i]["name"].ToString();
                        string sPath = "c:\\bbpcampaign1.txt";
                        string sID = dt.Rows[i]["id"].ToString();
                        string body = System.IO.File.ReadAllText(sPath);
                        body = body.Replace("[name]", username);
                        body = body.Replace("[reward]", "<a href='https://foundation.biblepay.org/LandingPage?id=" + sID + "'>Empower yourself with the free biblepay coins</a>");
                        body = body.Replace("[Unsubscribe]", "<a href='https://foundation.biblepay.org/LandingPage?id=" + sID + "&action=unsubscribe'>Unsubscribe</a>");
                        MailAddress r = new MailAddress("rob@saved.one", "BiblePay Team");
                        MailAddress t = new MailAddress(email, username);
                        MailMessage m = new MailMessage(r, t);
                        m.Subject = "Sharing the Gospel and BiblePay";
                        m.IsBodyHtml = true;
                        m.Body = body;
                        bool fSent = SendMail(m);
                        if (fSent)
                        {
                            sql = "Update Leads set Advertised = getdate() where id = '" + sID + "'";
                            gData.Exec(sql);
                        }

                        nSent++;
                        if (nSent >= nMax)
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Log("Send Marketing Email Issues: " + ex.Message);
            }
        }
        */


        public static void TallyBXMRC()
        {
            string sql = "select sum(bxmrc) bxmrc,sum(bxmr)-sum(bxmrc) shrs from Share (nolock) where updated > getdate() - .07";
            PoolCommon.BXMRC = (int)gData.GetScalarDouble(sql, "bxmrc");
            PoolCommon.SHARES = (int)gData.GetScalarDouble(sql, "shrs");
        }

        public static void GetKarma(string email, out double kplus, out double kneg)
        {
            string sql = "SELECT karma_good,karma_bad from smf_members where email_address='" + email + "'";
            MySqlDataReader dr = MySQLData.GetMySqlDataReader(sql);
            kplus = 0;
            kneg = 0;
            while (dr.Read())
            {

                kplus = GetDouble(dr[0].ToString());
                kneg = GetDouble(dr[1].ToString());

            }
            dr.Close();
        }
        public static string GetFieldValue(string sEmail, string sField)
        {
            string sql = "Select * from USERS where EmailAddress='" + BMS.PurifySQL(sEmail, 100) + "'";
            string sValue = gData.GetScalarString2(sql, sField);
            return sValue;
        }

        public static void MonetizeContent(double nID, string sEmail, int nTime, double nAmount)
        {
            try
            {
                string sCherry = "\r\n[hr][img]https://san.biblepay.org/Images/cherriessmall.png[/img] " + nAmount.ToString() + " BBP";
                string sql = "Update smf_messages set body=CONCAT(body, '" + sCherry + "') where id_msg='" + nID.ToString() + "' and id_topic='517'";
                bool fSuccess = MySQLData.ExecuteNonQuery(sql);

            }
            catch(Exception ex)
            {
                Log("Unable to monentize content " + ex.Message);
            }
            
        }
        public static void UserActivityRewards()
        {

            return;

            try
            {

                double nTime = UnixTimeStamp() - (60 * 60 * 24 * 7);

                int iCt = 0;

                string sql = "SELECT poster_name, id_msg, id_topic, poster_time, id_member, subject, poster_email, poster_ip, body, approved "
                    + " FROM smf_messages where id_topic = '517' and poster_time > '" + nTime.ToString() + "' and approved=1 order by poster_time";
                MySqlDataReader dr = MySQLData.GetMySqlDataReader(sql);

                while (dr.Read())
                {
                    iCt++;

                    double nCt = GetDouble(dr[1].ToString());
                    string subject = dr[5].ToString();
                    string member_name = dr[0].ToString();
                    string semail = dr[6].ToString();
                    // Grab the User Karma
                    double kplus = 0;
                    double kneg = 0;
                    GetKarma(semail, out kplus, out kneg);
                    double ktotal = kplus + (kneg * -1);
                    double nKarmaFactor = ktotal / 10;
                    if (nKarmaFactor < 1)
                        nKarmaFactor = 1;
                    double nReward = 1 * nKarmaFactor * 1000;
                    
                    // Get the User info from our system
                    string sRewardAddress = GetFieldValue(semail, "forumrewardsaddress");
                    string sUserId = GetFieldValue(semail, "id");

                    double nID = GetDouble(dr[1].ToString());
                    if (nReward > 50000)
                    {
                        Log("Somting is very wrong.");
                        nReward = 50007;
                    }
                    sql = "Select count(*) ct from Monetization where postid='" + nID + "'";

                    double dCt = gData.GetScalarDouble(sql, "ct");

                    if (sRewardAddress.Length > 20 && dCt == 0)
                    {
                        MonetizeContent(nID, semail, (int)nTime, nReward);
                        DataOps.AdjBalance(nReward, sUserId, "Forum Benevolence Reward for [" + subject + "]");
                        sql = "Insert into monetization (id,postid,added) values (newid(),'" + nID + "', getdate())";
                        gData.Exec(sql);
                    }

                }
                dr.Close();
            }
            catch (Exception ex)
            {
                Log("Err SyncUsers " + ex.Message);
            }

        }

        static int nLastUpdatedUsers = 0;
        public static void SyncUsers()
        {
            int iFinished = 0;
            int nElapsed = UnixTimeStamp() - nLastUpdatedUsers;
            if (nElapsed < (60 * 60 * 12))
                return;


            try
            {
                string sql5 = "Select count(*) ct from smf_members";
                double dSMFCt = GetDouble(MySQLData.GetScalarString(sql5, 0));

                string sql6 = "Select Value from System where SystemKey='SMF_CT'";
                double dMSCt = gData.GetScalarDouble(sql6, "Value");
                if (dSMFCt == dMSCt)
                    return;

                sql6 = "Delete from System where SystemKey='SMF_CT'\r\nInsert into System (id,SystemKey,Value,updated) values (newid(),'SMF_CT','" + dSMFCt.ToString() + "',getdate())";
                gData.Exec(sql6);

                string sql = "SELECT member_name,real_name,email_address,date_registered,posts,member_ip,member_ip2,karma_good,karma_bad From smf_members";
                MySqlDataReader dr = MySQLData.GetMySqlDataReader(sql);

                while (dr.Read())
                {
                    string member_name = dr[0].ToString();
                    string real_name = dr[1].ToString();
                    iFinished++;
                    string email_address = dr[2].ToString();
                    string drr = dr[3].ToString();
                    DateTime dtDr = UnixTimeStampToDateTime(GetDouble(drr));

                    string sql2 = "Select * from Users where Username='" + BMS.PurifySQL(real_name,30) + "' or emailAddress='" + BMS.PurifySQL(email_address,100) + "'";
                    DataTable dtOurUser = gData.GetDataTable2(sql2);
                    if (dtOurUser.Rows.Count > 0)
                    {
                        string id = dtOurUser.Rows[0]["id"].ToString();
                        string our_email_address = dtOurUser.Rows[0]["emailaddress"].ToString();

                        if (id.Length > 0 && email_address != our_email_address)
                        {
                            string sql3 = "Update Users set EmailAddress='" + email_address + "',updated='" + dtDr.ToString() + "' where id = '" + id + "'";
                            gData.Exec(sql3);
                        }
                    }
                    else
                    {
                        string sql4 = "Insert into Users (id,Username,EmailAddress,Added,Updated,Admin) values (newid(),'" + member_name + "','" + email_address + "','" + dtDr.ToString() + "','" + dtDr.ToString() + "',0)";
                        gData.Exec(sql4);
                    }
                }
                dr.Close();
            }
            catch(Exception ex)
            {
                Log("Err SyncUsers " + ex.Message);
            }
            Log("Successfully updated " + iFinished.ToString() + " USERS");
            nLastUpdatedUsers = UnixTimeStamp();
        }


        static List<string> lBanList = new List<string>();
        public static bool fUseJobsTable = false;
        public static bool fUseBanTable = false;
        public static bool fLogSql = false;
        static int nLastDeposited = 0;
        static int nLastHourly = 0;
        static int nLastBoarded = 0;
        public static void Leaderboard()
        {
            int nElapsed = UnixTimeStamp() - nLastBoarded;
            int nDepositElapsed = UnixTimeStamp() - nLastDeposited;
            int nHourlyElapsed = UnixTimeStamp() - nLastHourly;
            if (nElapsed < (60 * 2))
                return;
            nLastBoarded = UnixTimeStamp();
            fUseBanTable = Convert.ToBoolean(GetDouble(GetPoolValue("USEBAN")));
            fUseJobsTable = Convert.ToBoolean(GetDouble(GetPoolValue("USEJOB")));
            fLogSql = Convert.ToBoolean(GetDouble(GetPoolValue("USESQL")));

            try
            {
                // Update the leaderboard
                string sql = "exec updLeaderboard";
                SqlCommand command = new SqlCommand(sql);
                lSQL.Add(command);
                TallyBXMRC();
                GetChartOfWorkers();
                GetChartOfHashRate();
                GetChartOfBlocks();

                if (nDepositElapsed > (60 * 15))
                {
                    nLastDeposited = UnixTimeStamp();
                    GetDepositTXIDList();
                }

                if (nHourlyElapsed > (60 * 60))
                {
                    nLastHourly = UnixTimeStamp();
                    Proposals.SubmitProposals(true);
                    Proposals.SubmitProposals(false);
                    SyncUsers();
                }

            }
            catch (Exception ex)
            {
                Log("PoolLeaderboard" + ex.Message);
            }
        }


        private static void RecordParticipants()
        {
            // This is for the difficulty chart

            int nBestHeight = _pool._template.height;
            if (nBestHeight == 0) return;

            string sql = "Select max(height) h from DifficultyHistory";
            int nH = (int)gData.GetScalarDouble(sql, "h");
            if (nH < nBestHeight - 1000)
                nH = nBestHeight - 1000;
            // Set subsidies first
            for (int iMyHeight = nH - 1; iMyHeight < nBestHeight; iMyHeight++)
            {
                double nSubsidy = 0;
                string sRecip = "";
                GetSubsidy(iMyHeight, ref sRecip, ref nSubsidy);
                double dPOWDiff = GetDouble(GetShowBlock("showblock", iMyHeight, "difficulty"));

                sql = "Delete from DifficultyHistory where height = '" + iMyHeight.ToString() + "'\r\nInsert Into DifficultyHistory (id,height,recipient,subsidy,added,difficulty) values (newid(),'"
                    + iMyHeight.ToString() + "','" + sRecip + "','" + nSubsidy.ToString() + "',getdate(),'" + dPOWDiff.ToString() + "')";
                if (iMyHeight > 0 && sRecip != "" && sRecip != null && nSubsidy > 0)
                {
                    gData.Exec(sql);
                }
            }
        }


        public static void ExecRokuOperations()
        {

            try
            {
                RokuOperations.GenerateMediaPlayGrid();
                RokuOperations.GenerateMediaListXML();
                string test = "";
            }
            catch(Exception ex)
            {
                Log("ExecRokuOperations::" + ex.Message);
            }
        }


        private static int nLastGrouped = 0;
        public static void GroupShares()
        {
            int nElapsed = UnixTimeStamp() - nLastGrouped;
            if (nElapsed < (60 * 20))
                return;

            try
            {

                nLastGrouped = UnixTimeStamp();
                if (_pool._template.height == 0)
                {
                    GetBlockForStratum();
                }

                ExecRokuOperations();

                int nBestHeight = _pool._template.height;
                if (nBestHeight == 0) return;
                int nLookback = 205;

            
                for (int iMyHeight = nBestHeight - nLookback; iMyHeight < nBestHeight - 7; iMyHeight++)
                {
                    string sql7 = "Select count(*) ct from Share (nolock) where paid is null and Subsidy is null and height = '" + iMyHeight.ToString() + "'";
                    double dCt = gData.GetScalarDouble(sql7, "ct", false);

                    if (dCt > 0)
                    {
                        double nSubsidy = 0;
                        string sRecip = "";
                        GetSubsidy(iMyHeight, ref sRecip, ref nSubsidy);
                        string sPoolAddress = GetBMSConfigurationKeyValue("PoolAddress");
                        if (sPoolAddress == "")
                        {
                            Log("Unable to start pool; pool address not set.  Set PoolAddress=receiveaddress in bms.conf.");
                        }
                        if (sRecip != sPoolAddress)
                        {
                            nSubsidy = .02;
                            string sql3 = "Select * from Share (nolock) Where Paid is null and height = @height";
                            SqlCommand command3 = new SqlCommand(sql3);
                            command3.Parameters.AddWithValue("@height", iMyHeight);
                            DataTable dt4 = gData.GetDataTable(command3, false);
                            for (int x = 0; x < dt4.Rows.Count; x++)
                            {
                                string bbpaddress1 = dt4.Rows[x]["bbpaddress"].ToString();

                                string sql9 = "exec insShare @bbpid,@shareAdj,@failAdj,@height,@sxmr,@fxmr,@sxmrc,@fxmrc,@bxmr,@bxmrc";
                                SqlCommand command5 = new SqlCommand(sql9);
                                command5.Parameters.AddWithValue("@bbpid", bbpaddress1);
                                command5.Parameters.AddWithValue("@shareAdj", GetDouble(dt4.Rows[x]["shares"]));
                                command5.Parameters.AddWithValue("@failAdj", GetDouble(dt4.Rows[x]["fails"]));
                                command5.Parameters.AddWithValue("@height", iMyHeight + 1);
                                command5.Parameters.AddWithValue("@sxmr", GetDouble(dt4.Rows[x]["sucxmr"]));
                                command5.Parameters.AddWithValue("@fxmr", GetDouble(dt4.Rows[x]["failxmr"]));
                                command5.Parameters.AddWithValue("@sxmrc", GetDouble(dt4.Rows[x]["SucXMRC"]));
                                command5.Parameters.AddWithValue("@fxmrc", GetDouble(dt4.Rows[x]["FailXMRC"]));
                                command5.Parameters.AddWithValue("@bxmr", GetDouble(dt4.Rows[x]["BXMR"]));
                                command5.Parameters.AddWithValue("@bxmrc", GetDouble(dt4.Rows[x]["BXMRC"]));
                                try
                                {
                                    gData.ExecCmd(command5);
                                }
                                catch(Exception ex2)
                                {
                                    Log("GroupShares:" + ex2.Message);
                                }


                                //now delete the source share
                                sql3 = "Delete from Share where height=@height and bbpaddress=@bbpid";
                                command5 = new SqlCommand(sql3);
                                command5.Parameters.AddWithValue("@bbpid", bbpaddress1);
                                command5.Parameters.AddWithValue("@height", iMyHeight);

                                gData.ExecCmd(command5);

                            }


                        }

                        string sql8 = "Update Share Set Subsidy=@subsidy,Solved=@solved where height = @height and subsidy is null";
                        SqlCommand command1 = new SqlCommand(sql8);
                        command1.Parameters.AddWithValue("@subsidy", nSubsidy);
                        command1.Parameters.AddWithValue("@height", iMyHeight);
                        int iSolved = nSubsidy > 1 ? 1 : 0;
                        command1.Parameters.AddWithValue("@solved", iSolved);

                        gData.ExecCmd(command1);
                    }
                }
                        

                // Set subsidies next
                for (int iMyHeight = nBestHeight - nLookback; iMyHeight < nBestHeight - 7; iMyHeight++)
                {
                    string sHeightRange = "height = '" + iMyHeight.ToString() + "'";
                    string sql = "Select shares,sucXMRC,bxmr,bbpaddress,subsidy from Share (nolock) WHERE subsidy > 1 and percentage is null and "
                        + sHeightRange + " and paid is null";
                    DataTable dt1 = gData.GetDataTable2(sql, false);
                    if (dt1.Rows.Count > 0)
                    {
                        // First get the total shares
                        double nTotalShares = 0;
                        double nTotalSubsidy = GetDouble(dt1.Rows[0]["subsidy"]);
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            double nHPS = GetDouble(dt1.Rows[i]["Shares"]) + (GetDouble(dt1.Rows[i]["bxmr"]));
                            nTotalShares += nHPS;
                        }
                        if (nTotalShares == 0)
                            nTotalShares = .01;
                        double nPoolFee = GetDouble(GetBMSConfigurationKeyValue("PoolFee"));
                        double nBonus = GetDouble(GetBMSConfigurationKeyValue("PoolBlockBonus"));
                        double nPercOfSubsidy = nBonus / (nTotalSubsidy + .01);

                        double nIndividualPiece = nPercOfSubsidy / (dt1.Rows.Count + .01);

                        double nMinBonusShareThresh = GetDouble(GetBMSConfigurationKeyValue("MinBlockBonusThreshhold"));

                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            //double nShare = GetDouble(dt1.Rows[i]["Shares"]) / nTotalShares;
                            double nMinerShares = (GetDouble(dt1.Rows[i]["Shares"]) + (GetDouble(dt1.Rows[i]["bxmr"])));
                            double nMinerFee = nMinerShares * nPoolFee;

                            nMinerShares = nMinerShares - nMinerFee;
                            double nShare = nMinerShares / nTotalShares;
                            // Add on the extra bonus
                            if (nMinerShares > nMinBonusShareThresh)
                                nShare += nIndividualPiece;
                             
                            sql = "Update Share Set Percentage=@percentage,Reward=@percentage * Subsidy where " + sHeightRange + " and bbpaddress=@bbpaddress";
                            SqlCommand command = new SqlCommand(sql);
                            command.Parameters.AddWithValue("@percentage", Math.Round(nShare, 4));
                            command.Parameters.AddWithValue("@height", iMyHeight);
                            command.Parameters.AddWithValue("@bbpaddress", dt1.Rows[i]["bbpaddress"]);
                            gData.ExecCmd(command);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Group Shares " + ex.Message);
            }
            if (false)
                Log("Finished Grouping shares v2.0", true);
        }
        public static void SQLExecutor()
        {
            while (true)
            {
                // This thread executes SQL in a way that prevents deadlocks
                for (int i = 0; i < lSQL.Count; i++)
                {
                    try
                    {
                        gData.ExecCmd(lSQL[i]);
                    }
                    catch (Exception ex2)
                    {
                        Log("SQLExecutor::" + ex2.Message + ":" + lSQL[i].CommandText);
                    }
                    lSQL.RemoveAt(i);
                    i--;
                }
                Thread.Sleep(100);
            }
        }

        public static string PoolBonusNarrative()
        {
            double nBonus = GetDouble(GetBMSConfigurationKeyValue("PoolBlockBonus"));
            if (nBonus > 0)
            {
                double nMinBonusShareThresh = GetDouble(GetBMSConfigurationKeyValue("MinBlockBonusThreshhold"));

                string sNarr = "We are giving away an extra " + nBonus.ToString() + " BBP per block split equally across participating miners who have more than " + nMinBonusShareThresh.ToString() + " shares in the leaderboard (see Block Bonus).";
                return sNarr;
            }
            return "";
        }

        public static void GetRandomXAudit(string rxheader, string rxkey, ref string rx, ref string rx_root)
        {
            try
            {
                object[] oParams = new object[3];
                oParams[0] = "randomx_pool";
                oParams[1] = rxheader;
                oParams[2] = rxkey;
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                dynamic oOut = n.SendCommand("exec", oParams);
                rx = oOut.Result["RX"];
                rx_root = oOut.Result["RX_root"];
            }
            catch (Exception ex)
            {
                Log("GRXA " + ex.Message);
            }
        }
        public static string GetShowBlock(string sCommand, int iBlockNumber, string sJSONFieldName)
        {
            try
            {
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                object[] oParams = new object[1];
                oParams[0] = iBlockNumber.ToString();
                dynamic oOut = n.SendCommand("getblock", oParams);
                string sOut = oOut.Result[sJSONFieldName].ToString();
                return sOut;
            }
            catch (Exception)
            {
                return "";
            }
        }
        private static void GetSubsidy(int nHeight, ref string sRecipient, ref double nSubsidy)
        {
            try
            {
                object[] oParams = new object[2];
                oParams[0] = "subsidy";
                oParams[1] = nHeight.ToString();
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                dynamic oOut = n.SendCommand("exec", oParams);
                nSubsidy = GetDouble(oOut.Result["subsidy"]);
                sRecipient = oOut.Result["recipient"];
                return;
            }
            catch (Exception ex)
            {
                Log("GS " + ex.Message);
            }
            sRecipient = "";
            nSubsidy = 0;

        }

        public static bool SubmitBlock(string hex)
        {
            try
            {
                object[] oParams = new object[1];
                oParams[0] = hex;
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                dynamic oOut = n.SendCommand("submitblock", oParams);
                string result = oOut.Result.Value;
                // To do return binary response code here; check response for fail and success
                if (result == null)
                    return true;
                if (result == "high-hash")
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Log("SB " + ex.Message);
            }
            return false;
        }

        public static string GetBlockForStratumHex(string poolAddress, string rxkey, string rxheader)
        {
            try
            {
                object[] oParams = new object[3];
                oParams[0] = poolAddress;
                oParams[1] = rxkey;
                oParams[2] = rxheader;
                NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();
                dynamic oOut = n.SendCommand("getblockforstratum", oParams);
                string result = oOut.Result["hex"];
                return result;
            }
            catch (Exception ex)
            {
                Log("GBFS " + ex.Message);
            }
            return "";
        }

        public static byte[] StringToByteArr(string hex)
        {
            try
            {
                if (hex == null)
                {
                    byte[] b1 = new byte[0];
                    return b1;
                }
                if (hex.Length % 2 == 1)
                    throw new Exception("The binary key cannot have an odd number of digits");

                byte[] arr = new byte[hex.Length >> 1];

                for (int i = 0; i < hex.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
                }
                return arr;
            }
            catch (Exception ex)
            {
                Log(" STBA " + ex.Message);
                byte[] b = new byte[0];
                return b;
            }
        }

        public static int GetHexVal(char hex)
        {
            try
            {
                int val = (int)hex;
                return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
            }
            catch (Exception ex)
            {
                Log("GHV " + ex.Message);
                return 0;
            }
        }

        public static string ReverseHexString(string hexString)
        {
            var sb = new StringBuilder();
            for (var i = hexString.Length - 2; i > -1; i = i - 2)
            {
                string hexChar = hexString.Substring(i, 2);
                sb.Append(hexChar);
            }
            return sb.ToString();
        }

        public static List<string> randomxhashes = new List<string>();
        public static bool IsUnique(string bbphash)
        {
            if (fMonero2000)
                return true;
            try
            {
                if (randomxhashes.Contains(bbphash))
                    return false;
                randomxhashes.Add(bbphash);
                return true;
            }
            catch (Exception)
            {

            }
            return true;
        }

        private static object cs_stratum = new object();
        public static void GetBlockForStratum()
        {
            int nAge = UnixTimeStamp() - _pool._template.updated;
            if (nAge < 60)
                return;

            lock (cs_stratum)
            {
                try
                {
                    // When it expires, get new template
                    NBitcoin.RPC.RPCClient n = WebRPC.GetLocalRPCClient();

                    string poolAddress = GetBMSConfigurationKeyValue("PoolAddress");
                    object[] oParams = new object[1];
                    oParams[0] = poolAddress;
                    dynamic oOut = n.SendCommand("getblockforstratum", oParams);
                    _pool._template = new BlockTemplate();
                    _pool._template.hex = oOut.Result["hex"].ToString();
                    _pool._template.curtime = oOut.Result["curtime"].ToString();
                    _pool._template.prevhash = oOut.Result["prevblockhash"].ToString();
                    _pool._template.height =(int)oOut.Result["height"];
                    _pool._template.bits = oOut.Result["bits"].ToString();
                    _pool._template.prevblocktime = oOut.Result["prevblocktime"].ToString();
                    _pool._template.target = oOut.Result["target"].ToString();
                    _pool._template.updated = UnixTimeStamp();
                    if (nGlobalHeight != _pool._template.height)
                    {
                        MarkForBroadcast();
                    }
                    nGlobalHeight = _pool._template.height;
                }
                catch (Exception ex)
                {
                    Log("GBFS1.1 " + ex.Message, true);
                }
            }
        }
        
    }
}
