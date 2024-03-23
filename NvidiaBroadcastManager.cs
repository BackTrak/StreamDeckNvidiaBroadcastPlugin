using com.zaphop.nvidiabroadcast.Entities;
using Microsoft.Win32;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Win32.Foundation;
using static PInvoke.User32;

namespace com.zaphop.nvidiabroadcast
{
    internal class NvidiaBroadcastManager
    {
        private readonly Dictionary<NvidiaBroadcastResourceID, string> _localizedStrings;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, StringBuilder lParam);

        private delegate bool EnumedWindow(IntPtr handleWindow, IntPtr arrayListPtr);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        public NvidiaBroadcastManager()
        {
            _localizedStrings = GetLocalizedStrings();
        }

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

        public bool IsToggleConfigured(string toggleString)
        {
            var toggle = ToggleType.None;
            var toggleParts = toggleString.Split(':');

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

            return toggle != ToggleType.None;
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

        //internal List<ToggleType> GetAvailableToggleTypes()
        //{
        //    var returnValue = new List<ToggleType>();

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect1", 3) < 3)
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

        private int NumberOfMicrophoneTogglesSet()
        {
            int toggleCount = 0;

            if ((MicrophoneEffectType)(int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect1", (int)MicrophoneEffectType.None) != MicrophoneEffectType.None)
                toggleCount++;

            if ((MicrophoneEffectType)(int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect2", (int)MicrophoneEffectType.None) != MicrophoneEffectType.None)
                toggleCount++;

            return toggleCount;
        }

        private int NumberOfSpeakerTogglesSet()
        {
            int toggleCount = 0;

            if ((SpeakerEffectType)(int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect1", (int)SpeakerEffectType.None) != SpeakerEffectType.None)
                toggleCount++;

            if ((SpeakerEffectType)(int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerEffect2", (int)SpeakerEffectType.None) != SpeakerEffectType.None)
                toggleCount++;

            return toggleCount;
        }

        private int NumberOfCameraTogglesSet()
        {
            int toggleCount = 0;

            var cameraEffectSelected1 = (int) Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected", -1);

            if (cameraEffectSelected1 != -1)
                toggleCount++;

            var cameraEffectSelected2 = (int) Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "CameraEffectSelected2", -1);

            if (cameraEffectSelected2 != -1)
                toggleCount++;

            return toggleCount;
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

        public void Toggle(string toggle, string device, string resolution, int slot)
        {
            var toggleParts = toggle.Split(':');

            switch (toggleParts[0])
            {
                case "M":
                    if (Enum.TryParse<MicrophoneEffectType>(toggleParts[1], out var microphoneEffect) == true)
                        Toggle(microphoneEffect, device, resolution, slot);
                    break;

                case "S":
                    if (Enum.TryParse<SpeakerEffectType>(toggleParts[1], out var speakerEffect) == true)
                        Toggle(speakerEffect, device, resolution, slot);
                    break;

                case "C":
                    if (Enum.TryParse<CameraEffectType>(toggleParts[1], out var cameraEffect) == true)
                        Toggle(cameraEffect, device, resolution, slot);
                    break;

                default:
                    throw new NotSupportedException(toggle);
            }
        }

        private void Toggle(MicrophoneEffectType effect, string device, string resolution, int slot)
        {
            var toggle = GetToggleTypeForEffect(effect);

            if (toggle == ToggleType.None)
            {
                var activeToggleCount = this.NumberOfMicrophoneTogglesSet();
                if (activeToggleCount == 0)
                    AddEffect(AddEffectButton.MicSlot1);

                if(activeToggleCount == 1 && slot == 2)
                    AddEffect(AddEffectButton.MicSlot2);
            }

            SetEffect(effect, device, slot);

            for (int i = 0; i < 50; i++)
            {
                toggle = GetToggleTypeForEffect(effect);
                if (toggle != ToggleType.None)
                    break;

                Thread.Sleep(100);
            }

            if (toggle != ToggleType.None)
                Toggle(toggle);
        }


        private void Toggle(SpeakerEffectType effect, string device, string resolution, int slot)
        {
            var toggle = GetToggleTypeForEffect(effect);

            if (toggle == ToggleType.None)
            {
                var activeToggleCount = this.NumberOfSpeakerTogglesSet();
                if (activeToggleCount == 0)
                    AddEffect(AddEffectButton.SpeakerSlot1);

                if (activeToggleCount == 1 && slot == 2)
                    AddEffect(AddEffectButton.SpeakerSlot2);
            }

            SetEffect(effect, device, slot);

            for (int i = 0; i < 50; i++)
            {
                toggle = GetToggleTypeForEffect(effect);
                if (toggle != ToggleType.None)
                    break;

                Thread.Sleep(100);
            }

            if (toggle != ToggleType.None)
                Toggle(toggle);
        }

        private void Toggle(CameraEffectType effect, string device, string resolution, int slot)
        {
            var toggle = GetToggleTypeForEffect(effect);

            if (toggle == ToggleType.None)
            {
                var activeToggleCount = this.NumberOfCameraTogglesSet();
                if (activeToggleCount == 0)
                    AddEffect(AddEffectButton.CameraSlot1);

                if (activeToggleCount == 1 && slot == 2)
                    AddEffect(AddEffectButton.CameraSlot2);
            }

            SetEffect(effect, device, resolution, slot);

            for (int i = 0; i < 50; i++)
            {
                toggle = GetToggleTypeForEffect(effect);
                if (toggle != ToggleType.None)
                    break;

                Thread.Sleep(100);
            }

            if (toggle != ToggleType.None)
                Toggle(toggle);
        }

        /// <summary>
        /// If the toggle is enabled, then turn it off. This is so that when the user
        /// presses the button, and the effect is switched, then the new effect is 
        /// turned on when the operation completes.
        /// </summary>
        /// <param name="comboBox"></param>
        /// <exception cref="NotSupportedException"></exception>
        private void DisableToggle(EffectComboBox comboBox)
        {
            switch (comboBox)
            {
                case EffectComboBox.MicEffect1:
                    if (IsToggleEnabled(ToggleType.MicEffect1) == true)
                        Toggle(ToggleType.MicEffect1);
                    break;

                case EffectComboBox.MicEffect2:
                    if (IsToggleEnabled(ToggleType.MicEffect2) == true)
                        Toggle(ToggleType.MicEffect2);
                    break;

                case EffectComboBox.SpeakerEffect1:
                    if (IsToggleEnabled(ToggleType.SpeakerEffect1) == true)
                        Toggle(ToggleType.SpeakerEffect1);
                    break;

                case EffectComboBox.SpeakerEffect2:
                    if (IsToggleEnabled(ToggleType.SpeakerEffect1) == true)
                        Toggle(ToggleType.SpeakerEffect1);
                    break;

                case EffectComboBox.CameraEffect1:
                    if (IsToggleEnabled(ToggleType.CameraEffectSelected) == true)
                        Toggle(ToggleType.CameraEffectSelected);
                    break;

                case EffectComboBox.CameraEffect2:
                    if (IsToggleEnabled(ToggleType.CameraEffectSelected2) == true)
                        Toggle(ToggleType.CameraEffectSelected2);
                    break;

                default:
                    throw new NotSupportedException(comboBox.ToString());
            }
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

        private void SetEffect(MicrophoneEffectType effect, string device, int slot)
        {
            var effectString = effect switch
            {
                MicrophoneEffectType.NoiseRemoval => _localizedStrings[NvidiaBroadcastResourceID.NoiseRemoval],
                MicrophoneEffectType.RoomEchoRemoval => _localizedStrings[NvidiaBroadcastResourceID.RoomEchoRemoval],
                _ => throw new NotSupportedException()
            };

            ClickButton(TabButton.Microphone);

            SetComboBox(SourceOption.MicrophoneSource, device);

            switch (slot)
            {
                case 1:
                    SetComboBox(EffectComboBox.MicEffect1, effectString);
                    break;

                case 2:
                    SetComboBox(EffectComboBox.MicEffect2, effectString);
                    break;

                default:
                    throw new NotSupportedException(slot.ToString());
            };
        }

        private void SetEffect(SpeakerEffectType effect, string device, int slot)
        {
            var effectString = effect switch
            {
                SpeakerEffectType.NoiseRemoval => _localizedStrings[NvidiaBroadcastResourceID.NoiseRemoval],
                SpeakerEffectType.RoomEchoRemoval => _localizedStrings[NvidiaBroadcastResourceID.RoomEchoRemoval],
                _ => throw new NotSupportedException()
            };

            ClickButton(TabButton.Speakers);

            SetComboBox(SourceOption.SpeakerSource, device);

            switch (slot)
            {
                case 1:
                    SetComboBox(EffectComboBox.SpeakerEffect1, effectString);
                    break;

                case 2:
                    SetComboBox(EffectComboBox.SpeakerEffect2, effectString);
                    break;

                default:
                    throw new NotSupportedException(slot.ToString());
            };
        }

        private void SetEffect(CameraEffectType effect, string device, string resolution, int slot)
        {
            var effectString = effect switch
            {
                CameraEffectType.EyeContact => _localizedStrings[NvidiaBroadcastResourceID.EyeContact],
                CameraEffectType.Vignette => _localizedStrings[NvidiaBroadcastResourceID.Vignette],
                CameraEffectType.BackgroundReplacement => _localizedStrings[NvidiaBroadcastResourceID.BackgroundReplacement],
                CameraEffectType.VideoNoiseRemoval => _localizedStrings[NvidiaBroadcastResourceID.VideoNoiseRemoval],
                CameraEffectType.AutoFrame => _localizedStrings[NvidiaBroadcastResourceID.AutoFrame],
                CameraEffectType.BackgroundBlur => _localizedStrings[NvidiaBroadcastResourceID.BackgroundBlur],
                CameraEffectType.BackgroundRemoval => _localizedStrings[NvidiaBroadcastResourceID.BackgroundRemoval],
                _ => throw new NotSupportedException(effect.ToString())
            };

            ClickButton(TabButton.Camera);

            Debug.WriteLine("Setting camera.");
            SetComboBox(SourceOption.CameraSource, device);
            //Thread.Sleep(10000);

            Debug.WriteLine("Setting resolution.");
            SetComboBox(SourceOption.CameraResolution, resolution);
            //Thread.Sleep(10000);

            switch (slot)
            {
                case 1:
                    SetComboBox(EffectComboBox.CameraEffect1, effectString);
                    break;

                case 2:
                    SetComboBox(EffectComboBox.CameraEffect2, effectString);
                    break;

                default:
                    throw new NotSupportedException(slot.ToString());
            };
        }

        private void AddEffect(AddEffectButton slot)
        {
            // Fix bug in broadcast when effects are removed, toggle is not always reset to 0.
            //switch (slot)
            //{
            //    case AddEffectButton.MicSlot1:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicDenoising", 0);
            //        break;

            //    case AddEffectButton.MicSlot2:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicDenoising2", 0);
            //        break;

            //    case AddEffectButton.SpeakerSlot1:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerDenoising", 0);
            //        break;

            //    case AddEffectButton.SpeakerSlot2:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "SpeakerDenoising2", 0);
            //        break;

            //    case AddEffectButton.CameraSlot1:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue", 0);
            //        break;

            //    case AddEffectButton.CameraSlot2:
            //        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "cameraToggleValue2", 0);
            //        break;

            //    default:
            //        throw new NotSupportedException(slot.ToString());
            //}

            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID != (int)slot)
                        continue;

                    Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.BM_CLICK, 0, 0);
                    //Thread.Sleep(2000);
                }
            }
        }

        internal void SetComboBox(SourceOption source, string value)
        {
            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID != (int)source)
                        continue;

                    var targetPtr = Marshal.StringToHGlobalAuto(value);
                    WPARAM startIndex = new WPARAM((uint)int.MaxValue + 1);

                    Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_SELECTSTRING, startIndex, targetPtr);
                    // Thread.Sleep(1000);

                    Marshal.FreeHGlobal(targetPtr); 

                    // This was hard. Setting the ComboBox value doesn't signal the parent that the value was changed. 
                    // An additional message to the parent control is needed to signal the change.
                    // https://stackoverflow.com/questions/37907524/the-cbn-selchange-notification-not-works-when-i-use-the-function-combobox-setcur
                    unsafe
                    {
                        var parent = GetAncestor(handle, GetAncestorFlags.GA_PARENT);
                        var gwlpID = (ushort)GetWindowLongPtr(handle, WindowLongIndexFlags.GWLP_ID);
                        PostMessage(parent, WindowMessage.WM_COMMAND, new IntPtr(MAKEWPARAM(gwlpID, (ushort)Windows.Win32.PInvoke.CBN_SELCHANGE)), handle);
                    }

                    
                    //Thread.Sleep(250);
                    //while (Windows.Win32.PInvoke.IsWindowEnabled((HWND)handle) == false)
                    //{
                    //    Thread.Sleep(100);
                    //    Debug.WriteLine(source.ToString() + ": Waiting for combobox to re-enable.");
                    //}

                    break;
                }
            }
        }

        int MAKEWPARAM(int l, int h)
        {
            return (l & 0xFFFF) | (h << 16);
        }

        internal void SetComboBox(EffectComboBox comboBox, string value)
        {
            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID != (int)comboBox)
                        continue;

                    
                    string currentValue = GetComboBoxText(comboBox);

                    // Get current combobox string. If it doesn't match current, then turn off the toggle. 
                    // Otherwise, there's nothing to do.
                    if (currentValue.StartsWith(value, StringComparison.InvariantCultureIgnoreCase) == false)
                        DisableToggle(comboBox);
                    else
                        return;


                    var targetPtr = Marshal.StringToHGlobalAuto(value);
                    WPARAM startIndex = new WPARAM((uint)int.MaxValue + 1);

                    //Windows.Win32.PInvoke.SetWindowText((Windows.Win32.Foundation.HWND)handle, value);
                    var index = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_SELECTSTRING, startIndex, targetPtr);
                    //Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_SETCURSEL, (nuint) index.Value, 0);

                    Debug.WriteLine("Index from select: " + index);
                    //Thread.Sleep(1000);

                    // This was hard. Setting the ComboBox value doesn't signal the parent that the value was changed. 
                    // An additional message to the parent control is needed to signal the change.
                    // https://stackoverflow.com/questions/37907524/the-cbn-selchange-notification-not-works-when-i-use-the-function-combobox-setcur
                    unsafe
                    {
                        var parent = GetAncestor(handle, GetAncestorFlags.GA_PARENT);
                        var gwlpID = (ushort)GetWindowLongPtr(handle, WindowLongIndexFlags.GWLP_ID);
                        PostMessage(parent, WindowMessage.WM_COMMAND, new IntPtr(MAKEWPARAM(gwlpID, (ushort)Windows.Win32.PInvoke.CBN_SELCHANGE)), handle);
                    }

                    
                    //Thread.Sleep(250);
                    //while (Windows.Win32.PInvoke.IsWindowEnabled((HWND)handle) == false)
                    //{
                    //    Debug.WriteLine(comboBox.ToString() + ": Waiting for combobox to re-enable.");
                    //    Thread.Sleep(100);
                    //}

                    break;
                }
            }
        }

        internal List<IntPtr> GetWindowHandles()
        {
            var rootWindowHwnd = GetRootWindowFromNvidiaBroadcast();

            EnumedWindow callBackPtr = GetWindowHandle;
            var windowHandles = new List<IntPtr>();

            GCHandle listHandle = GCHandle.Alloc(windowHandles);

            PInvoke.User32.EnumChildWindows(rootWindowHwnd, Marshal.GetFunctionPointerForDelegate(callBackPtr), GCHandle.ToIntPtr(listHandle));

            return windowHandles;
        }

        internal List<String> GetSourceOptions(SourceOption source)
        {
            List<String> returnValue = new List<string>();

            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID != (int) source)
                        continue;

                    StringBuilder stringBuilder = new StringBuilder();
                    GetWindowCaption(handle, stringBuilder, 255);

                    var result = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETCOUNT, 0, 0);

                    Console.WriteLine($"{controlID}: {User32.GetClassName(handle)} - {stringBuilder.ToString()} = {result}");

                    for (ulong i = 0; i < (ulong)result.Value; i++)
                    {
                        var data = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETITEMDATA, (Windows.Win32.Foundation.WPARAM)i, 0);

                        //var c = new char[256];
                        var outPtr = Marshal.AllocHGlobal(256);
                        //Windows.Win32.Foundation.PWSTR str = new Windows.Win32.Foundation.PWSTR(c);
                        //Windows.Win32.PInvoke.GetWindowText(

                        var textLen = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETLBTEXT, (Windows.Win32.Foundation.WPARAM)i, outPtr);

                        var outString = Marshal.PtrToStringAuto(outPtr);
                        Marshal.FreeHGlobal(outPtr);

                        Console.WriteLine($"{i}: {outString}");

                        returnValue.Add(outString);
                    }

                    break;
                }
            }

            return returnValue;
        }

        public string GetComboBoxText(EffectComboBox comboBox)
        {
            return GetComboBoxText((int)comboBox);
        }

        public string GetComboBoxText(SourceOption source)
        {
            return GetComboBoxText((int) source);
        }

        private string GetComboBoxText(int targetControlID)
        {
            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("ComboBox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID != targetControlID)
                        continue;

                    var selectedIndex = Windows.Win32.PInvoke.SendMessage((HWND)handle, Windows.Win32.PInvoke.CB_GETCURSEL, 0, 0);
                    var outPtr = Marshal.AllocHGlobal(256);
                    var textLen = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETLBTEXT, (WPARAM) Convert.ToUInt64(selectedIndex.Value), outPtr);
                    var outString = Marshal.PtrToStringAuto(outPtr);
                    Marshal.FreeHGlobal(outPtr);

                    return outString;
                }
            }

            return String.Empty;
        }


            public void ComboBoxTest()
        {
            foreach (var handle in GetWindowHandles())
            {
                //if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                //{
                //    StringBuilder stringBuilder = new StringBuilder();
                //    GetWindowCaption(handle, stringBuilder, 255);

                //    if (stringBuilder.ToString().Equals("ADD EFFECT") == true)
                //    {
                //        // Get the control's Automation ID. 
                //        var controlID = GetDlgCtrlID(handle);

                //        var isEnabled = Windows.Win32.PInvoke.IsWindowEnabled((HWND)handle);
                //        Console.WriteLine($"Button: {controlID} = {isEnabled}");




                //        //if (controlID == 0x000080E1)
                //        //{
                //        //    var isEnabled = Windows.Win32.PInvoke.IsWindowEnabled((HWND)handle);
                //        //}
                //    }
                //}

                if (User32.GetClassName(handle).Equals("Combobox", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    StringBuilder stringBuilder = new StringBuilder();
                    GetWindowCaption(handle, stringBuilder, 255);


                    var result = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETCOUNT, 0, 0);

                    Console.WriteLine($"{controlID}: {User32.GetClassName(handle)} - {stringBuilder.ToString()} = {result}");

                    for (ulong i = 0; i < (ulong)result.Value; i++)
                    {
                        //var data = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETITEMDATA, (Windows.Win32.Foundation.WPARAM)i, 0);

                        //var c = new char[256];
                        var outPtr = Marshal.AllocHGlobal(256);
                        //Windows.Win32.Foundation.PWSTR str = new Windows.Win32.Foundation.PWSTR(c);
                        //Windows.Win32.PInvoke.GetWindowText(

                        var textLen = Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_GETLBTEXT, (Windows.Win32.Foundation.WPARAM)i, outPtr);

                        var outString = Marshal.PtrToStringAuto(outPtr);
                        Marshal.FreeHGlobal(outPtr);

                        Console.WriteLine($"{i}: {outString}");
                    }

                    // Speakers
                    if (controlID == 1007)
                    {
                        var targetPtr = Marshal.StringToHGlobalAuto("DELL U2718Q (NVIDIA High Definition Audio)");
                        WPARAM startIndex = new WPARAM((uint)int.MaxValue + 1);

                        Windows.Win32.PInvoke.SendMessage((Windows.Win32.Foundation.HWND)handle, Windows.Win32.PInvoke.CB_SELECTSTRING, startIndex, targetPtr);
                    }


                    //if (controlID == (int)toggle)
                    //    User32.SendMessage(handle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                }
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

        private bool ClickButton(TabButton button)
        {
            foreach (var handle in GetWindowHandles())
            {
                if (User32.GetClassName(handle).Equals("Button", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    // Get the control's Automation ID. 
                    var controlID = GetDlgCtrlID(handle);

                    if (controlID == (int)button)
                        User32.SendMessage(handle, User32.WindowMessage.WM_BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                }
            }

            return false;
        }

        private bool ToggleSetting(ToggleType toggle)
        {
            foreach (var handle in GetWindowHandles())
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

        //    if ((int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", "MicEffect1", 3) < 3)
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
