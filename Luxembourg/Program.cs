using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace Luxembourg
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Lux.RunPrompt();
            /*if (args.Length > 1)
            {
                Console.WriteLine("Usage: lux [script]");
                Environment.Exit(1);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }*/
        }
    }
}