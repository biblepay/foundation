using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DACClient
{
    

    public class BBPWebClient : System.Net.WebClient
    {
        private int DEFAULT_TIMEOUT = 30000;
        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            System.Net.WebRequest w = base.GetWebRequest(uri);
            w.Timeout = DEFAULT_TIMEOUT;
            return w;
        }
        public void SetTimeout(int nTimeOut)
        {
            DEFAULT_TIMEOUT = nTimeOut;
        }
    }

    class IO
    {
       
        private static string GetFolderLog(string sFileName)
        {
            string sPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            string sFullPath = Path.Combine(sPath, sFileName);
            return sFullPath;
        }

        public static void Log(string sData, bool fQuiet = false)
        {
                    try
                    {
                            string sPath = GetFolderLog("bbp_ipfs_client.log");
                            System.IO.StreamWriter sw = new System.IO.StreamWriter(sPath, true);
                            string Timestamp = DateTime.Now.ToString();
                            sw.WriteLine(Timestamp + ": " + sData);
                            sw.Close();
                    }

                    catch (Exception ex)
                    {
                        string sMsg = ex.Message;
                    }
                
         
        }
        public static string ExtractXML(string sData, string sStartKey, string sEndKey)
        {
            if (sData == null)
                return string.Empty;

            int iPos1 = (sData.IndexOf(sStartKey, 0) + 1);
            if (iPos1 < 1)
                return string.Empty;

            iPos1 = (iPos1 + sStartKey.Length);
            int iPos2 = (sData.IndexOf(sEndKey, (iPos1 - 1)) + 1);
            if ((iPos2 == 0))
            {
                return String.Empty;
            }
            string sOut = sData.Substring((iPos1 - 1), (iPos2 - iPos1));
            return sOut;
        }

        public static double GetDouble(object o)
        {
            try
            {
                if (o == null) return 0;
                if (o.ToString() == string.Empty) return 0;
                double d = Convert.ToDouble(o.ToString());
                return d;
            }
            catch (Exception ex)
            {
                // Letters
                return 0;
            }
        }

        public static string SubmitPart(string sAPI_KEY, string sURL, string sOriginalName, string sFileName, int iPartNumber, int iTotalParts)
        {
            try
            {
                BBPWebClient w = new BBPWebClient();
                w.Headers.Add("APIKey", sAPI_KEY);
                w.Headers.Add("Part", sFileName);
                w.Headers.Add("PartNumber", iPartNumber.ToString());
                w.Headers.Add("OriginalName", sOriginalName);
                w.Headers.Add("TotalParts", iTotalParts.ToString());
                if (iTotalParts == iPartNumber)
                    w.SetTimeout(240000);

                byte[] b = System.IO.File.ReadAllBytes(sFileName);
                byte[] e = w.UploadData(sURL, b);
                string sData = Encoding.UTF8.GetString(e, 0, e.Length);
                return sData;
            }
            catch(Exception ex)
            {
                Log("Unable to submit part #" + iPartNumber.ToString() + "; " + ex.Message);
                return "<status>0</status>";
            }
            
        }
    }



    public static class Splitter
    {
        public static int MAX_PARTS = 7000;

        public static bool RelinquishSpace(string sPath)
        {
            FileInfo fi = new FileInfo(sPath);
            string sDir = Path.Combine(Path.GetTempPath(), sPath.GetHashCode().ToString());
            Directory.Delete(sDir, true);
            return true;

        }

        public static string SplitFile(string sPath)
        {
            FileInfo fi = new FileInfo(sPath);

            int iPart = 0;
            string sDir = Path.Combine(Path.GetTempPath(), sPath.GetHashCode().ToString());
            
            using (Stream source = File.OpenRead(sPath))
            {
                byte[] buffer = new byte[10000000];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {

                    string sPartPath = sDir + "\\" + iPart.ToString() + ".dat";

                    if (!System.IO.Directory.Exists(sDir))
                        System.IO.Directory.CreateDirectory(sDir);

                    Stream dest = new FileStream(sPartPath, FileMode.Create);

                    dest.Write(buffer, 0, bytesRead);
                    dest.Close();
                    iPart++;
                }
            }
            return sDir;

        }

        public static void ResurrectFile(string sFolder, string sFinalFileName)
        {
            DirectoryInfo di = new DirectoryInfo(sFolder);
            string sMasterOut = Path.Combine(sFolder, sFinalFileName);
            Stream dest = new FileStream(sMasterOut, FileMode.Create);

            for (int i = 0; i < MAX_PARTS; i++)
            {
                string sPath = di.FullName + "\\" + i.ToString() + ".dat";
                if (File.Exists(sPath))
                {
                    byte[] b = System.IO.File.ReadAllBytes(sPath);
                    dest.Write(b, 0, b.Length);

                }
                else
                {
                    break;
                }
            }
            dest.Close();

        }

    }

}
