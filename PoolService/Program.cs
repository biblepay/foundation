using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Service Loop");
            Service.ServiceLoop();
            Console.WriteLine("Ending...");
        }
    }
}
