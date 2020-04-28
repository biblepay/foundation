using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.Common;


namespace Saved.Code
{

    public static class PoolCommon
    {

        public static Dictionary<string, WorkerInfo> dictWorker = new Dictionary<string, WorkerInfo>();
        public static Dictionary<string, WorkerInfo> dictBan = new Dictionary<string, WorkerInfo>();
        public static int iThreadCount = 0;
        public static int nGlobalHeight = 0;
        public static int pool_version = 1007;
        public static int iXMRThreadID = 0;
        public static int iXMRThreadCount = 0;
        public static int iTitheNumber = 0;

        public static List<SqlCommand> lSQL = new List<SqlCommand>();
        public static DateTime start_date = DateTime.Now;
        public static int MIN_DIFF = 1;
        public static object cs_p = new object();

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
            catch(Exception ex)
            {
                // This is not supposed to happen, but I see this error in the log... 
                WorkerInfo w = new WorkerInfo();
                SetWorker(w, socketid);
                return w;
            }
        }

        public static string GetPoolValue(string sKey)
        {
            string sql = "Select value from System where systemkey='" + sKey + "'";
            string value = gData.GetScalarString(sql, "value", false);
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
            catch (Exception ex)
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
            catch(Exception x)
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


        public static void InsShare(string bbpaddress, int nShareAdj, int nFailAdj, int height, int nBXMR, int nBXMRC)
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
                string sql = "Select bbpaddress from worker (nolock) where moneroaddress='" + sMoneroAddress + "'";
                string bbp = gData.GetScalarString(sql, "bbpaddress");
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
            catch (Exception ex)
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

        public static string GetChartOfHashRate()
        {
            int nMax = _pool._template.height - 1;

            int nMin = nMax - 205;

            string sql = "select  HashRate,Height From HashRate where height > "
                + nMin.ToString() + " and height < " + nMax.ToString() + " order by height";

            DataTable dt = gData.GetDataTable(sql, false);
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
            DataTable dt = gData.GetDataTable(sql, false);
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
            DataTable dt = gData.GetDataTable(sql, false);
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


        public static bool ValidateBiblepayAddress(string sAddress)
        {
            try
            {
                object[] oParams = new object[1];
                oParams[0] = sAddress;
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

                dynamic oOut = n.SendCommand("validateaddress", oParams);
                string sResult = oOut.Result["isvalid"].ToString();
                if (sResult.ToLower().Contains("true")) return true;
                return false;
            }
            catch (Exception ex)
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
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

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
            try
            {
                string sql = "SELECT   amount, height, txid, address, id      FROM       sanctuaryPayment      WHERE paid is null and amount > 100";
                DataTable dt = gData.GetDataTable(sql, false);
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string sql2 = "Select * From SanctuaryInvestments order by Amount";
                    double nSancReward = GetDouble(dt.Rows[i]["Amount"]);
                    double nHeight = GetDouble(dt.Rows[i]["height"]);
                    string sId = dt.Rows[i]["id"].ToString();
                    string sAddress = dt.Rows[i]["address"].ToString();
                    string sTXID = dt.Rows[i]["txid"].ToString();

                    DataTable dt2 = gData.GetDataTable(sql2, false);
                    for (int j = 0; j < dt2.Rows.Count; j++)
                    {

                        string sql3 = "Insert Into Deposit (id, address, txid, userid, added, amount, height, notes) values (newid(), @address, @txid, @userid, getdate(), @amount, @height, @notes)";
                        SqlCommand command = new SqlCommand(sql3);
                        command.Parameters.AddWithValue("@address", sAddress);
                        command.Parameters.AddWithValue("@txid", sTXID + "-" + j.ToString());
                        command.Parameters.AddWithValue("@userid", dt2.Rows[j]["userid"]);
                        double nAmount = GetDouble(dt2.Rows[j]["amount"]);
                        if (nAmount > 0)
                        {
                            double nReward = nAmount / 4500000 * nSancReward;
                            command.Parameters.AddWithValue("@amount", nReward);
                            command.Parameters.AddWithValue("@height", nHeight);
                            command.Parameters.AddWithValue("@notes", "Sanctuary Payment [for " + nAmount.ToString() + "]");
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




        static int nLastPaid = 0;
        public static bool Pay()
        {
            int nElapsed = UnixTimeStamp() - nLastPaid;
            if (nElapsed < (60 * 60 * 4))
                return false;
            nLastPaid = UnixTimeStamp();
            try
            {
                GetSancTXIDList();
                PaySanctuaryInvestors();
                RecordParticipants();
            }
            catch (Exception ex2)
            {
                Log("GetSancTXIDList: " + ex2.Message);
            }
            try
            {

                // Create a batchid
                string batchid = Guid.NewGuid().ToString();
                string sql = "Update share set txid=@batchid where Paid is null and subsidy > 1 and updated < getdate() - .25";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@batchid", batchid);
                gData.ExecCmd(command, false, false, false);
                sql = "Select bbpaddress, sum(Reward) reward from Share where txid = @batchid and paid is null group by bbpaddress";
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@batchid", batchid);

                DataTable dt = gData.GetDataTable(command, false);
                List<Payment> Payments = new List<Payment>();
                double nTotal = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string address = dt.Rows[i]["bbpaddress"].ToString();
                    double nReward = GetDouble(dt.Rows[i]["Reward"]);

                    bool bValid = ValidateBiblepayAddress(address);

                    if (bValid && nReward > .01)
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

        //Scan for the credit amount
        public static string GetRawTransaction(string sTxid)
        {
            try
            {
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
                object[] oParams = new object[2];
                oParams[0] = sTxid;
                oParams[1] = 1;
                dynamic oOut = n.SendCommand("getrawtransaction", oParams);
                // Loop Through the Vouts and get the recip ids and the amounts
                string sOut = "";
                for (int y = 0; y < oOut.Result["vout"].Count; y++)
                {
                    string sPtr = "";
                    try
                    {
                        sPtr = (oOut.Result["vout"][y] ?? "").ToString();
                    }
                    catch (Exception ey)
                    {
                    }

                    if (sPtr != "")
                    {
                        string sAmount = oOut.Result["vout"][y]["value"].ToString();
                        string sAddress = "";
                        if (oOut.Result["vout"][y]["scriptPubKey"]["addresses"] != null)
                        {
                            sAddress = oOut.Result["vout"][y]["scriptPubKey"]["addresses"][0].ToString();
                        }
                        else { sAddress = "?"; } //Happens when pool pays itself
                        string height = oOut.Result["height"].ToString();

                        sOut += sAmount + "," + sAddress + "," + height + "|";
                    }
                    else
                    {
                        break;
                    }
                }
                return sOut;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
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
            DataTable d1 = gData.GetDataTable(sql, false);
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string id = d1.Rows[i]["id"].ToString();
                string address = d1.Rows[i]["Address"].ToString();
                string sTxId = d1.Rows[i]["txid"].ToString();

                string sRawTx = GetRawTransaction(sTxId);
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
            DataTable d1 = gData.GetDataTable(sql, false);
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

            string sOut = "";
            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string id = d1.Rows[i]["id"].ToString();
                string address = d1.Rows[i]["Address"].ToString();
                string sTxId = d1.Rows[i]["txid"].ToString();

                string sRawTx = GetRawTransaction(sTxId);
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
            string sql = "Select distinct id,depositaddress from Users where depositaddress is not null";
            DataTable d1 = gData.GetDataTable(sql, false);
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

            string sOut = "";
            for (int i = 0; i < d1.Rows.Count; i++)
            {
                string address = d1.Rows[i]["depositaddress"].ToString();
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
                            sql = " IF NOT EXISTS (SELECT TXID FROM Deposit WHERE deposit.txid='"
                                + sTxId + "') BEGIN \r\n INSERT INTO Deposit (id,notes,address,txid,userid,added,pending) values (newid(),'Deposit','"
                                + address + "','" + sTxId + "','" + sUserId + "',getdate(),0) END";
                            gData.Exec(sql, false, true);
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


        public static string GetSancTXIDList()
        {
            string sql = "Select distinct id,paymentaddress from Sancs where paymentaddress is not null";
            DataTable d1 = gData.GetDataTable(sql, false);
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

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
                    dynamic o = jOut.Result;           //
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

        static List<string> lBanList = new List<string>();
        public static bool fUseJobsTable = false;
        public static bool fUseBanTable = false;
        static int nLastBoarded = 0;
        public static void Leaderboard()
        {
            int nElapsed = UnixTimeStamp() - nLastBoarded;
            if (nElapsed < (60 * 2))
                return;
            nLastBoarded = UnixTimeStamp();
            fUseBanTable = Convert.ToBoolean(GetDouble(GetPoolValue("USEBAN")));
            fUseJobsTable = Convert.ToBoolean(GetDouble(GetPoolValue("USEJOB")));

            try
            {
                // Update the leaderboard
                string sql = "exec updLeaderboard";
                SqlCommand command = new SqlCommand(sql);
                lSQL.Add(command);

                GetChartOfWorkers();
                GetChartOfHashRate();
                GetDepositTXIDList();
                GetChartOfBlocks();
                Code.BMS.LaunchInterfaceWithWCG();


                // Clear banned pool users
                try
                {
                    dictBan.Clear();
                    //Memorize the excess banlist
                    sql = "Select distinct dbo.iponly(ip) ip from Worker where bbpaddress in (select bbpaddress from leaderboard where efficiency < .20) UNION ALL Select IP from Bans";
                    DataTable dt = gData.GetDataTable(sql);
                    lBanList.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["ip"].ToString().Length > 1)
                            lBanList.Add(dt.Rows[i]["ip"].ToString());
                    }
                    // Clear the ddos level also
                    PurgeSockets(true);
                    // End of Clear ban
                }
                catch (Exception ex)
                {
                    Log("Clearing ban " + ex.Message);
                }


            }
            catch (Exception ex)
            {
                Log("PoolLeaderboard" + ex.Message);
            }
        }


        private static void RecordParticipants()
        {
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


                int nBestHeight = _pool._template.height;
                if (nBestHeight == 0) return;

                // Set subsidies first
                for (int iMyHeight = nBestHeight - 200; iMyHeight < nBestHeight - 7; iMyHeight++)
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
                                }catch(Exception ex2)
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

                // Set subsidies first
                for (int iMyHeight = nBestHeight - 205; iMyHeight < nBestHeight - 7; iMyHeight++)
                {
                    string sql = "Select shares,sucxmrc,bxmr,bbpaddress from Share (nolock) WHERE subsidy > 1 and percentage is null and height=@height and paid is null";
                    SqlCommand command = new SqlCommand(sql);
                    command.Parameters.AddWithValue("@height", iMyHeight);
                    DataTable dt1 = gData.GetDataTable(command, false);
                    if (dt1.Rows.Count > 0)
                    {
                        // First get the total shares
                        double nTotalShares = 0;
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            double nHPS = GetDouble(dt1.Rows[i]["Shares"]) + (GetDouble(dt1.Rows[i]["bxmr"]));
                            //double nHPS = GetDouble(dt1.Rows[i]["Shares"]);

                            nTotalShares += nHPS;
                        }
                        if (nTotalShares == 0) nTotalShares = .01;
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            //double nShare = GetDouble(dt1.Rows[i]["Shares"]) / nTotalShares;
                            double nShare = (GetDouble(dt1.Rows[i]["Shares"]) + (GetDouble(dt1.Rows[i]["bxmr"]))) / nTotalShares;
                            sql = "Update Share Set Percentage=@percentage,Reward=Subsidy * @percentage where height = @height and bbpaddress=@bbpaddress";
                            command = new SqlCommand(sql);
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
            Log("Finished Grouping shares", true);
        }

        public static void GetRandomXAudit(string rxheader, string rxkey, ref string rx, ref string rx_root)
        {
            try
            {
                object[] oParams = new object[3];
                oParams[0] = "randomx_pool";
                oParams[1] = rxheader;
                oParams[2] = rxkey;
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
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
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
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
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
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
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
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
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
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
            try
            {
                if (randomxhashes.Contains(bbphash))
                    return false;
                randomxhashes.Add(bbphash);
                return true;
            }
            catch (Exception ex)
            {

            }
            return true;
        }

        private static object cs_stratum = new object();
        public static void GetBlockForStratum()
        {
            lock (cs_stratum)
            {
                try
                {
                    // When it expires, get new template
                    int nAge = UnixTimeStamp() - _pool._template.updated;
                    if (nAge < 60)
                        return;
                    NBitcoin.RPC.RPCClient n = GetLocalRPCClient();

                    string poolAddress = GetBMSConfigurationKeyValue("PoolAddress");
                    object[] oParams = new object[1];
                    oParams[0] = poolAddress;
                    dynamic oOut = n.SendCommand("getblockforstratum", oParams);
                    _pool._template = new BlockTemplate();
                    _pool._template.hex = oOut.Result["hex"].ToString();
                    _pool._template.curtime = oOut.Result["curtime"].ToString();
                    _pool._template.prevhash = oOut.Result["prevblockhash"];
                    _pool._template.height = oOut.Result["height"];
                    _pool._template.bits = oOut.Result["bits"];
                    _pool._template.prevblocktime = oOut.Result["prevblocktime"];
                    _pool._template.target = oOut.Result["target"];
                    _pool._template.updated = UnixTimeStamp();
                    if (nGlobalHeight != _pool._template.height)
                    {
                        MarkForBroadcast();
                    }
                    nGlobalHeight = _pool._template.height;
                }
                catch (Exception ex)
                {
                    Log("GBFS1.1 " + ex.Message);
                }
            }
        }
        //Todo: Put an index on Share (bbpaddress, height).  Delete old share data older than 2 weeks, etc.
    }
}
