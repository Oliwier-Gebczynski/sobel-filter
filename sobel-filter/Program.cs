using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace sobel_filter
{
    internal class Program
    {
        [DllImport(@"C:\Users\oliwi\Documents\GitHub\sobel-filter\sobel-filter\x64\Debug\sobel-dll.dll")]
        static extern int MyProc1(int a, int b);
        static void Main(string[] args)
        {
            int x = 5, y = 3;
            int retVal = MyProc1(x, y);
            Console.WriteLine(retVal);
            Console.ReadLine();
        }
    }
}
