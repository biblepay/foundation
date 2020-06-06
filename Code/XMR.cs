using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved.Code
{
    public class XMRPool
    {
        private XMRJob RetrieveXMRJob(double jobid)
        {
            if (dictJobs.ContainsKey(jobid))
            {
                return dictJobs[jobid];
            }
            XMRJob x = new XMRJob();
            x.timestamp = UnixTimeStamp();
            x.jobid = jobid;
            return x;
        }

        private void PutXMRJob(XMRJob x)
        {
            if (x.jobid == 0)
                return;
            dictJobs[x.jobid] = x;
        }
        private bool SendXMRPacketToMiner(Socket oClient, byte[] oData, int iSize, string socketid)
        {
            try
            {
                oClient.Send(oData, iSize, SocketFlags.None);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("was aborted"))
                {

                }
                else
                {
                    bool fPrint = !(ex.Message.Contains("A connection attempt failed because the connected party did not properly respond after a period of time") || ex.Message.Contains("An existing connection was forcibly closed"));
                    if (fPrint)
                        Log("SEND " + ex.Message);
                }
                return false;
            }
        }
        public bool SubmitBiblePayShare(double nJobID)
        {
            try
            {
                XMRJob x = RetrieveXMRJob(nJobID);
                if (x.hash == null || x.hash == "")
                    return false;

                byte[] oRX = PoolCommon.StringToByteArr(x.hash);
                double nSolutionDiff = PoolCommon.FullTest(oRX);
                bool fUnique = PoolCommon.IsUnique(x.hash);
                string revBBPHash = PoolCommon.ReverseHexString(x.hash);
                // Check to see if this share actually solved the block:
                NBitcoin.uint256 uBBP = new NBitcoin.uint256(revBBPHash);
                NBitcoin.uint256 uTarget = new NBitcoin.uint256(_pool._template.target);
                NBitcoin.arith256 aBBP = new NBitcoin.arith256(uBBP);
                NBitcoin.arith256 aTarget = new NBitcoin.arith256(uTarget);
                int nTest = aBBP.CompareTo(aTarget);

                if (aBBP.CompareTo(aTarget) == -1)
                {
                    if (false)
                    {
                        Log("Submitting JOBID " + x.jobid + " with jobidsubmit " + x.jobid.ToString() + " with nonce " +
                            x.nonce + " at height " + _pool._template.height.ToString() + " seed " + x.seed + " with target " + _pool._template.target + " and solution " + x.solution);
                        Log("Submitting RXHash " + revBBPHash);
                    }
                    // We solved the block
                    string poolAddress = GetBMSConfigurationKeyValue("PoolAddress");
                    string hex = PoolCommon.GetBlockForStratumHex(poolAddress, x.seed, x.solution);
                    bool fSuccess = PoolCommon.SubmitBlock(hex);
                    if (fSuccess)
                    {
                        string sql = "Update Share Set Solved=1 where height=@height";
                        SqlCommand command = new SqlCommand(sql);
                        command.Parameters.AddWithValue("@height", _pool._template.height);
                        gData.ExecCmd(command, false, false, false);
                        Log("SUBMIT_SUCCESS: Success for nonce " + x.nonce + " at height " + _pool._template.height.ToString() + " hex " + hex);
                    }
                    else
                    {
                        Log("SUBMITBLOCK: Tried to submit the block for nonce " + x.nonce + " and target " + _pool._template.target + " with seed " + x.seed + " and solution " + x.solution + " with hex " + hex + " and failed");
                    }
                    try
                    {
                        if (dictJobs.Count > 10000)
                        {
                            dictJobs.Clear();
                        }
                        dictJobs.Remove(nJobID);
                    }
                    catch (Exception ex1)
                    {
                        Log("cant find the job " + x.jobid.ToString());
                    }

                    Thread.Sleep(1000);  // Give some time for blockchain to advance
                    _pool._template.updated = 0; // Forces us to get a new block
                    PoolCommon.GetBlockForStratum();
                }
                else
                {
                    PoolCommon.GetBlockForStratum();
                }
                return true;
            }
            catch(Exception ex)
            {
                Log("Unable to submit bbp share:  " + ex.Message);
            }
            return false;
        }
            
        private void IncTitheNbr()
        {
            PoolCommon.iTitheNumber++;
            if (PoolCommon.iTitheNumber % 50 == 0)
            {
                // Launch a new seed
                Random r = new Random();
                int rInt = r.Next(0, 10);
                PoolCommon.iTitheModulus = rInt;
            }
        }

        private void PersistWorker(WorkerInfo w)
        {
            try
            {
                if (w.IP == null || w.IP == "")
                    return;
                string sql = " IF NOT EXISTS (SELECT moneroaddress FROM worker WHERE moneroaddress='"
                          + w.moneroaddress + "') BEGIN \r\n INSERT INTO Worker (id,added,moneroaddress,bbpaddress,ip) values (newid(),getdate(),'"
                          + Saved.Code.BMS.PurifySQL(w.moneroaddress, 255) + "','" + Saved.Code.BMS.PurifySQL(w.bbpaddress, 255) + "','" + GetIPOnly(w.IP) + "') END";
                gData.Exec(sql);
            }
            catch(Exception ex)
            {
                Log("Exception PW " + ex.Message);
            }
        }


        private void minerXMRThread(Socket client, TcpClient t, string socketid)
        {
            bool fCharity = false;
            string bbpaddress = "";
            string moneroaddress = "";
            double nTrace = 0;
            try
            {
                client.ReceiveTimeout = 7777;
                client.SendTimeout = 5000;

                while (true)
                {
                    int size = 0;
                    byte[] data = new byte[256000];

                    try
                    {
                        size = client.Receive(data);
                        nTrace = 1;
                    }
                    catch (ThreadAbortException abortException)
                    {
                        Log("XMR thread(2) is going down...", true);
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("An existing connection was forcibly closed"))
                        {
                            Console.WriteLine("ConnectionClosed");
                            return;
                        }
                        else if (ex.Message.Contains("was aborted"))
                        {
                            return;
                        }
                        Console.WriteLine("Error occurred while receiving data " + ex.Message);
                    }
                    if (size > 0)
                    {
                        nTrace = 2;
                        string sData = Encoding.UTF8.GetString(data, 0, data.Length);
                        sData = sData.Replace("\0", "");
                        // The Stratum data is first split by \r\n
                        string[] vData = sData.Split("\n");
                        for (int i = 0; i < vData.Length; i++)
                        {
                            string sJson = vData[i];
                            if (sJson.Contains("submit"))
                            {
                                // See if this is a biblepay share:
                                if (PoolCommon.fMonero2000)
                                {
                                    string out_rx = "";
                                    string out_rx_root = "";
                                    nTrace = 3;
                                    // Store the result on the work record
                                    JObject oStratum = JObject.Parse(sJson);
                                    string nonce = "00000000" + oStratum["params"]["nonce"].ToString();
                                    double nJobID = GetDouble(oStratum["params"]["job_id"].ToString());
                                    string hash = oStratum["params"]["result"].ToString();
                                    XMRJob xmrJob = RetrieveXMRJob(nJobID);
                                    string rxheader = xmrJob.blob;
                                    string rxkey = xmrJob.seed;
                                    if (rxheader == null)
                                    {
                                        //Log("cant even find the job " + nJobID.ToString());
                                        PoolCommon.WorkerInfo wban = PoolCommon.Ban(socketid, 1, "CANT-FIND-JOB");
                                    }
                                    if (rxheader != null)
                                    {
                                        nTrace = 4;
                                        nonce = nonce.Substring(8, 8);
                                        xmrJob.solution = rxheader.Substring(0, 78) + nonce + rxheader.Substring(86, rxheader.Length - 86);
                                        xmrJob.hash = oStratum["params"]["result"].ToString();
                                        xmrJob.hashreversed = PoolCommon.ReverseHexString(hash);
                                        xmrJob.nonce = nonce;
                                        xmrJob.bbpaddress = bbpaddress;
                                        xmrJob.moneroaddress = moneroaddress;
                                       
                                        if (false)
                                        {
                                            PoolCommon.GetRandomXAudit(xmrJob.solution, xmrJob.seed, ref out_rx, ref out_rx_root);
                                            bool fMatches = xmrJob.hashreversed == out_rx_root;
                                            // out_rx_root should contain the RandomX hash; out_rx contains the blakehash
                                            // Nonce should be placed in monero location 78,8
                                            //{"id":55,"jsonrpc":"2.0","method":"submit","params":{"id":"277677767004670","job_id":"536896777408374","nonce":"3f400200","result":"dd27f58fd8064c573000c21b7bc2eae95a9d01b62671ce43402d61bac9070000"}}
                                            // Spec 2.0 (allow fully compatible xmr merge mining)
                                            // If out_rx_root > BBP_DIFF level, accept as a BBP share
                                        }
                                        nTrace = 5;
                                        PutXMRJob(xmrJob);
                                        nTrace = 6;
                                        SubmitBiblePayShare(xmrJob.jobid);
                                        nTrace = 7;
                                    }
                                }
                            }
                            else if (sJson.Contains("login"))
                            {
                                //{"id":1,"jsonrpc":"2.0","method":"login","params":{"login":"41s2xqGv4YLfs5MowbCwmmLgofywnhbazPEmL2jbnd7p73mtMH4XgvBbTxc6fj4jUcbxEqMFq7ANeUjktSiZYH3SCVw6uat","pass":"x","agent":"bbprig/5.10.0 (Windows NT 6.1; Win64; x64) libuv/1.34.0 gcc/9.2.0","algo":["cn/0","cn/1","cn/2","cn/r","cn/fast","cn/half","cn/xao","cn/rto","cn/rwz","cn/zls","cn/double","","cn-lite/0","cn-lite/1","cn-heavy/0","cn-heavy/tube","cn-heavy/xhv","cn-pico","cn-pico/tlo","rx/0","rx/wow","rx/loki","rx/arq","rx/sfx","rx/keva","argon2/chukwa","argon2/wrkz","astrobwt"]}}
                                nTrace = 8;
                                JObject oStratum = JObject.Parse(sJson);
                                dynamic params1 = oStratum["params"];
                                if (PoolCommon.fMonero2000)
                                {
                                    moneroaddress = params1["login"].ToString();
                                    bbpaddress = params1["pass"].ToString();
                                    if (bbpaddress.Length != 34 || moneroaddress.Length < 95)
                                    {
                                        PoolCommon.iXMRThreadCount--;
                                        client.Close();
                                        PoolCommon.WorkerInfo wban = PoolCommon.Ban(socketid, 1, "BAD-CONFIG");
                                        return;
                                    }
                                    WorkerInfo w = PoolCommon.GetWorker(socketid);
                                    w.moneroaddress = moneroaddress;
                                    w.bbpaddress = bbpaddress;
                                    w.IP = GetIPOnly(socketid);
                                    PoolCommon.SetWorker(w, socketid);
                                    nTrace = 9;
                                    PersistWorker(w);
                                }
                                nTrace = 10;
                                PoolCommon.iTitheNumber++;
                                var poolPubCharityAddress = GetBMSConfigurationKeyValue("MoneroAddress");
                                bool fTithe = (moneroaddress.Length > 10 && PoolCommon.iTitheNumber % 10 == 0);
                                nTrace = 11;
                                if (fTithe)
                                {
                                    string newData = sData.Replace(moneroaddress, poolPubCharityAddress);
                                    data = Encoding.ASCII.GetBytes(newData);
                                    size = newData.Length;
                                    fCharity = true;
                                    nTrace = 12;
                                }
                                else
                                {
                                    fCharity = false;
                                }
                            }
                            else if (sJson != "")
                            {
                                Console.WriteLine(sJson);
                            }
                        }

                        // Miner->XMR Pool
                        nTrace = 13;
                        Stream stmOut = t.GetStream();
                        stmOut.Write(data, 0, size);
                        nTrace = 14;
                    }
                    else
                    {
                        if (true)
                        {
                            // Keepalive (prevents the pool from hanging up on the miner)
                            nTrace = 15;
                            var json = "{ \"id\": 0, \"method\": \"keepalived\", \"arg\": \"na\" }\r\n";
                            data = Encoding.ASCII.GetBytes(json);
                            Stream stmOut = t.GetStream();
                            stmOut.Write(data, 0, json.Length);
                        }
                    }

                    // In from XMR Pool -> Miner
                    nTrace = 16;
                    Stream stmIn = t.GetStream();
                    nTrace = 17;
                    byte[] bIn = new byte[128000];
                    nTrace = 18;
                    int bytesIn = 0;

                    try
                    {
                        t.ReceiveTimeout = 4777;
                        t.SendTimeout = 5000;
                        nTrace = 19;
                        bytesIn = stmIn.Read(bIn, 0, 127999);
                        if (bytesIn > 0)
                        {
                            nTrace = 20;
                            string sData = Encoding.UTF8.GetString(bIn, 0, bytesIn);
                            sData = sData.Replace("\0", "");
                            string[] vData = sData.Split("\n");
                            for (int i = 0; i < vData.Length; i++)
                            {
                                string sJson = vData[i];
                                if (sJson.Contains("result"))
                                {
                                    nTrace = 21;
                                    WorkerInfo w = PoolCommon.GetWorker(socketid);
                                    PoolCommon.SetWorker(w, socketid);
                                    JObject oStratum = JObject.Parse(sJson);
                                    string status = oStratum["result"]["status"].ToString();
                                    int id = (int)GetDouble(oStratum["id"]);
                                    if (id == 1 && status == "OK" && sJson.Contains("blob"))
                                    {
                                        // BiblePay Pool to Miner
                                        nTrace = 22;
                                        double nJobId = GetDouble(oStratum["result"]["job"]["job_id"].ToString());
                                        XMRJob x = RetrieveXMRJob(nJobId);
                                        x.blob = oStratum["result"]["job"]["blob"].ToString();
                                        x.target = oStratum["result"]["job"]["target"].ToString();
                                        x.seed = oStratum["result"]["job"]["seed_hash"].ToString();
                                        PutXMRJob(x);
                                    }
                                    else if (id > 1 && status == "OK")
                                    {
                                        // They solved an XMR
                                        int iCharity = fCharity ? 1 : 0;
                                        nTrace = 24;
                                        PoolCommon.InsShare(bbpaddress, 1, 0, _pool._template.height, 1, iCharity, moneroaddress);
                                    }
                                    else if (id > 1 && status != "OK" && status != "KEEPALIVED")
                                    {
                                        nTrace = 25;
                                        PoolCommon.InsShare(bbpaddress, 0, 1, _pool._template.height, 0, 0, moneroaddress);
                                    }
                                }
                                else if (sJson.Contains("submit"))
                                {
                                    // Noop
                                }
                                else if (sJson.Contains("\"method\":\"job\""))
                                {
                                    nTrace = 26;
                                    JObject oStratum = JObject.Parse(sJson);
                                    nTrace = 27;
                                    double nJobId = GetDouble(oStratum["params"]["job_id"].ToString());
                                    XMRJob x = RetrieveXMRJob(nJobId);
                                    x.blob = oStratum["params"]["blob"].ToString();
                                    x.target = oStratum["params"]["target"].ToString();
                                    x.seed = oStratum["params"]["seed_hash"].ToString();
                                    PutXMRJob(x);
                                }
                                else if (sJson != "")
                                {
                                    Console.WriteLine(sJson);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("being aborted"))
                        {
                            //PoolCommon.CloseSocket(client);
                            //return;
                        }
                        else if (!ex.Message.Contains("did not properly respond"))
                        {
                            Log("minerXMRThread[0]: Trace=" + nTrace.ToString() + ":" + ex.Message);
                        }
                    }
                    if (bytesIn > 0)
                    {
                        // This goes back to the miner
                        SendXMRPacketToMiner(client, bIn, bytesIn, socketid);
                    }
                    
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException abortException)
            {
                Log("minerXMRThread is going down...", true);
                return;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("was aborted"))
                {
                    // Noop
                }
                else if (ex.Message.Contains("forcibly closed"))
                {

                }
                else if (!ex.Message.Contains("being aborted"))
                {
                    Log("minerXMRThread2 : " + ex.Message);
                }
            }

            PoolCommon.iXMRThreadCount = iXMRThreadCount - .91;

        }

        void InitializeXMR()
        {

            retry:
            TcpListener listener = null;
            try
            {
                {
                    IPAddress ipAddress = IPAddress.Parse(GetBMSConfigurationKeyValue("bindip"));
                    listener = new TcpListener(IPAddress.Any, (int)GetDouble(GetBMSConfigurationKeyValue("XMRPort")));
                    listener.Start();
                }
            }
            catch (Exception ex1)
            {
                Log("Problem starting XMR pool:" + ex1.Message);
            }

            while (true)
            {
                //  Complimentary outbound socket
                try
                {
                    Thread.Sleep(10);
                    if (listener.Pending())
                    {
                        Socket client = listener.AcceptSocket();
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        
                        string socketid = client.RemoteEndPoint.ToString();
                        PoolCommon.WorkerInfo wban = PoolCommon.Ban(socketid, .25, "XMR-Connect");
                        if (!wban.banned)
                        {
                            PoolCommon.iXMRThreadID++;
                            TcpClient tcp = new TcpClient();
                            tcp.Connect(GetBMSConfigurationKeyValue("XMRExternalPool"), (int)GetDouble(GetBMSConfigurationKeyValue("XMRExternalPort")));
                            ThreadStart starter = delegate { minerXMRThread(client, tcp, socketid); };
                            var childSocketThread = new Thread(starter);
                            PoolCommon.iXMRThreadCount++;
                            childSocketThread.Start();
                        }
                        else
                        {
                            // They are already banned
                            PoolCommon.CloseSocket(client);
                        }
                    }
                }
                catch (ThreadAbortException abortException)
                {
                    Log("XMR Pool is going down...");
                    return;
                }
                catch (Exception ex)
                {
                    Log("InitializeXMRPool v1.3: " + ex.Message);
                    Thread.Sleep(5000);
                    goto retry;
                }
            }
        }

        public XMRPool()
        {
            var t = new Thread(InitializeXMR);
            t.Start();
        }
    }
}

