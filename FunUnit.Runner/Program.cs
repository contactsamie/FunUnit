using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunUnit.Runner
{
    internal class Program
    {
        private static readonly Mutex Process = new Mutex(false, System.Diagnostics.Process.GetCurrentProcess().ProcessName);
        private static void Main(string[] args) 
        {
            new DynaInvoke(new ConsoleFunUnitNotification()).Watch();
            Console.ReadLine();
            Process.ReleaseMutex();
        }
    }
}
