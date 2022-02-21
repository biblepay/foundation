using System;
using System.Data;
using System.Text;

namespace Saved.Code
{

    public class HexadecimalEncoding
    {
        public static string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                //Note: X for upper, x for lower case letters
                sb.Append(Convert.ToInt32(t).ToString("x"));
            }
            return sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }
    }
    public static class Proposals
    {


        private static string GJE(string sKey, string sValue, bool bIncludeDelimiter, bool bQuoteValue)
        {
            // This is a helper for the Governance gobject create method
            string sQ = "\"";
            string sOut = sQ + sKey + sQ + ":";
            if (bQuoteValue)
            {
                sOut += sQ + sValue + sQ;
            }
            else
            {
                sOut += sValue;
            }
            if (bIncludeDelimiter) sOut += ",";
            return sOut;
        }

        public static string gobject_serialize_internal(int nStartTime, int nEndTime, string sName, string sAddress, string sAmount, string sURL, string sExpenseType)
        {

            // gobject prepare 0 1 EPOCH_TIME HEX
            string sType = "1"; //Proposal
            string sQ = "\"";
            string sJson = "[[" + sQ + "proposal" + sQ + ",{";
            sJson += GJE("start_epoch", nStartTime.ToString(), true, false);
            sJson += GJE("end_epoch", nEndTime.ToString(), true, false);
            sJson += GJE("name", sName, true, true);
            sJson += GJE("payment_address", sAddress, true, true);
            sJson += GJE("payment_amount", sAmount, true, false);
            sJson += GJE("type", sType, true, false);
            sJson += GJE("expensetype", sExpenseType, true, true);
            sJson += GJE("url", sURL, false, true);
            sJson += "}]]";
            // make into hex
            
            string Hex = HexadecimalEncoding.StringToHex(sJson);

            return Hex;
        }
        public static bool gobject_serialize(bool fTestNet, string sUserID, string sUserName,  
            string sName, string sAddress, string sAmount, string sURL, string sExpenseType)
        {
            string sChain = fTestNet ? "test" : "main";
            try
            {
                int unixStartTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                int unixEndTime = unixStartTimestamp + (60 * 60 * 24 * 7);
                
                string sHex = gobject_serialize_internal(unixStartTimestamp,unixEndTime,sName, sAddress, sAmount, sURL, sExpenseType);

                string sID = Guid.NewGuid().ToString();

                string sql = "Insert Into Proposal (id,ExpenseType,UserId,UserName,URL,name,Address,amount,unixstarttime,"
                    + "preparetxid,added,updated,hex,chain) "
                    + " values ('" + sID + "','" + BMS.PurifySQL(sExpenseType, 50)
                    + "','" + sUserID + "','" + sUserName
                    + "','" + BMS.PurifySQL(sURL, 150)
                    + "','" + BMS.PurifySQL(sName, 100)
                    + "','" + BMS.PurifySQL(sAddress, 80)
                    + "','" + BMS.PurifySQL(sAmount, 50)
                    + "','" + unixStartTimestamp.ToString()
                    + "',null,getdate(),getdate(),'" + sHex + "','" + sChain + "')";
                Common.gData.Exec(sql);
                gobject_prepare(fTestNet, sID, unixStartTimestamp, sHex);
                return true;
            }catch(Exception ex)
            {
                Common.Log("Issue with Proposal Submit:: " + ex.Message);
                return false;
            }

        }

        public static void gobject_prepare(bool fTestNet, string sID, int StartTimeStamp, string sHex)
        {
            // gobject prepare
            string sArgs = "0 1 " + StartTimeStamp.ToString() + " " + sHex;
            string sCmd1 = "gobject prepare " + sArgs;
            object[] oParams = new object[5];
            oParams[0] = "prepare";
            oParams[1] = "0";
            oParams[2] = "1";
            oParams[3] = StartTimeStamp.ToString();
            oParams[4] = sHex;

            NBitcoin.RPC.RPCClient n = fTestNet ? WebRPC.GetTestNetRPCClient() : WebRPC.GetLocalRPCClient();

            dynamic oOut = n.SendCommand("gobject", oParams);
            string sPrepareTXID = oOut.Result.ToString();
            
            string sql4 = "Update Proposal Set PrepareTxId='" +
                sPrepareTXID + "',Updated=getdate() where id = '" 
                + sID + "'";
            Common.gData.Exec(sql4);

        }

        public static bool gobject_submit(bool fTestNet, string sID, int nProposalTimeStamp, string sHex, string sPrepareTXID)
        {
            try
            {
                if (sPrepareTXID == "")
                    return false;
                // Submit the gobject to the network - gobject submit parenthash revision time datahex collateraltxid
                string sArgs = "0 1 " + nProposalTimeStamp.ToString() + " " + sHex + " " + sPrepareTXID;
                string sCmd1 = "gobject submit " + sArgs;
                object[] oParams = new object[6];
                oParams[0] = "submit";
                oParams[1] = "0";
                oParams[2] = "1";
                oParams[3] = nProposalTimeStamp.ToString();
                oParams[4] = sHex;
                oParams[5] = sPrepareTXID;
                NBitcoin.RPC.RPCClient n = fTestNet ? WebRPC.GetTestNetRPCClient() : WebRPC.GetLocalRPCClient();
                dynamic oOut = n.SendCommand("gobject", oParams);
                string sSubmitTXID = oOut.Result.ToString();
                if (sSubmitTXID.Length > 20)
                {
                    // Update the record allowing us to know this has been submitted
                    string sql = "Update Proposal set Submitted=GetDate(),SubmitTXID='" + sSubmitTXID + "' where id = '" + sID + "'";
                    Common.gData.Exec(sql);
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static void SubmitProposals(bool fTestNet)
        {
            string sChain = fTestNet ? "test" : "main";
            string sql = "Select * from Proposal where CHAIN = '" + sChain + "' and submitted is null and Updated < getdate()-.02";
            DataTable dt = Common.gData.GetDataTable2(sql);
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                bool fSubmitted = Proposals.gobject_submit(fTestNet, dt.Rows[y]["id"].ToString(), (int)Common.GetDouble(dt.Rows[y]["unixstarttime"].ToString()),
                    dt.Rows[y]["hex"].ToString(), dt.Rows[y]["preparetxid"].ToString());
            }
        }


    }
}