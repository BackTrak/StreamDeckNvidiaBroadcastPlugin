using BarRaider.SdTools;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.zaphop.nvidiabroadcast
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TestBroadcastManager();

            // Uncomment this line of code to allow for debugging
            // This will cause the application to wait until you attach to the running process from Visual Studio.
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }

        private static void TestBroadcastManager()
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("pt-BR");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            Windows.Win32.PInvoke.SetThreadLocale(1046);
            Windows.Win32.PInvoke.SetThreadUILanguage(1046); // This one did the trick!

            var x = Windows.Win32.PInvoke.GetThreadLocale();

            NvidiaBroadcastManager n = new NvidiaBroadcastManager(NvidiaBroadcastResourceID.AutoFrame, "FaceZoom");
        }
    }
}
