using ExcelParser;
using ExcelRosterReader.CommandLineParsing;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader
{
    class Program
    {
        static int Main(string[] args)
        {
            var returnVar = new App().Execute(args);
#if DEBUG
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
#endif
            return returnVar;
        }
    }
}
