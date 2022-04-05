﻿using Newtonsoft.Json.Linq;
using System;
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
        public bool SubmitBiblePayShare(string socketid)
        {

            if (_pool._template.height < 300000)
            {
                //Chain is still syncing...
                return false;
            }

            try
            {
                XMRJob x = RetrieveXMRJob(socketid);
                if (x.hash == null || x.hash == "")
                {
                    Log("SubmitBBPShare::emptyhash", true);
                    return false;
                }

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
                        Log("SUBMIT_SUCCESS: Success for nonce " + x.nonce + " at height " 
                            + _pool._template.height.ToString() + " hex " + hex, true);
                    }
                    else
                    {
                        Log("SUBMITBLOCK: Tried to submit the block for nonce " + x.nonce + " and target " 
                            + _pool._template.target + " with seed " + x.seed + " and solution " 
                            + x.solution + " with hex " + hex + " and failed");
                    }
                   
                    _pool._template.updated = 0; // Forces us to get a new block
                    PoolCommon.GetBlockForStratum();
                }
                else
                {
                    PoolCommon.GetBlockForStratum();
                }

                try
                {
                    if (dictJobs.Count > 25000)
                    {
                        dictJobs.Clear();
                    }
                }
                catch (Exception ex1)
                {
                    Log("cant find the job " + x.socketid.ToString() + ex1.Message);
                }

                return true;
            }
            catch(Exception ex)
            {
                Log("submitshare::Unable to submit bbp share:  " + ex.Message);
            }
            return false;
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

        private static double ConvertTargetToDifficulty(XMRJob x)
        {
            string sDiff = "000000" + x.target + "0000000000000000000000000000000000000000000000000000000000000000";
            sDiff = sDiff.Substring(0, 64);
            System.Numerics.BigInteger biDiff = new System.Numerics.BigInteger(PoolCommon.StringToByteArr(sDiff));
            System.Numerics.BigInteger biMin = new System.Numerics.BigInteger(PoolCommon.StringToByteArr("0x00000000FFFF0000000000000000000000000000000000000000000000000000"));
            System.Numerics.BigInteger bidiff = System.Numerics.BigInteger.Divide(biMin, biDiff);
            double nDiff = GetDouble(bidiff.ToString());
            return nDiff;
        }

        private static double WeightAdjustedShare(XMRJob x)
        {
            if (x.difficulty <= 0)
                return 0;
            double nAdj = x.difficulty / 256000;
            return nAdj;
        }

        private static int nDebugCount = 0;
        private void minerXMRThread(Socket client, TcpClient t, string socketid)
        {
            bool fCharity = false;
            string bbpaddress = String.Empty;
            string moneroaddress = String.Empty;
            double nTrace = 0;
            string sData = String.Empty;
            string sParseData = String.Empty;

            try
            {
                client.ReceiveTimeout = 4777;
                client.SendTimeout = 3000;

                while (true)
                {
                    int size = 0;
                    byte[] data = new byte[256000];

                    if (client.Available > 0)
                    {

                        try
                        {
                            size = client.Receive(data);
                            nTrace = 1;
                        }
                        catch (ThreadAbortException)
                        {
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
                            sData = Encoding.UTF8.GetString(data, 0, data.Length);
                            sData = sData.Replace("\0", "");
                            // {"id":107,"jsonrpc":"2.0","method":"submit","params":{"id":"1","job_id":"5","nonce":"08af0200","result":"542      
                            // We are seeing nTrace==2, with a truncation occurring around position 107 having no json terminator:
                            if (sData.Contains("jsonrpc") && sData.Contains("submit") && sData.Contains("params") && sData.Length < 128)
                            {
                                if (sData.Contains("{") && sData.Contains("id") && !sData.Contains("}"))
                                {
                                    Log("XMRPool::Received " + socketid + " truncated message.  ", true);
                                    PoolCommon.iXMRThreadCount--;
                                    client.Close();
                                    PoolCommon.WorkerInfo wban = PoolCommon.Ban(socketid, 1, "BAD-CONFIG");
                                    return;
                                }
                            }

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
                                        sParseData = sJson;
                                        JObject oStratum = JObject.Parse(sJson);
                                        string nonce = "00000000" + oStratum["params"]["nonce"].ToString();
                                        double nJobID = GetDouble(oStratum["params"]["job_id"].ToString());
                                        string hash = oStratum["params"]["result"].ToString();
                                        XMRJob xmrJob = RetrieveXMRJob(socketid);
                                        string rxheader = xmrJob.blob;
                                        string rxkey = xmrJob.seed;

                                        if (rxheader == null)
                                        {
                                            //Log("cant find the job " + nJobID.ToString());
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
                                            PutXMRJob(xmrJob);
                                            SubmitBiblePayShare(xmrJob.socketid);
                                        }
                                    }
                                }
                                else if (sJson.Contains("login"))
                                {
                                    //{"id":1,"jsonrpc":"2.0","method":"login","params":{"login":"41s2xqGv4YLfs5MowbCwmmLgofywnhbazPEmL2jbnd7p73mtMH4XgvBbTxc6fj4jUcbxEqMFq7ANeUjktSiZYH3SCVw6uat","pass":"x","agent":"bbprig/5.10.0 (Windows NT 6.1; Win64; x64) libuv/1.34.0 gcc/9.2.0","algo":["cn/0","cn/1","cn/2","cn/r","cn/fast","cn/half","cn/xao","cn/rto","cn/rwz","cn/zls","cn/double","","cn-lite/0","cn-lite/1","cn-heavy/0","cn-heavy/tube","cn-heavy/xhv","cn-pico","cn-pico/tlo","rx/0","rx/wow","rx/loki","rx/arq","rx/sfx","rx/keva","argon2/chukwa","argon2/wrkz","astrobwt"]}}
                                    nTrace = 8;
                                    sParseData = sJson;
                                    if (sJson.Contains("User-Agent:") || sJson.Contains("HTTP/1.1"))
                                    {
                                        // Someone is trying to connect to the pool with a web browser?  (Instead of a miner):
                                        Log("XMRPool::Received " + socketid + " Web browser Request ", true);
                                        PoolCommon.iXMRThreadCount--;
                                        client.Close();
                                        PoolCommon.WorkerInfo wban = PoolCommon.Ban(socketid, 1, "BAD-CONFIG");
                                        return;
                                    }
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
                                        PersistWorker(w);
                                    }
                                    nTrace = 10;
                                }
                                else if (sJson != "")
                                {
                                    Console.WriteLine(sJson);
                                }
                            }

                            // Miner->XMR Pool
                            Stream stmOut = t.GetStream();
                            stmOut.Write(data, 0, size);
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
                    }

                    // ****************************************** In from XMR Pool -> Miner *******************************************************
                    nTrace = 16;
                    NetworkStream stmIn = t.GetStream();
                    nTrace = 17;
                    byte[] bIn = new byte[128000];
                    nTrace = 18;
                    int bytesIn = 0;

                    try
                    {
                        t.ReceiveTimeout = 5777;
                        t.SendTimeout = 4777;
                        nTrace = 19;

                        if (stmIn.DataAvailable)
                        {
                            bytesIn = stmIn.Read(bIn, 0, 127999);
                            if (bytesIn > 0)
                            {
                                nTrace = 20;
                                sData = Encoding.UTF8.GetString(bIn, 0, bytesIn);
                                sData = sData.Replace("\0", "");
                                string[] vData = sData.Split("\n");
                                for (int i = 0; i < vData.Length; i++)
                                {
                                    string sJson = vData[i];
                                    if (sJson.Contains("result"))
                                    {
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
                                            XMRJob x = RetrieveXMRJob(socketid);
                                            x.blob = oStratum["result"]["job"]["blob"].ToString();
                                            x.target = oStratum["result"]["job"]["target"].ToString();
                                            x.seed = oStratum["result"]["job"]["seed_hash"].ToString();
                                            x.difficulty = ConvertTargetToDifficulty(x);
                                            PutXMRJob(x);
                                        }
                                        else if (id > 1 && status == "OK")
                                        {
                                            // They solved an XMR
                                            int iCharity = fCharity ? 1 : 0;
                                            nTrace = 24;
                                            // Weight adjusted share
                                            XMRJob x = RetrieveXMRJob(socketid);
                                            double nShareAdj = WeightAdjustedShare(x);
                                            nDebugCount++;
                                            System.Diagnostics.Debug.WriteLine("solved " + nDebugCount.ToString());

                                            PoolCommon.InsShare(bbpaddress, nShareAdj, 0, _pool._template.height, nShareAdj, iCharity, moneroaddress);
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
                                        XMRJob x = RetrieveXMRJob(socketid);
                                        nTrace = 27.2;
                                        x.blob = oStratum["params"]["blob"].ToString();
                                        nTrace = 27.4;
                                        x.target = oStratum["params"]["target"].ToString();
                                        nTrace = 27.5;
                                        x.seed = oStratum["params"]["seed_hash"].ToString();
                                        nTrace = 27.6;
                                        x.difficulty = ConvertTargetToDifficulty(x);
                                        PutXMRJob(x);
                                        nTrace = 27.9;
                                    }
                                    else if (sJson != "")
                                    {
                                        Console.WriteLine(sJson);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("did not properly respond"))
                        {
                            Log("minerXMRThread[0]: Trace=" + nTrace.ToString() + ":" + ex.Message);
                            Ban(socketid, 1, ex.Message.Substring(0, 12));
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
            catch (ThreadAbortException)
            {
                // Log("minerXMRThread is going down...", true);
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
                    //This is where we see Unexpected end of content while loading JObject. Path 'params.job_id', line 1, position 72.
                    // and Unterminated string. Expected delimiter: ". Path 'params.result', line 1, position 108.
                    // and Unterminated string. Expected delimiter: ". Path 'params.result', line 1, position 144.
                    //Invalid character after parsing property name. Expected ':' but got:
                    // and Unterminated string. Expected delimiter: ". Path 'params.id', line 1, position 72.
                    Log("minerXMRThread2 v2.0: " + ex.Message + " [sdata=" + sData + "], Trace=" + nTrace.ToString() + ", PARSEDATA     \r\n" + sParseData);
                }
            }

            PoolCommon.iXMRThreadCount = iXMRThreadCount - 1;
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
                catch (ThreadAbortException)
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

