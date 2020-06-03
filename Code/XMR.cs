using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Saved.Code.Common;

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

        public struct XMRJob
        {
            public string blob;
            public string jobid;
            public string target;
            public string seed;
            public string solution;
            public string hash;
            public string hashreversed;
            public string bbpaddress;
            public string moneroaddress;
            public bool fNeedsSubmitted;
        }

        public static bool SubmitBiblePayShare(ref XMRJob x)
        {

            try
            {
                if (!x.fNeedsSubmitted)
                    return false;
                byte[] oRX = PoolCommon.StringToByteArr(x.hash);
                double nSolutionDiff = PoolCommon.FullTest(oRX);
                int nShareAdj = 0;
                int nFailAdj = 0;
                bool fPassed = nSolutionDiff >= 1 && PoolCommon.IsUnique(x.hash);

                if (fPassed)
                    nShareAdj = (int)nSolutionDiff;
                else
                    nFailAdj = 1;

                // Insert the winning share in 'Shares'
                PoolCommon.InsShare(x.bbpaddress, nShareAdj, nFailAdj, _pool._template.height, 0, 0, x.moneroaddress);
                string revBBPHash = PoolCommon.ReverseHexString(x.hash);
                // Check to see if this share actually solved the block:
                NBitcoin.uint256 uBBP = new NBitcoin.uint256(revBBPHash);
                NBitcoin.uint256 uTarget = new NBitcoin.uint256(_pool._template.target);
                NBitcoin.arith256 aBBP = new NBitcoin.arith256(uBBP);
                NBitcoin.arith256 aTarget = new NBitcoin.arith256(uTarget);
                int nTest = aBBP.CompareTo(aTarget);
                x.fNeedsSubmitted = false;
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
                    }
                    Thread.Sleep(5000);  // Give some time for blockchain to advance
                    _pool._template.updated = 0; // Forces us to get a new block
                    PoolCommon.GetBlockForStratum();
                }
                else if (!fPassed)
                {
                    PoolCommon.GetBlockForStratum();
                }
                return true;
            }catch(Exception ex)
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

        private void minerXMRThread(Socket client, TcpClient t, string socketid)
        {
            bool fCharity = false;
            XMRJob x = new XMRJob();

            try
            {
                client.ReceiveTimeout = 777;
                client.SendTimeout = 5000;

                while (true)
                {
                    int size = 0;
                    byte[] data = new byte[32767];

                    try
                    {
                        size = client.Receive(data);
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
                                    // Store the result on the work record
                                    string rxheader = x.blob;
                                    string rxkey = x.seed;
                                    JObject oStratum = JObject.Parse(sJson);
                                    string nonce = "00000000" + oStratum["params"]["nonce"];
                                    nonce = nonce.Substring(8, 8);
                                    x.solution = rxheader.Substring(0, 78) + nonce + rxheader.Substring(86, rxheader.Length - 86);
                                    x.hash = oStratum["params"]["result"].ToString();
                                    x.hashreversed = PoolCommon.ReverseHexString(x.hash);
                                    if (false)
                                    {
                                        PoolCommon.GetRandomXAudit(x.solution, x.seed, ref out_rx, ref out_rx_root);
                                        bool fMatches = x.hashreversed == out_rx_root;
                                        // out_rx_root should contain the RandomX hash; out_rx contains the blakehash
                                        // Nonce should be placed in monero location 78,8
                                        //{"id":55,"jsonrpc":"2.0","method":"submit","params":{"id":"277677767004670","job_id":"536896777408374","nonce":"3f400200","result":"dd27f58fd8064c573000c21b7bc2eae95a9d01b62671ce43402d61bac9070000"}}
                                        // Spec 2.0 (allow fully compatible xmr merge mining)
                                        // If out_rx_root > BBP_DIFF level, accept as a BBP share
                                    }
                                    x.fNeedsSubmitted = true;
                                }
                            }
                            else if (sJson.Contains("login"))
                            {
                                //{"id":1,"jsonrpc":"2.0","method":"login","params":{"login":"41s2xqGv4YLfs5MowbCwmmLgofywnhbazPEmL2jbnd7p73mtMH4XgvBbTxc6fj4jUcbxEqMFq7ANeUjktSiZYH3SCVw6uat","pass":"x","agent":"bbprig/5.10.0 (Windows NT 6.1; Win64; x64) libuv/1.34.0 gcc/9.2.0","algo":["cn/0","cn/1","cn/2","cn/r","cn/fast","cn/half","cn/xao","cn/rto","cn/rwz","cn/zls","cn/double","invalid","cn-lite/0","cn-lite/1","cn-heavy/0","cn-heavy/tube","cn-heavy/xhv","cn-pico","cn-pico/tlo","rx/0","rx/wow","rx/loki","rx/arq","rx/sfx","rx/keva","argon2/chukwa","argon2/wrkz","astrobwt"]}}
                                JObject oStratum = JObject.Parse(sJson);
                                dynamic params1 = oStratum["params"];
                                if (PoolCommon.fMonero2000)
                                {
                                    x.moneroaddress = params1["login"].ToString();
                                    x.bbpaddress = params1["pass"].ToString();
                                    if (x.bbpaddress.Length != 34)
                                    {
                                        PoolCommon.iXMRThreadCount--;
                                        client.Close();
                                        return;
                                    }
                                }
                                else
                                {
                                    x.moneroaddress = params1["login"].ToString();
                                    x.bbpaddress = PoolCommon.GetBBPAddress(x.moneroaddress);
                                }
                                double nPerc = PoolCommon.GetTithePercent();
                                PoolCommon.iTitheNumber++;
                                var poolPubCharityAddress = GetBMSConfigurationKeyValue("MoneroAddress");
                                bool fTithe = (nPerc < 10 && x.moneroaddress.Length > 10 && PoolCommon.iTitheNumber % 7 == 0);
                                fTithe = (x.moneroaddress.Length > 10 && PoolCommon.iTitheNumber % 10 == 0);
                                if (fTithe)
                                {
                                    string newData = sData.Replace(x.moneroaddress, poolPubCharityAddress);
                                    data = Encoding.ASCII.GetBytes(newData);
                                    size = newData.Length;
                                    fCharity = true;
                                }
                                else
                                {
                                    fCharity = false;
                                }
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
                            var json = "{ \"id\": 0, \"method\": \"keepalived\", \"arg\": \"na\" }\r\n";
                            data = Encoding.ASCII.GetBytes(json);
                            Stream stmOut = t.GetStream();
                            stmOut.Write(data, 0, json.Length);
                        }
                    }

                    // In from XMR Pool -> Miner
                    Stream stmIn = t.GetStream();
                    byte[] bIn = new byte[65535];
                    int bytesIn = 0;

                    try
                    {
                        t.ReceiveTimeout = 777;
                        t.SendTimeout = 5000;

                        bytesIn = stmIn.Read(bIn, 0, 65535);
                        if (bytesIn > 0)
                        {
                            string sData = Encoding.UTF8.GetString(bIn, 0, bytesIn);
                            sData = sData.Replace("\0", "");
                            string[] vData = sData.Split("\n");
                            for (int i = 0; i < vData.Length; i++)
                            {
                                string sJson = vData[i];
                                // Console.WriteLine(sJson);
                      
                                if (sJson.Contains("result"))
                                {
                                    JObject oStratum = JObject.Parse(sJson);
                                    string status = oStratum["result"]["status"].ToString();
                                    int id = (int)GetDouble(oStratum["id"]);
                                    if (id == 1 && status == "OK")
                                    {
                                        x.blob = oStratum["result"]["job"]["blob"].ToString();
                                        x.jobid = oStratum["result"]["job"]["job_id"].ToString();
                                        x.target = oStratum["result"]["job"]["target"].ToString();
                                        x.seed = oStratum["result"]["job"]["seed_hash"].ToString();
                                    }
                                    else if (id > 1 && status == "OK")
                                    {
                                        // They solved an XMR
                                        int iCharity = fCharity ? 1 : 0;
                                        PoolCommon.InsShare(x.bbpaddress, 0, 0, _pool._template.height, 1, iCharity, x.moneroaddress);
                                        SubmitBiblePayShare(ref x);
                                        double nPerc = PoolCommon.GetTithePercent();
                                    }
                                }
                                else if (sJson.Contains("submit"))
                                {
                                    string sTest = "";
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
                            Log("minerXMRThread:" + ex.Message);
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

            PoolCommon.iXMRThreadCount--;

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

