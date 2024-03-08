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
                ToggleType.CameraEffectSelected => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue", 0) > 0),
                ToggleType.CameraEffectSelected2 => ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue2", 0) > 0),
                _ => throw new NotSupportedException(toggle.ToString()),
            };
        }

        public bool IsToggleEnabled(string toggleString)
        {
            var toggleParts = toggleString.Split(':');

            ToggleType toggle = ToggleType.None;

            switch (toggleParts[0])
            {
                case "M":
                    if (Enum.TryParse<MicrophoneEffectType>(toggleParts[1], out var microphoneEffect) == true)
                        toggle = GetToggleTypeForEffect(microphoneEffect);
                    break;

                case "S":
                    if (Enum.TryParse<SpeakerEffectType>(toggleParts[1], out var speakerEffect) == true)
                        toggle = GetToggleTypeForEffect(speakerEffect);
                    break;

                case "C":
                    if (Enum.TryParse<CameraEffectType>(toggleParts[1], out var cameraEffect) == true)
                        toggle = GetToggleTypeForEffect(cameraEffect);
                    break;

                default:
                    throw new NotSupportedException(toggleString);
            }

            return IsToggleEnabled(toggle);
        }

        private ToggleType GetToggleTypeForEffect(MicrophoneEffectType effect)
        {
            var effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.MicEffect1.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.MicEffect1;

            effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.MicEffect2.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.MicEffect2;

            return ToggleType.None;
        }

        private ToggleType GetToggleTypeForEffect(SpeakerEffectType effect)
        {
            var effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.SpeakerEffect1.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.SpeakerEffect1;

            effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.SpeakerEffect2.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.SpeakerEffect2;

            return ToggleType.None;
        }

        private ToggleType GetToggleTypeForEffect(CameraEffectType effect)
        {
            var effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.CameraEffectSelected.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.CameraEffectSelected;

            effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", ToggleType.CameraEffectSelected2.ToString(), -1);
            if ((int)effect == effectValue)
                return ToggleType.CameraEffectSelected2;

            return ToggleType.None;
        }

        public void Toggle(string toggle)
        {
            var toggleParts = toggle.Split(':');

            switch (toggleParts[0])
            {
                case "M":
                    if (Enum.TryParse<MicrophoneEffectType>(toggleParts[1], out var microphoneEffect) == true)
                        Toggle(microphoneEffect);
                    break;

                case "S":
                    if (Enum.TryParse<SpeakerEffectType>(toggleParts[1], out var speakerEffect) == true)
                        Toggle(speakerEffect);
                    break;

                case "C":
                    if (Enum.TryParse<CameraEffectType>(toggleParts[1], out var cameraEffect) == true)
                        Toggle(cameraEffect);
                    break;

                default:
                    throw new NotSupportedException(toggle);
            }
        }

        private void Toggle(MicrophoneEffectType effect)
        {
            var toggle = GetToggleTypeForEffect(effect);
            if (toggle != ToggleType.None)
                Toggle(toggle);
        }

        private void Toggle(SpeakerEffectType effect)
        {
            var toggle = GetToggleTypeForEffect(effect);
            if (toggle != ToggleType.None)
                Toggle(toggle);
        }

        private void Toggle(CameraEffectType effect)
        {
            var toggle = GetToggleTypeForEffect(effect);
            if (toggle != ToggleType.None)
                Toggle(toggle);
        }

        private void Toggle(ToggleType toggle)
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

        public Dictionary<NvidiaBroadcastResourceID, string> GetLocalizedStrings()
        {
            var returnValue = new Dictionary<NvidiaBroadcastResourceID, string>();

            var exeLocation = GetNvidiaBroadcastExeLocation();

            foreach (var toggleResourceID in Enum.GetValues(typeof(NvidiaBroadcastResourceID)))
            {
                // This specifier is documented here: https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-shloadindirectstring
                var resourceFilename = $"@{exeLocation},-{(int)toggleResourceID}";

                var resourceString = new StringBuilder(1024);

                if (SHLoadIndirectString(resourceFilename, resourceString, 1024, IntPtr.Zero) != 0)
                    throw new KeyNotFoundException($"Couldn't look up resource id: {toggleResourceID}({(int)toggleResourceID}) from {resourceFilename}");

                returnValue.Add((NvidiaBroadcastResourceID) toggleResourceID, resourceString.ToString());
            }

            return returnValue;
        }

        private string GetNvidiaBroadcastExeLocation()
        {
            var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\NVIDIA Corporation\\Global\\NvBroadcast";

            // Get NVidia Broadcast UI installation directory.
            var exeLocation = (string)Registry.GetValue(registryPath, "RBXAppPath", null) ??
                throw new Exception("Couldn't find NVidia Broadcast install location from: {registryPath}\\RBXAppPath");

            return exeLocation;
        }

        private IntPtr GetRootWindowFromNvidiaBroadcast()
        {
            var rootWindowHwnd = PInvoke.User32.FindWindow("RTXVoiceWindowClass", null);

            if (rootWindowHwnd == IntPtr.Zero)
            {
                var exeLocation = GetNvidiaBroadcastExeLocation();

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

        //internal List<ToggleType> GetAvailableToggleTypes()
        //{
        //    var returnValue = new List<ToggleType>();
            
        //    if ((int) Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect1", 3) < 3)
        //        returnValue.Add(ToggleType.MicEffect1);

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect2", 3) < 3)
        //        returnValue.Add(ToggleType.MicEffect2);

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect1", 3) < 3)
        //        returnValue.Add(ToggleType.SpeakerEffect1);

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect2", 3) < 3)
        //        returnValue.Add(ToggleType.SpeakerEffect2);

        //    // Note: Naming standard and filter unset value changes for camera... this will probably also change for mic and speaker at some point.
        //    // This will probably break the above reg keys. 
        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected", -1) > -1)
        //        returnValue.Add(ToggleType.CameraEffectSelected);

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected2", -1) > 1)
        //        returnValue.Add(ToggleType.CameraEffectSelected2);

        //    return returnValue;
        //}
    }
}
