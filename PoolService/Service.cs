using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolService
{
    class Service
    {
        // The Service loop performs functions that are not possible to run from the ASP.NET (IIS) Application
        // This includes:  Converting youtube videos to mp4
        // Not only are these conversions long running, but more specifically need to be run as Administrator
        // The IIS App Pool User does not have enough privilege to run external tools with arguments, such as youtube-dl.exe, etc.
        public static void ServiceLoop()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(30000);
                Console.WriteLine("Working on videos");
                Saved.Code.Common.ConvertVideos();
                Console.WriteLine("Done with videos");
            }
        }
    }
}
