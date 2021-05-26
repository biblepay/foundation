using Microsoft.VisualBasic;
using MimeKit;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using static Saved.Code.PoolCommon;
using static Saved.Code.Common;

namespace Saved.Code
{

    public class MyWebClient : System.Net.WebClient
    {
        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            System.Net.WebRequest w = base.GetWebRequest(uri);

            w.Timeout = 7000;
            return w;
        }
    }




    public static class WebRPC
    {
        public static string SendRawTx(string hex)
        {
            try
            {
                object[] oParams = new object[1];
                oParams[0] = hex;
                NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
                dynamic oOut = n.SendCommand("sendrawtransaction", oParams);
                string result = oOut.Result.Value;
                // To do return binary response code here; check response for fail and success
                if (result == null)
                    return "";

                return result;
            }
            catch (Exception ex)
            {
                Common.Log("SendRawTx:: " + ex.Message);
                return "";
            }
        }

        public static NBitcoin.RPC.RPCClient GetTestNetRPCClient()
        {
                NBitcoin.RPC.RPCCredentialString r = new NBitcoin.RPC.RPCCredentialString();
                System.Net.NetworkCredential t = new System.Net.NetworkCredential(GetBMSConfigurationKeyValue("testnetrpcuser"), GetBMSConfigurationKeyValue("testnetrpcpassword"));
                r.UserPassword = t;
                string sHost = GetBMSConfigurationKeyValue("testnetrpchost");
                NBitcoin.RPC.RPCClient n = new NBitcoin.RPC.RPCClient(r, sHost, NBitcoin.Network.BiblepayTest);
                return n;
        }

        private static NBitcoin.RPC.RPCClient _rpcclient = null;

        public static NBitcoin.RPC.RPCClient GetLocalRPCClient()
        {
            if (_rpcclient == null)
            {
                NBitcoin.RPC.RPCCredentialString r = new NBitcoin.RPC.RPCCredentialString();
                System.Net.NetworkCredential t = new System.Net.NetworkCredential(GetBMSConfigurationKeyValue("rpcuser"), GetBMSConfigurationKeyValue("rpcpassword"));
                r.UserPassword = t;
                string sHost = GetBMSConfigurationKeyValue("rpchost");
                NBitcoin.RPC.RPCClient n = new NBitcoin.RPC.RPCClient(r, sHost, NBitcoin.Network.BiblepayMain);
                _rpcclient = n;
                return n;
            }
            else
            {
                try
                {
                    var nbal = _rpcclient.GetBalance();
                }
                catch (Exception)
                {
                    _rpcclient = null;
                    return GetLocalRPCClient();
                }
                return _rpcclient;
            }
        }

        public static string GetNewDepositAddress()
        {
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
            string sAddress = n.GetNewAddress().ToString();
            return sAddress;
        }

        public static int GetHeight()
        {
            object[] oParams = new object[1];
            NBitcoin.RPC.RPCClient n = GetLocalRPCClient();
            dynamic oOut = n.SendCommand("getmininginfo");
            int nBlocks = (int)GetDouble(oOut.Result["blocks"]);
            return nBlocks;

        }

        

    }
}

