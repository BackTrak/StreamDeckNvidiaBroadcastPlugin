using Microsoft.Win32;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static PInvoke.User32;

namespace com.zaphop.nvidiabroadcast
{
    internal class NvidiaBroadcastManager
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, StringBuilder lParam);

        private delegate bool EnumedWindow(IntPtr handleWindow, IntPtr arrayListPtr);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        public bool IsToggleEnabled(ToggleType toggle)
        {
            return toggle switch
            {
                ToggleType.None => false,
                ToggleType.MicEffect1 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicDenoising", 0) > 0),
                ToggleType.MicEffect2 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicDenoising2", 0) > 0),
                ToggleType.SpeakerEffect1 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerDenoising", 0) > 0),
                ToggleType.SpeakerEffect2 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerDenoising2", 0) > 0),
                ToggleType.CameraEffect1 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue", 0) > 0),
                ToggleType.CameraEffect2 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue2", 0) > 0),
                _ => throw new NotSupportedException(toggle.ToString()),
            };
        }

        public void Toggle(ToggleType toggle)
        {
            bool currentState = IsToggleEnabled(toggle);

            for (int i = 0; i < 5 && currentState == IsToggleEnabled(toggle); i++)
            {
                ToggleSetting(toggle);
                Thread.Sleep(100);
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        static extern IntPtr GetWindowCaption(IntPtr hwnd, StringBuilder lpString, int maxCount);

        [DllImport("user32.dll", EntryPoint = "GetDlgCtrlID", CharSet = CharSet.Auto)]
        static extern long GetDlgCtrlID(IntPtr hwnd);

        private bool GetWindowHandle(IntPtr windowHandle, IntPtr arrayListPtr)
        {
            GCHandle gch = GCHandle.FromIntPtr(arrayListPtr);

            if (gch.Target is not List<IntPtr> windowHandles)
                throw new ArgumentNullException(nameof(windowHandles));

            windowHandles.Add(windowHandle);
            return true;
        }

        private bool ToggleSetting(ToggleType toggle)
        {
            var rootWindowHwnd = GetRootWindowFromNvidiaBroadcast();

            EnumedWindow callBackPtr = GetWindowHandle;
            var windowHandles = new List<IntPtr>();

            GCHandle listHandle = GCHandle.Alloc(windowHandles);

            PInvoke.User32.EnumChildWindows(rootWindowHwnd, Marshal.GetFunctionPointerForDelegate(callBackPtr), GCHandle.ToIntPtr(listHandle));

            foreach (var handle in windowHandles)
            {
                if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID == (int) toggle)
                        User32.SendMessage(handle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                }   
            }

            return false;
        }

        private IntPtr GetRootWindowFromNvidiaBroadcast()
        {
            var rootWindowHwnd = PInvoke.User32.FindWindow("RTXVoiceWindowClass", null);

            if (rootWindowHwnd == IntPtr.Zero)
            {
                var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\NVIDIA Corporation\\Global\\NvBroadcast";

                // Get NVidia Broadcast UI installation directory.
                var exeLocation = (string)Registry.GetValue(registryPath, "RBXAppPath", null) ??
                    throw new Exception("Couldn't find NVidia Broadcast install location from: {registryPath}\\RBXAppPath");

                var runningProcess = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "NVIDIA Broadcast UI");

                if(runningProcess == null)
                    Process.Start(exeLocation);

                for (int i = 0; i < 300; i++)
                {
                    if ((rootWindowHwnd = PInvoke.User32.FindWindow("RTXVoiceWindowClass", null)) != IntPtr.Zero)
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                }
            }

            return rootWindowHwnd;
        }

        internal List<ToggleType> GetAvailableToggleTypes()
        {
            var returnValue = new List<ToggleType>();
            
            if ((int) Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect1", 3) < 3)
                returnValue.Add(ToggleType.MicEffect1);

            if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect2", 3) < 3)
                returnValue.Add(ToggleType.MicEffect2);

            if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect1", 3) < 3)
                returnValue.Add(ToggleType.SpeakerEffect1);

            if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect2", 3) < 3)
                returnValue.Add(ToggleType.SpeakerEffect2);

            // Note: Naming standard and filter unset value changes for camera... this will probably also change for mic and speaker at some point.
            // This will probably break the above reg keys. 
            if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected", -1) > -1)
                returnValue.Add(ToggleType.CameraEffect1);

            if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected2", -1) > 1)
                returnValue.Add(ToggleType.CameraEffect2);

            return returnValue;
        }
    }
}
