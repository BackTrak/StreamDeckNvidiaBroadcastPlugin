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
            var bm = new NvidiaBroadcastManager(NvidiaBroadcastResourceID.BackgroundRemoval, "BackgroundRemoval");

            TestResourceLoading();

            // Uncomment this line of code to allow for debugging
            // This will cause the application to wait until you attach to the running process from Visual Studio.
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        private static void TestResourceLoading()
        {
            unsafe
            {
                string file = @"C:\Program Files\NVIDIA Corporation\NVIDIA Broadcast\NVIDIA Broadcast UI.exe";
                //string file = @"C:\Program Files\NVIDIA Corporation\NVIDIA Broadcast\1.exe";

                if (File.Exists(file) == false)
                    throw new FileNotFoundException();

                StringBuilder output = new StringBuilder(1024);

                //Directory.SetCurrentDirectory(Path.GetDirectoryName(file));
                
                var resourceFilename = $"@{file},-32832";
                //var resourceFilename = $"@{Path.GetFileName(file)},-2053";
                //var resourceFilename = $"@vmms.exe,-279";

                var result = SHLoadIndirectString(resourceFilename, output, 1024, IntPtr.Zero);
                //var lastError = PInvoke.Kernel32.GetLastError();

                //var currentDir = Directory.GetCurrentDirectory();

                var library = PInvoke.Kernel32.LoadLibrary(file);

                var resource = PInvoke.Kernel32.FindResource(library, PInvoke.Kernel32.MAKEINTRESOURCE(2053), PInvoke.Kernel32.RT_STRING);

                char[] lpBuffer = new char[1024];

                var loadString = PInvoke.User32.LoadString(resource, 32832, lpBuffer, 1024);

                // https://forums.codeguru.com/showthread.php?226637-FindResource-with-String-Table

                var data = PInvoke.Kernel32.LoadResource(library, resource);

                var size = PInvoke.Kernel32.SizeofResource(library, resource);

                byte [] dataBytes = new byte[1024];
                Marshal.Copy(data, dataBytes, 0, size);

                var dataString = System.Text.ASCIIEncoding.UTF8.GetString(dataBytes);

                //char[] lpBuffer = new char[256];

                //var resource = PInvoke.User32.LoadString(library.DangerousGetHandle(), PInvoke.Kernel32.Mak(2053), lpBuffer, lpBuffer.Length);

                //Console.WriteLine(resource + " " + lpBuffer.Length);
                //var lastError = PInvoke.Kernel32.GetLastError();

                //var resource = PInvoke.Kernel32.LoadResource(library, IntPtr.Zero);
                //var data = PInvoke.Kernel32.LockResource((IntPtr)resource.ToPointer());
            }

        }
    }
}
