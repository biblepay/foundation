using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
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
            SetWorker(w, socketid);
        }

        public static string mBatch = "";
        public static int nBatchCount = 0;
        public static void BatchExec(string sql)
        {
            mBatch += sql + "\r\n";
            if (nBatchCount > 20)
            {
                nBatchCount = 0;
                gData.Exec(mBatch, false, true);
                mBatch = "";
            }
            nBatchCount++;
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

                if (Debugger.IsAttached)
                {
                    // We can put unit test code in here etc:
                }

                Thread.Sleep(60000);
                if (!Debugger.IsAttached)
                {
                    GroupShares();
                    Leaderboard();
                    Pay();
                    PurgeSockets(false);
                    PurgeJobs();
                }
            }
        }


        public Pool()
        {
            //var t = new Thread(InitializePool);
            //t.Start();
            var t1 = new Thread(PoolService);
            t1.Start();
            var t2 = new Thread(SQLExecutor);
            t2.Start();
        }
    }
}
