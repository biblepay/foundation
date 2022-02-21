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



    public static class Retired
    {



        /*
         * 
         * 
        // coding horror
        ///
        /// CreateProcessWithLogonW is the unmangaged method used to launch a process under the context of
        /// alternative, user provided, credentials. It is called by the managed method CreateProcessAsUser
        /// defined earlier in this class. Further information is available on MSDN under
        /// CreateProcessWithLogonW (a href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/"http://msdn.microsoft.com/library/default.asp?url=/library/en-us//a
        /// dllproc/base/createprocesswithlogonw.asp).
        ///
        /// Whether to load a full user profile(param value = 1) or perform a
        /// network only (param value = 2) logon.
        /// The application to execute (populate either this parameter
        /// or the commandLine parameter).
        /// The command to execute.
        /// Flags that control how the process is created.
        ///
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool CreateProcessWithLogonW(string userName, string domain, string password, int logonFlags, string applicationPath, string commandLine,
        int creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInformation, out ProcessInformation processInformation);
                public static string run_cmd2(string cmd, string args)
        {
            string result = "";

            if (ImpersonateUser(GetBMSConfigurationKeyValue("impersonationuser"), GetBMSConfigurationKeyValue("impersonationdomain"), GetBMSConfigurationKeyValue("impersonationpassword")))
            {
                ProcessStartInfo start = new ProcessStartInfo();
                //Found in "where python"
                start.FileName = GetBMSConfigurationKeyValue("pypath");
                start.Arguments = string.Format("{0} {1}", cmd, args);
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
                UndoImpersonation();
                return result;
            }
            else
            {
                //Your impersonation failed. Therefore, include a fail-safe mechanism here.
                Log("Impersonation failed-most likely the password is bad.");
            }
            return result;

        }

        
        /// CloseHandle closes an open object handle.
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr handle);
        


        /// 
        /// The StartupInformation structure is used to specify the window station, desktop, standard handles
        /// and appearance of the main window for the new process. Further information is available on MSDN
        /// under STARTUPINFO (a href="http://msdn.microsoft.com/library/en-us/dllproc/base/startupinfo_str.asp)."http://msdn.microsoft.com/library/en-us/dllproc/base/startupinfo_str.asp)./a
        /// 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct StartupInformation
        {
            internal int cb;
            internal string reserved;
            internal string desktop;
            internal string title;
            internal int x;
            internal int y;
            internal int xSize;
            internal int ySize;
            internal int xCountChars;
            internal int yCountChars;
            internal int fillAttribute;
            internal int flags;
            internal UInt16 showWindow;
            internal UInt16 reserved2;
            internal byte reserved3;
            internal IntPtr stdInput;
            internal IntPtr stdOutput;
            internal IntPtr stdError;
        }

        /// 
        /// The ProcessInformation structure contains information about the newly created process and its
        /// primary thread.
        /// 
        /// hProcess is a handle to the newly created process.
        /// hThread is a handle to the primary thread of the newly created process.
        [StructLayout(LayoutKind.Sequential)]
        struct ProcessInformation
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal int processId;
            internal int threadId;
        }



        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr process, ref UInt32 exitCode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);


        // end of coding horror
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        public static WindowsImpersonationContext impersonationContext;
        private static object dicNickNames;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        public static bool ImpersonateUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        public static void UndoImpersonation()
        {
            impersonationContext.Undo();
        }

           

        public static void PaykuMiners()
        {
            try
            {

                string sql = "Select count(*) from Miner where Paid > getdate()-1 and userid is not null";
                DataTable dt2 = gData.GetDataTable(sql);
                if (dt2.Rows.Count < 1)
                    return;

                string batchID = Guid.NewGuid().ToString();

                sql = "Update Miner Set BatchID = '" + batchID + "' where userid is not null and updated > getdate()-1";
                gData.Exec(sql);
                sql = "Select sum(RAC) r1 from Miner where batchid = '" + batchID + "'";
                double dTotalRAC = gData.GetScalarDouble(sql, "r1");
                if (dTotalRAC < 1)
                {
                    Log("Pay  Miners less than 1 rac.");
                    return;
                }
                sql = "Select * from Miner where batchid = '" + batchID + "' order by rac";
                DataTable dt1 = gData.GetDataTable(sql);
                double nTot = 0;
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    double nMyRac = GetDouble(dt1.Rows[i]["rac"]);
                    double nPct = nMyRac / dTotalRAC;
                    double nAmt = 100000 * nPct;
                    if (nAmt > 1)
                    {
                        DataOps.AdjBalance(nAmt, dt1.Rows[i]["UserId"].ToString(), " Mining " + Math.Round(nPct * 100, 2) + "%, Sanctitude " + nMyRac.ToString());
                        nTot += nAmt;
                    }
                }
                sql = "Update Miner Set Paid=getdate() where 1=1";
                gData.Exec(sql);
                Log("Paid the  guys " + nTot.ToString() + "!");
            }
            catch (Exception ex)
            {
                Log("Error while paying  Miners " + ex.Message);
            }
        }
        */


    }

}
