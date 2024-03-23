using BarRaider.SdTools;
using com.zaphop.nvidiabroadcast.Entities;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            //NvidiaBroadcastManager bm = new NvidiaBroadcastManager();
            //bm.ComboBoxTest();
           // return;

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

            //Windows.Win32.PInvoke.SetThreadUILanguage(1046); // This one did the trick!

            var x = Windows.Win32.PInvoke.GetThreadLocale();

            Console.WriteLine("Thread locale: " + x.ToString());

            var m = new NvidiaBroadcastManager();
            var toggleList = m.GetLocalizedStrings();
        }

        private static void ToggleAllControlsTest()
        {
            var m = new NvidiaBroadcastManager();

            //var toggleTypes = m.GetAvailableToggleTypes();

            m.Toggle("M:" + MicrophoneEffectType.RoomEchoRemoval, "(Default Device)", "", 1);
            //Thread.Sleep(2000);
            m.Toggle("M:" + MicrophoneEffectType.NoiseRemoval, "(Default Device)", "", 2);
            //Thread.Sleep(2000);
            m.Toggle("S:" + SpeakerEffectType.RoomEchoRemoval, "(Default Device)", "", 1);
            //Thread.Sleep(2000);
            m.Toggle("S:" + SpeakerEffectType.NoiseRemoval, "(Default Device)", "", 2);
            //Thread.Sleep(2000);
            m.Toggle("C:" + CameraEffectType.BackgroundRemoval, "Logitech Webcam C925e", "1280 x 720 @ 30FPS", 1);
            //Thread.Sleep(2000);
            m.Toggle("C:" + CameraEffectType.Vignette, "Logitech Webcam C925e", "1280 x 720 @ 30FPS", 2);
            //Thread.Sleep(2000);
        }
    }
}
