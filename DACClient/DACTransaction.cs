using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DACClient
{
    class DACTransaction
    {
        public struct DACResult
        {
            public string Result;
            public string Error;
            public double Fee;
            public bool OverallResult;
            public string URL;
        }

        public static DACResult UploadFile(string sLocalPath, string sAPI_KEY)
        {
            DACResult r = new DACResult();
            // Returns the URL of the resource
            string sDir = Splitter.SplitFile(sLocalPath);
            FileInfo fi = new FileInfo(sLocalPath);
            string sOriginalName = fi.Name;
            DirectoryInfo di = new DirectoryInfo(sDir);
            string sURL = "https://localhost:44358/UnchainedUpload";
            int nTotalParts = 0;
            for (int i = 0; i < Splitter.MAX_PARTS; i++)
            {
                string sPartial = i.ToString() + ".dat";
                string sPath = Path.Combine(di.FullName, sPartial);
                if (File.Exists(sPath))
                {
                    nTotalParts = i;
                }
                else
                {
                    break; 
                }
            }
            for (int i = 0; i <= nTotalParts; i++)
            {
                string sPartial = i.ToString() + ".dat";
                string sPath = Path.Combine(di.FullName, sPartial);
                if (File.Exists(sPath))
                {
                    string sResult = IO.SubmitPart(sAPI_KEY, sURL, sOriginalName, sPath, i, nTotalParts);
                    string sStatus = IO.ExtractXML(sResult, "<status>", "</status>");
                    string out_URL = IO.ExtractXML(sResult, "<url>", "</url>");
                    double nStatus = IO.GetDouble(sStatus);

                    if (nStatus != 1)
                    {
                        r.OverallResult = false;
                        break;
                    }
                    if (i == nTotalParts)
                    {
                        r.OverallResult = true;
                        r.URL = out_URL;

                    }
                }
            }
            // Relinquish Space
            bool fEliminated = Splitter.RelinquishSpace(sLocalPath);
            return r;
        }
    }
}
