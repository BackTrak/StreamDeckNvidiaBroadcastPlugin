using Microsoft.Win32;
using Newtonsoft.Json;
using PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly string _toggleName;
        private readonly string _configName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toggleResourceID">The Resource ID of the selection string from the combobox. This can be found by using Resource Hacker to find the resource text string. Locale is set by the current application globalization settings.</param>
        /// <param name="configName">The string from the config file that indicates when the option is enabled</param>
        public NvidiaBroadcastManager(NvidiaBroadcastResourceID toggleResourceID, string configName)
        {
            _toggleName = GetToggleNameFromResourceID(toggleResourceID);   
            _configName = configName;   
        }

        private string GetToggleNameFromResourceID(NvidiaBroadcastResourceID toggleResourceID)
        {
            var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\NVIDIA Corporation\\Global\\NvBroadcast";

            // Get NVidia Broadcast UI installation directory.
            var exeLocation = (string) Registry.GetValue(registryPath, "RBXAppPath", null) ??
                throw new Exception("Couldn't find NVidia Broadcast install location from: {registryPath}\\RBXAppPath");

            // This specifier is documented here: https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-shloadindirectstring
            var resourceFilename = $"@{exeLocation},-{(int) toggleResourceID}";

            var resourceString = new StringBuilder(1024);

            

            if (SHLoadIndirectString(resourceFilename, resourceString, 1024, IntPtr.Zero) != 0)
                throw new KeyNotFoundException($"Couldn't look up resource id: {toggleResourceID}({(int) toggleResourceID}) from {resourceFilename}");

            return resourceString.ToString();
        }

        public bool IsToggleEnabled()
        {
            return IsSettingEnabled(_configName);
        }

        private bool IsSettingEnabled(string settingName)
        {
            string configPath = System.Environment.ExpandEnvironmentVariables(@"%ProgramData%\\NVIDIA\\NVIDIABroadcast\\Settings\\NvVirtualCamera\\config.json");

            var nvidiaConfig = JsonConvert.DeserializeObject<NvidiaBroadcastConfig>(File.ReadAllText(configPath));

            return nvidiaConfig.RTXCamera.Streams.Exists(p => p.Effect.Exists(r => r.Name.Equals(settingName, StringComparison.InvariantCultureIgnoreCase)));
        }

        public void Toggle()
        {
            bool currentState = IsToggleEnabled();

            for (int i = 0; i < 5 && currentState == IsToggleEnabled(); i++)
            {
                ToggleSetting(_toggleName);
                Thread.Sleep(100);
            }
        }

        private bool ToggleSetting(string name)
        {
            var rootWindowHwnd = PInvoke.User32.FindWindow("RTXVoiceWindowClass", null);

            EnumedWindow callBackPtr = GetWindowHandle;
            var windowHandles = new List<IntPtr>();

            GCHandle listHandle = GCHandle.Alloc(windowHandles);

            PInvoke.User32.EnumChildWindows(rootWindowHwnd, Marshal.GetFunctionPointerForDelegate(callBackPtr), GCHandle.ToIntPtr(listHandle));

            WINDOWPLACEMENT comboBoxLocation = default;

            // Find target combobox
            foreach (var handle in windowHandles)
            {
                User32.WINDOWINFO pwi = default;
                User32.GetWindowInfo(handle, ref pwi);

                //Console.WriteLine($"className: {User32.GetClassName(handle)}");

                if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    var windowText = new StringBuilder(255);
                    SendMessage(handle, (uint)User32.WindowMessage.WM_GETTEXT, 255, windowText);

                    if (windowText.ToString().IndexOf(name, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        GetWindowPlacement(handle, ref comboBoxLocation);
                        break;
                    }
                }
            }

            // No combobox found.
            if (comboBoxLocation.length == 0)
                return false;

            IntPtr targetButtonHandle = IntPtr.Zero;

            // Find the button to the right of the ComboBox.
            foreach (var handle in windowHandles)
            {
                if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    WINDOWPLACEMENT buttonPlacement = default;
                    GetWindowPlacement(handle, ref buttonPlacement);

                    if (buttonPlacement.rcNormalPosition.top >= comboBoxLocation.rcNormalPosition.top
                        && buttonPlacement.rcNormalPosition.bottom <= comboBoxLocation.rcNormalPosition.bottom
                        && buttonPlacement.rcNormalPosition.left > comboBoxLocation.rcNormalPosition.right)
                    {
                        targetButtonHandle = handle;
                        break;
                    }
                }
            }

            //Console.WriteLine($"targetButtonHandle: {targetButtonHandle.ToString("X")}");

            User32.SendMessage(targetButtonHandle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);

            return true;
        }

        private bool GetWindowHandle(IntPtr windowHandle, IntPtr arrayListPtr)
        {
            GCHandle gch = GCHandle.FromIntPtr(arrayListPtr);

            // Not sure how this even compiles... where does windowHandles come from??
            if (gch.Target is not List<IntPtr> windowHandles)
                throw new ArgumentNullException(nameof(windowHandles));

            //var windowHandles = gch.Target as List<IntPtr>;

            //if (windowHandles == null)
            //    throw new ArgumentNullException(nameof(windowHandles));

            windowHandles.Add(windowHandle);
            return true;
        }
    }
}
