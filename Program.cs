using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.zaphop.nvidiabroadcast
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            // This will cause the application to wait until you attach to the running process from Visual Studio.
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }
    }
}
