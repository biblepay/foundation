﻿using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved.Code
{
    public class Pool
    {
        public BlockTemplate _template;
        public static int iID = 0;
        public static bool fUseLocalXMR = true;

        private void SetWorkerForErase(string socketid)
        {
            WorkerInfo w = GetWorker(socketid);
            w.bbpaddress = "";
            w.receivedtime = 1;
            dictWorker[socketid] = w;
        }
        private bool Send(Socket oClient, byte[] oData, string socketid)
        {
            try
            {
                oClient.Send(oData);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("was aborted"))
                {
                    try
                    {
                        SetWorkerForErase(socketid);
                    } 
                    catch (Exception ex2)
                    {

                    }
                }
                else
                {
                    bool fPrint = !(ex.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time") || ex.Message.Contains("An existing connection was forcibly closed"));
                    if (ex.Message.Contains("access a disposed"))
                    {
                        return false;
                    }
                    if (fPrint)
                        Log("SEND " + ex.Message);
                }
                return false;
            }
        }


        public static string mBatch = "";
        public static int nBatchCount = 0;
        public static void BatchExec(string sql)
        {
            //gData.Exec(sql);
            //return;

            mBatch += sql + "\r\n";
            if (nBatchCount > 20)
            {
                nBatchCount = 0;
                gData.Exec(mBatch, false, true);
                mBatch = "";
                //Log("executed batch " + mBatch);
            }
            nBatchCount++;
        }

        public static int jobid = 0;
        bool SendMiningJob(Socket oClient, string bbpaddress, string socketid)
        {
            try
            {
                if (jobid == 0)
                    jobid = UnixTimeStamp();

                if (bbpaddress == "" || bbpaddress == null)
                    return false;

                jobid++;
                GetBlockForStratum();
                SetNextDifficulty(socketid);
                WorkerInfo w = GetWorker(socketid);
                w.height = _template.height;
                w.jobid = jobid;
                w.difficulty = w.nextdifficulty;

                if (w.reset)
                {
                    w.reset = false;
                    w.difficulty = MIN_DIFF;
                }

                if (w.difficulty < MIN_DIFF)
                    w.difficulty = MIN_DIFF;

                w.updated = UnixTimeStamp();
                w.Broadcast = false;
                dictWorker[socketid] = w;

                // Insert the Job
                string sql = "exec insJob @jobid,@bbpid,@diff,@ip";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@jobid", jobid);
                command.Parameters.AddWithValue("@bbpid", bbpaddress);
                command.Parameters.AddWithValue("@diff", w.difficulty);
                command.Parameters.AddWithValue("@ip", oClient.RemoteEndPoint.ToString());
                gData.ExecCmd(command, true, true);

                string job_id = jobid.ToString();
                string prevhash = _template.prevhash;
                string coinbase = _template.hex;
                string nbits = _template.bits;
                string ntime = _template.curtime;
                string c3 = "";
                string c4 = "";
                string c5 = "";
                string c8 = "";
                string prevblocktime = _template.prevblocktime;
                var json = "{ \"id\": null, \"method\": \"mining.notify\", \"params\": [\"" + job_id + "\",\"" + prevhash + "\",\""
                    + coinbase + "\",\"" + c3 + "\",\"" + c4 + "\",\"" + c5 + "\",\"" + nbits + "\",\"" + ntime + "\",\"" + c8 + "\",\"" + prevblocktime + "\"]}\r\n";
                byte[] bytes = Encoding.ASCII.GetBytes(json);
                bool f1 = Send(oClient, bytes, socketid);
                if (!f1)
                    return f1;
                // Set the difficulty
                json = "{ \"id\":null, \"method\": \"mining.set_difficulty\", \"params\": [\"" + w.difficulty.ToString() + "\",\"512\"]}\r\n";
                bytes = Encoding.ASCII.GetBytes(json);
                bool f2 = Send(oClient, bytes, socketid);

                w.starttime = UnixTimeStamp();
                dictWorker[socketid] = w;

                return f2;
            }
            catch(Exception ex)
            {
                bool fPrint = !ex.Message.Contains("was being aborted");
                if (!fPrint)
                      Log("SendMiningJob " + ex.Message);
                return false;
            }
        }

        private void SetNextDifficulty(string socketid)
        {
            // This sub implements vardiff
            WorkerInfo w = GetWorker(socketid);
            int nSolveDuration = UnixTimeStamp() - w.priorsolvetime;

            if (nSolveDuration < (60 * 2))
            {
                w.nextdifficulty = w.difficulty + 1;
            }
            else
            {
                w.nextdifficulty = w.difficulty - 1;
            }
            if (nSolveDuration > (60 * 8))
            {
                w.nextdifficulty = w.difficulty / 2;
            }
            if (nSolveDuration > (60 * 10))
                w.nextdifficulty = w.difficulty / 4;

            if (w.nextdifficulty < MIN_DIFF)
                w.nextdifficulty = MIN_DIFF;
            dictWorker[socketid] = w;
        }

        private void PersistWorker(WorkerInfo w)
        {
            string sql = " IF NOT EXISTS (SELECT moneroaddress FROM worker WHERE moneroaddress='"
                      + w.moneroaddress + "') BEGIN \r\n INSERT INTO Worker (id,added,moneroaddress,bbpaddress) values (newid(),getdate(),'"
                      + Saved.Code.BMS.PurifySQL(w.moneroaddress,255) + "','" + Saved.Code.BMS.PurifySQL(w.bbpaddress,255) + "') END";
            gData.Exec(sql);
        }

        object cs_message = new object();
        private bool HandleSocket(string sJson, Socket oClient, string socketid)
        {

            if (sJson == "" || sJson == null)
            {
                return true;
            }

            try
            {
                JObject oStratum = JObject.Parse(sJson);
                // Handle the 'Method' 
                string method = oStratum["method"].ToString();
                int id = (int)Saved.Code.Common.GetDouble(oStratum["id"].ToString());
                dynamic params1 = oStratum["params"];
                if (method == "mining.altruism")
                {
                    var bbp_address = params1[0];
                    var poolArray = "";
                    var poolPorts = "";
                    if (!fUseLocalXMR)
                    {
                        poolArray = GetBMSConfigurationKeyValue("XMRExternalPool");
                        poolPorts = GetBMSConfigurationKeyValue("XMRPort");
                    }
                    else 
                    {
                        poolArray = GetBMSConfigurationKeyValue("PoolDNS");
                        poolPorts = GetBMSConfigurationKeyValue("XMRPort");
                    }
                    var poolPubCharityAddress = GetBMSConfigurationKeyValue("MoneroAddress");
                    var poolName = GetBMSConfigurationKeyValue("PoolDNS");
                    var json = "{ \"id\": null, \"method\": \"mining.set_altruism\", \"params\": [\""
                        + poolArray + "\",\"" + poolPorts + "\",\"" + poolPubCharityAddress + "\",\"" + poolName + "\",\"true\"]"
                        + ", \"pools\": \"" + poolArray + "\", \"charityaddress\": \"" + poolPubCharityAddress + "\"}\r\n";
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    bool f4 = Send(oClient, bytes, socketid);
                    return f4;
                }
                else if (method == "mining.authorize")
                {
                    WorkerInfo w = GetWorker(socketid);
                    w.bbpaddress = params1[0];
                    w.moneroaddress = params1[1];
                    dictWorker[socketid] = w;
                    int z = 0;
                    if (w.bbpaddress != null && w.bbpaddress.Length > 10)
                    {
                        Ban(socketid, -1,out z);
                    }
                    PersistWorker(w);

                    var json = "{ \"id\": " + id.ToString() + ", \"result\": true, \"error\": \"\" }\r\n";
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    bool f5 = Send(oClient, bytes, socketid);
                    return f5;
                }
                else if (method == "mining.subscribe")
                {
                    // Increment subscription counter
                    int diff = 1;
                    string subid = "1";
                    string extranonce1 = "0";
                    string extranonce2size = "0";

                    string json = "{ \"id\": " + id.ToString() + ", \"result\": [[[\"mining.set_difficulty\",\"" + diff.ToString()
                        + "\"],[\"mining.notify\",\"" + subid + "\"]],\"" + extranonce1 + "\",\"" + extranonce2size + "\"],\"error\": null}\r\n";

                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    bool f1 = Send(oClient, bytes, socketid);
                    if (!f1)
                        return f1;

                    WorkerInfo w = GetWorker(socketid);
                    if (w.bbpaddress == "" || w.bbpaddress == null)
                        return true;

                    bool f2 = SendMiningJob(oClient, w.bbpaddress, socketid);
                    return f2;
                }
                else if (method == "keepalived")
                {
                    return true;
                }
                else if (method == "mining.submit")
                {
                    lock (cs_message)
                    {
                        // Solution from miner to pool
                        var userid = params1[0];
                        var jobid = params1[1];
                        string rxheader = params1[2];
                        var jobtime = params1[3];
                        string rxkey = params1[4];
                        string bbphash = params1[5];
                        var sucbbp = params1[6];
                        var failbbp = params1[7];
                        var sucxmr = params1[8];
                        var failxmr = params1[9];
                        var sucxmrc = params1[10];
                        var failxmrc = params1[11];
                        WorkerInfo w = GetWorker(socketid);
                        if (w.bbpaddress == null || w.bbpaddress == "")
                        {
                            return true;
                        }
                        // Test the block
                        byte[] oRX = StringToByteArr(bbphash);

                        if (w.difficulty == 0)
                            w.difficulty = MIN_DIFF;

                        double nDiff = FullTest(oRX);
                        string out_rx = "";
                        string out_rx_root = "";
                        // Store the result on the work record
                        GetRandomXAudit(rxheader, rxkey, ref out_rx, ref out_rx_root);
                        string revBBPHash = ReverseHexString(bbphash);
                        bool fPassed = false;
                        if (revBBPHash == out_rx && nDiff >= 1 && nDiff < w.difficulty)
                        {
                            w.difficulty = 1;
                        }
                        int z = 0;
                        fPassed = (revBBPHash == out_rx && nDiff >= w.difficulty);
                        double iBanlevel = 0;
                        if (fPassed)
                        {
                            iBanlevel = Ban(socketid, -1, out z);
                        }
                        else
                        {
                            iBanlevel = Ban(socketid, .25, out z);
                        }
                        // If the rx_root matches the current bbphash, we know they solved the share (because the blakehash requires the current bbp prev hash)
                        // And, we also check that they met the difficulty level
                        // Move the record from "job" to "share"
                        int nShareAdj = 0;
                        int nFailAdj = 0;
                        string sql = "Select count(*) ct from Job (nolock) where BBPHash = @bbphash";
                        SqlCommand command = new SqlCommand(sql);
                        command.Parameters.AddWithValue("@bbphash", bbphash);
                        double nCount = gData.GetScalarDouble(command, "ct", false);

                        if (nCount > 0)
                        {
                            fPassed = false;
                        }

                        if (fPassed)
                        {
                            w.priorsolvetime = w.solvetime;
                            w.solvetime = UnixTimeStamp();
                            dictWorker[socketid] = w;
                        }

                        // Batch Exec
                        int nVerified = 0;
                        nVerified = fPassed ? 1 : 0;

                        sql = "Update Job set bbphash='" + bbphash + "',Verified='" + nVerified.ToString() + "',solvedtime=getdate(),solvedDiff='" + nDiff.ToString() 
                            + "',banlevel='" + iBanlevel.ToString() + "' where jobid='" + w.jobid.ToString()  + "'";
                        
                        BatchExec(sql);

                        if (fPassed) 
                            nShareAdj = w.difficulty;
                        else
                            nFailAdj = 1;

                        // Insert the winning share in 'Shares'
                        PoolCommon.InsShare(w.bbpaddress, nShareAdj, nFailAdj, _template.height, 0, 0);

                        var sResult = fPassed ? "" : "ERROR";
                        var sErr = fPassed ? "null" : "\"ERR1\"";

                        var json = "{ \"id\": 100, \"result\": \"" + sResult + "\",\"error\": " + sErr + ", \"method\": \"submitresponse\", \"error2\": " + sErr + " }\r\n";
                        // Return with pass fail
                        byte[] bytes = Encoding.ASCII.GetBytes(json);
                        bool f11 = Send(oClient, bytes, socketid);
                        string revTarget = ReverseHexString(_template.target);

                        // Check to see if this share actually solved the block:
                        NBitcoin.uint256 uBBP = new NBitcoin.uint256(revBBPHash);
                        NBitcoin.uint256 uTarget = new NBitcoin.uint256(_template.target);
                        NBitcoin.arith256 aBBP = new NBitcoin.arith256(uBBP);
                        NBitcoin.arith256 aTarget = new NBitcoin.arith256(uTarget);


                        int nTest = aBBP.CompareTo(aTarget);

                        if (aBBP.CompareTo(aTarget) == -1)
                        {
                            // We solved the block
                            string poolAddress = GetBMSConfigurationKeyValue("PoolAddress");
                            string hex = GetBlockForStratumHex(poolAddress, rxkey, rxheader);
                            bool fSuccess = SubmitBlock(hex);
                            if (fSuccess)
                            {
                                sql = "Update Share Set Solved=1 where height=@height";
                                command = new SqlCommand(sql);
                                //command.Parameters.AddWithValue("@bbpid", jobid);
                                command.Parameters.AddWithValue("@height", _template.height);
                                gData.ExecCmd(command, false, false, false);
                            }
                            Thread.Sleep(5000);  // Give some time for blockchain to advance
                            _template.updated = 0; // Forces us to get a new block
                            GetBlockForStratum();
                        }
                        else if (!fPassed)
                        {
                            GetBlockForStratum();
                            w.Broadcast = true;
                        }
                        return true;
                    }

                }
                else
                {
                    if (method=="login")
                    {
                        return true;
                    }
                    Log("Unknown pool method: " + method);
                    return true;
                }
            }
            catch (Exception ex)
            {
                bool fPrint = !ex.Message.Contains("being aborted");
                if (fPrint)
                      Log("POOL DISTRESS, HandleSocket: " + ex.Message+ ", JSON: " + sJson);
                bool fIllegal = ex.Message.Contains("Unexpected character encountered");
                if (fIllegal)
                {
                    try
                    {
                        oClient.Close();
                    }
                    catch (Exception ex3) { }
                    return false;
                }

                return true;
            }
        }

        private void minerThread(Socket client, int oldthid)
        {
            string socketid = client.RemoteEndPoint.ToString();

            try
            {
                client.ReceiveTimeout = 7777;
                client.SendTimeout = 7777;
                IncThreadCount(1);

                while (true)
                {
                    int size = 0;
                    byte[] data = new byte[65535];

                    WorkerInfo w1 = GetWorker(socketid);
                    if (w1.Broadcast)
                    {
                        SendMiningJob(client, w1.bbpaddress, socketid);
                    }
                    try
                    {
                        size = client.Receive(data);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("An existing connection was forcibly closed"))
                        {
                            Console.WriteLine("ConnectionClosed");
                            IncThreadCount(-1);
                            try
                            {
                                dictWorker.Remove(socketid);
                            }
                            catch(Exception ex4)
                            {

                            }
                            return;
                        }
                        Console.WriteLine("Error occurred while receiving data " + ex.Message);
                    }
                    int nRecElapsed2 = UnixTimeStamp() - w1.receivedtime;

                    if (size > 0)
                    {
                        w1.receivedtime = UnixTimeStamp();
                        dictWorker[socketid] = w1;
                    }

                    int nRecElapsed = UnixTimeStamp() - w1.receivedtime;
                    if (nRecElapsed > (60 * 5) && (w1.bbpaddress == null || w1.bbpaddress == ""))
                    {
                        try
                        {
                            //Log("MinerThread::Closing with Empty BBP Address " + ToString() + " due to inactivity. ");
                            client.Close();
                            dictWorker.Remove(socketid);
                        }
                        catch (Exception ex2)
                        {
                            Log("MinerThread1 " + socketid + ", " + ex2.Message);
                        }
                        IncThreadCount(-1);
                        return;

                    }
                    if (nRecElapsed > (60 * 30))
                    {
                        // This thread has not done anything for a long time.
                        try
                        {
                            Log("MinerThread2::Closing " + socketid + " due to inactivity. ");
                            client.Close();
                            dictWorker.Remove(socketid);
                        }
                        catch (Exception ex2)
                        {
                            Log("MinerThread3 " + socketid + ", " + ex2.Message);
                        }
                        IncThreadCount(-1);
                        return;
                    }

                    if (size > 0)
                    {
                        string sData = Encoding.UTF8.GetString(data, 0, data.Length);
                        sData = sData.Replace("\0", "");
                        // The Stratum data is first split by \r\n
                        string[] vData = sData.Split("\n");
                        for (int i = 0; i < vData.Length; i++)
                        {
                            string sJson = vData[i];
                            bool f10 = HandleSocket(sJson, client, socketid);
                        }
                    }
                    Thread.Sleep(1000);
                    // keep the port open
                }
            }
            catch (Exception ex)
            {
                    if (ex.Message.Contains("bounds of the array"))
                    {
                        int z = 0;
                        Ban(socketid, 1, out z);
                    }
                    else if (ex.Message.Contains("being aborted") || ex.Message.Contains("forcibly closed"))
                    {
                        // noop
                    }
                    else
                    {
                        Log("minerThread4 : " + ex.Message);
                    }
            }
            IncThreadCount(-1);
        }

        static int iStart = 0;
        void PoolService()
        {
            // Services - Executes batch jobs
            while (true)
            {
                if (iStart == 0)
                {
                    GetBlockForStratum();
                    iStart++;
                }
                Thread.Sleep(60000);
                GroupShares();
                Leaderboard();
                Pay();
                PurgeSockets(false);
            }
        }

        void InitializePool()
        {
            retry:

            TcpListener listener = null;

            try
            {
                Console.WriteLine("Starting Pool...");
                IPAddress ipAddress = IPAddress.Parse(GetBMSConfigurationKeyValue("bindip"));
                listener = new TcpListener(ipAddress, (int)GetDouble(GetBMSConfigurationKeyValue("PoolPort")));
                listener.Start();
            }
            catch (Exception ex1)
            {
                Log("Problem starting pool:" + ex1.Message);
            }


            while (true)
            {
                try
                {
                    Socket client = listener.AcceptSocket();
                    string socketid = client.RemoteEndPoint.ToString();
                    int iLastReceived = 0;
                    double iLevel = Ban(socketid, .5, out iLastReceived);
                    int nElapsed = UnixTimeStamp() - iLastReceived;
                    if (nElapsed > 5)
                    {
                        Ban(socketid, -1, out iLastReceived);
                    }

                    if (iLevel < 256)
                    {
                        iID++;
                        ThreadStart starter = delegate { minerThread(client, iID); };
                        var childSocketThread = new Thread(starter);
                        IncThreadCount(1);
                        childSocketThread.Start();
                        //Log("Accept " + socketid);

                    }
                    else
                    {
                         PoolCommon.CloseSocket(client);
                         //Ban(socketid, -1);
                    }
                    //Thread.Sleep(1000);
                }
                catch (ThreadAbortException abortException)
                {
                    Log("Shutting down pool...");
                    return;
                    //Thread.ResetAbort();
                    //goto retry;
                }
                catch(Exception ex)
                {
                    Log("InitializePool 1.2:" + ex.Message);
                    if (ex.Message.Contains("aborted"))
                    {
                        Log("  Shutting off pool forcefully...");
                        return;
                    }
                    //Thread.ResetAbort();
                    Thread.Sleep(5000);
                    goto retry;
                }
            }
        }



        public Pool()
        {
            var t = new Thread(InitializePool);
            t.Start();
            var t1 = new Thread(PoolService);
            t1.Start();
        }
    }
}