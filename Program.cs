using BarRaider.SdTools;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members

namespace com.zaphop.nvidiabroadcast
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ToggleAllControlsTest();
            //TestBroadcastManager();

            // Uncomment this line of code to allow for debugging
            // This will cause the application to wait until you attach to the running process from Visual Studio.
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }

        private static void TestBroadcastManager()
        {
            var ci = new System.Globalization.CultureInfo("pt-BR");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            Windows.Win32.PInvoke.SetThreadLocale(1046);
            Windows.Win32.PInvoke.SetThreadUILanguage(1046); // This one did the trick!

            var x = Windows.Win32.PInvoke.GetThreadLocale();

            Console.WriteLine(x.ToString());
        }

        private static void ToggleAllControlsTest()
        {
            var m = new NvidiaBroadcastManager();
            m.GetAvailableToggleTypes();
            m.Toggle(ToggleType.MicEffect1);
            m.Toggle(ToggleType.MicEffect2);
            m.Toggle(ToggleType.SpeakerEffect1);
            m.Toggle(ToggleType.SpeakerEffect2);
            m.Toggle(ToggleType.CameraEffect1);
            m.Toggle(ToggleType.CameraEffect2);
        }
    }
}
