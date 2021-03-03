using System;
using System.IO;
using System.Web.SessionState;
using System.Web.UI;
using static Saved.Code.PoolCommon;

namespace Saved.Code
{

    public static class FileSplitter
    {

        public static bool RelinquishSpace(string sPath)
        {
            try
            {
                string sDir = Path.Combine(Common.GetFolderUnchained("Temp"), "SVR" + sPath.GetHashCode().ToString());
                Directory.Delete(sDir, true);
                return true;
            }
            catch (Exception ex)
            {
                Common.Log("Unable to relinquish space in " + sPath);
            }
            return false;
        }

        public static string SplitFile(string sPath)
        {
            int iPart = 0;
            string sDir = Path.Combine(Common.GetFolderUnchained("Temp"), "SVR" + sPath.GetHashCode().ToString());
            using (Stream source = File.OpenRead(sPath))
            {
                byte[] buffer = new byte[10000000];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string sPartPath = Path.Combine(sDir, iPart.ToString() + ".dat");
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
        private static int MAX_PARTS = 7000;
        public static void ResurrectFile(string sFolder, string sFinalFileName)
        {
            DirectoryInfo di = new DirectoryInfo(sFolder);
            string sMasterOut = Path.Combine(sFolder, sFinalFileName);
            Stream dest = new FileStream(sMasterOut, FileMode.Create);
            for (int i = 0; i < MAX_PARTS; i++)
            {
                string sFN = i.ToString() + ".dat";
                string sPath = Path.Combine(di.FullName, sFN);
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

