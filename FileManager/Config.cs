using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    public class Config
    {
        public int PageSize;
        public string StartFolder1, StartFolder2, cmdLineFolder;
        public void Default()
        {
            StartFolder1 = "";
            StartFolder2 = @"C:\";
            cmdLineFolder = Directory.GetCurrentDirectory();
            PageSize = 25;
        }
        public void Input()
        {
            Console.Clear();
            Console.WriteLine("Setup: ");
            Console.Write("Please enter start folder for Left Window :> "); StartFolder1 = Console.ReadLine();
            Console.Write("Please enter start folder for Right Window :> "); StartFolder2 = Console.ReadLine();
            do
            {
                Console.Write("Please enter page size (max number of lines per page) :> "); var str = Console.ReadLine();
                try
                {
                    PageSize = Int32.Parse(str);
                    if (PageSize <= 0) Console.WriteLine("Number must be positive.");
                }
                catch { Console.WriteLine("Please enter the number..."); PageSize = 0; Window.logError("Entering number failure."); }
            } while (PageSize <= 0);
        }
    }
}
