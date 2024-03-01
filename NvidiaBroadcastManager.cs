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

        //private readonly string _toggleName;
        //private readonly string _configName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toggleResourceID">The Resource ID of the selection string from the combobox. This can be found by using Resource Hacker to find the resource text string. Locale is set by the current application globalization settings.</param>
        /// <param name="configName">The string from the config file that indicates when the option is enabled</param>
        //public NvidiaBroadcastManager()
        //{
            
        //}

        //private string GetToggleNameFromResourceID(NvidiaBroadcastResourceID toggleResourceID)
        //{
        //    var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\NVIDIA Corporation\\Global\\NvBroadcast";

        //    // Get NVidia Broadcast UI installation directory.
        //    var exeLocation = (string) Registry.GetValue(registryPath, "RBXAppPath", null) ??
        //        throw new Exception("Couldn't find NVidia Broadcast install location from: {registryPath}\\RBXAppPath");

        //    // This specifier is documented here: https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-shloadindirectstring
        //    var resourceFilename = $"@{exeLocation},-{(int) toggleResourceID}";

        //    var resourceString = new StringBuilder(1024);

        //    if (SHLoadIndirectString(resourceFilename, resourceString, 1024, IntPtr.Zero) != 0)
        //        throw new KeyNotFoundException($"Couldn't look up resource id: {toggleResourceID}({(int) toggleResourceID}) from {resourceFilename}");

        //    return resourceString.ToString();
        //}

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

        //private bool IsSettingEnabled(string settingName)
        //{
        //    string configPath = System.Environment.ExpandEnvironmentVariables(@"%ProgramData%\\NVIDIA\\NVIDIABroadcast\\Settings\\NvVirtualCamera\\config.json");

        //    var nvidiaConfig = JsonConvert.DeserializeObject<NvidiaBroadcastConfig>(File.ReadAllText(configPath));

        //    return nvidiaConfig.RTXCamera.Streams.Exists(p => p.Effect.Exists(r => r.Name.Equals(settingName, StringComparison.InvariantCultureIgnoreCase)));
        //}

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

        

        private bool ToggleSetting(ToggleType toggle)
        {
            var rootWindowHwnd = GetRootWindowFromNvidiaBroadcast();

            //string toggleControlAutomationName = String.Empty;
            //string toggleGroupName = String.Empty;

            //switch (toggle)
            //{
            //    case ToggleType.MicEffect1:
            //        toggleControlAutomationName = "Toggle first mic effect";
            //        toggleGroupName = "MICROPHONE";
            //        break;
            //    case ToggleType.MicEffect2:
            //        toggleControlAutomationName = "Toggle second mic effect";
            //        toggleGroupName = "MICROPHONE";
            //        break;
            //    case ToggleType.SpeakerEffect1:
            //        toggleControlAutomationName = "Toggle first speaker effect";
            //        toggleGroupName = "SPEAKERS";
            //        break;
            //    case ToggleType.SpeakerEffect2:
            //        toggleControlAutomationName = "Toggle second speaker effect";
            //        toggleGroupName = "SPEAKERS";
            //        break;
            //    case ToggleType.CameraEffect1:
            //        toggleControlAutomationName = "Toggle first camera effect";
            //        toggleGroupName = "CAMERA";
            //        break;
            //    case ToggleType.CameraEffect2:
            //        toggleControlAutomationName = "Toggle second camera effect";
            //        toggleGroupName = "CAMERA";
            //        break;
            //    default:
            //        throw new NotSupportedException(toggle.ToString());
            //}



            //using (var automation = new UIA3Automation())
            //{
            //    var mainWindow = automation.FromHandle(rootWindowHwnd);

            //    ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

            //    var groupButton = mainWindow.FindFirstDescendant(cf.ByName(toggleGroupName)).AsButton();
            //    groupButton.Invoke();

            //    var toggleButton = mainWindow.FindFirstDescendant(cf.ByName(toggleControlAutomationName)).AsButton();
            //    toggleButton.Invoke();
            //}

            //return true;


            //FlaUI.Core.WindowsAPI.

            //var ae = System.Windows.Automation.AutomationElement.FromHandle(rootWindowHwnd);

            //Condition condition = new System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.NameProperty, "Toggle first mic effect");

            //var c = ae.FindAll(System.Windows.Automation.TreeScope.Descendants, condition);

            //((System.Windows.Forms.Button)c).Invoke();

            EnumedWindow callBackPtr = GetWindowHandle;
            var windowHandles = new List<IntPtr>();

            GCHandle listHandle = GCHandle.Alloc(windowHandles);

            PInvoke.User32.EnumChildWindows(rootWindowHwnd, Marshal.GetFunctionPointerForDelegate(callBackPtr), GCHandle.ToIntPtr(listHandle));

            //var x = User32.FindWindowEx(rootWindowHwnd, IntPtr.Zero, null, "Toggle first mic effect"); 

            foreach (var handle in windowHandles)
            {
                if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    //var windowText = new StringBuilder(255);
                    //SendMessage(handle, (uint)User32.WindowMessage.WM_GETTEXT, 255, windowText);

                    //GetWindowCaption(handle, windowText, 255);
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID == (int) toggle)
                        User32.SendMessage(handle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                    
                    //Span<char> text = new Span<char>();
                    //User32.GetWindowCaption(handle, text);

                    //Console.WriteLine(windowText);

                    //Console.WriteLine(controlID);
                }   
            }

            return false;


            //WINDOWPLACEMENT comboBoxLocation = default;

            //// Find target combobox
            //foreach (var handle in windowHandles)
            //{
            //    User32.WINDOWINFO pwi = default;
            //    User32.GetWindowInfo(handle, ref pwi);

            //    //Console.WriteLine($"className: {User32.GetClassName(handle)}");

            //    if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
            //    {
            //        var windowText = new StringBuilder(255);
            //        SendMessage(handle, (uint)User32.WindowMessage.WM_GETTEXT, 255, windowText);

            //        if (windowText.ToString().IndexOf("name", StringComparison.InvariantCultureIgnoreCase) >= 0)
            //        {
            //            GetWindowPlacement(handle, ref comboBoxLocation);
            //            break;
            //        }
            //    }
            //}

            //// No combobox found.
            //if (comboBoxLocation.length == 0)
            //    return false;

            //IntPtr targetButtonHandle = IntPtr.Zero;

            //// Find the button to the right of the ComboBox.
            //foreach (var handle in windowHandles)
            //{
            //    if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
            //    {
            //        WINDOWPLACEMENT buttonPlacement = default;
            //        GetWindowPlacement(handle, ref buttonPlacement);

            //        if (buttonPlacement.rcNormalPosition.top >= comboBoxLocation.rcNormalPosition.top
            //            && buttonPlacement.rcNormalPosition.bottom <= comboBoxLocation.rcNormalPosition.bottom
            //            && buttonPlacement.rcNormalPosition.left > comboBoxLocation.rcNormalPosition.right)
            //        {
            //            targetButtonHandle = handle;
            //            break;
            //        }
            //    }
            //}

            ////Console.WriteLine($"targetButtonHandle: {targetButtonHandle.ToString("X")}");

            //User32.SendMessage(targetButtonHandle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);

            //return true;
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
