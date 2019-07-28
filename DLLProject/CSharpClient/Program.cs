using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpClient
{
    class Program
    {


        public class BindingDllClass
        {
            [DllImport(@"C:\Users\beao3002\Documents\Visual Studio 2015\Projects\DLLProject\Debug\DLLProject.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern double Add(double a, double b);

            [DllImport(@"C:\Users\beao3002\Documents\Visual Studio 2015\Projects\DLLProject\Debug\DLLProject.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int OCL();
        }
        static void Main(string[] args)
        {
            BindingDllClass.OCL();
            Console.ReadLine();
        }

    }
}
