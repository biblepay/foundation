using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoolService
{
    class Program
    {

        static void Main(string[] args)
        {
            Service.ServiceLoop();
            Console.WriteLine("Ending...");
        }
    }
}
