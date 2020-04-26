using Newtonsoft.Json.Linq;
using System;
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


        private void minerXMRThread(Socket client, TcpClient t, string socketid)
        {
            string bbpaddress = "";
            string moneroaddress = "";
            bool fCharity = false;

            try
            {
                client.ReceiveTimeout = 777;
                client.SendTimeout = 5000;

                while (true)
                {
                    int size = 0;
                    byte[] data = new byte[65535];

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
                                //{"id":30,"jsonrpc":"2.0","method":"submit","params":{"id":"723687625071416","job_id":"850418953653024","nonce":"43070300","result":"cd2868307881d164e64774294d5c326bc89003b6f168845a2f95a8d0dfba0000"}}
                            }
                            else if (sJson.Contains("login"))
                            {
                                //{"id":1,"jsonrpc":"2.0","method":"login","params":{"login":"41s2xqGv4YLfs5MowbCwmmLgofywnhbazPEmL2jbnd7p73mtMH4XgvBbTxc6fj4jUcbxEqMFq7ANeUjktSiZYH3SCVw6uat","pass":"x","agent":"bbprig/5.10.0 (Windows NT 6.1; Win64; x64) libuv/1.34.0 gcc/9.2.0","algo":["cn/0","cn/1","cn/2","cn/r","cn/fast","cn/half","cn/xao","cn/rto","cn/rwz","cn/zls","cn/double","invalid","cn-lite/0","cn-lite/1","cn-heavy/0","cn-heavy/tube","cn-heavy/xhv","cn-pico","cn-pico/tlo","rx/0","rx/wow","rx/loki","rx/arq","rx/sfx","rx/keva","argon2/chukwa","argon2/wrkz","astrobwt"]}}
                                JObject oStratum = JObject.Parse(sJson);
                                dynamic params1 = oStratum["params"];
                                moneroaddress = params1["login"].ToString();
                                bbpaddress = PoolCommon.GetBBPAddress(moneroaddress);
                                PoolCommon.iTitheNumber++;
                                if (PoolCommon.iTitheNumber % 10 == 0 && moneroaddress.Length > 10)
                                {
                                    var poolPubCharityAddress = GetBMSConfigurationKeyValue("MoneroAddress");
                                    string newData = sData.Replace(moneroaddress, poolPubCharityAddress);
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
                                //{"id":1,"jsonrpc":"2.0","error":null,"result":{"id":"329407916998887","job":
                                //{"height":2077509,"blob":"0c023aa40","job_id":"271902751405682","target":"b2df0000","algo":"rx/0",
                                //"seed_hash":"c8ef3d2c87e1591c8878da3cbaff2836511734eb49652ef4ead2dc8ba2dec8d6"},"status":"OK"}}
                                //{"id":24,"jsonrpc":"2.0","error":null,"result":{"status":"OK"}}

                                if (sJson.Contains("result"))
                                {
                                    JObject oStratum = JObject.Parse(sJson);
                                    string status = oStratum["result"]["status"].ToString();
                                    int id = (int)GetDouble(oStratum["id"]);
                                    if (id > 1 && status == "OK")
                                    {
                                        // They solved an XMR
                                        int iCharity = fCharity ? 1 : 0;
                                        PoolCommon.InsShare(bbpaddress, 0, 0, _pool._template.height, 1, iCharity);
                                    }
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
                if (!ex.Message.Contains("being aborted"))
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
                            if (PoolCommon.iXMRThreadCount > PoolCommon.iThreadCount)
                                PoolCommon.iXMRThreadCount = PoolCommon.iThreadCount;
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

