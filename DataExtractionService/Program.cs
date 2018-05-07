using DataExtractionService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtractionService
{
    static class Program
    {
        private static ITreasuaryRateExtraction treasuary;
       
        static void Main(string[] args)
        {
            treasuary.MigrateExchangeRate();
            Console.WriteLine("helo world");
            Console.ReadKey();
        }
    }
}
