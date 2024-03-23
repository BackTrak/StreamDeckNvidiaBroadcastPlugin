using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using com.zaphop.nvidiabroadcast.Entities;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.zaphop.nvidiabroadcast
{
    [PluginActionId("com.zaphop.nvidiabroadcast.nvidiabroadcast")]
    public class PluginNvidiaBroadcast : KeypadBase
    {
        private readonly NvidiaBroadcastManager _nvidiaBroadcastManager;
        private bool _wasEnabledOnLastTick = false;
        private bool _wasErrorOnLastTick = false;
        private string _toggle = String.Empty;
        private string _device = String.Empty;
        private string _resolution = String.Empty;
        private int _slot = 0;

        private Dictionary<NvidiaBroadcastResourceID, string> _localizedStrings;

        public PluginNvidiaBroadcast(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            UpdateSettings(payload.Settings);

            _nvidiaBroadcastManager = new NvidiaBroadcastManager();
            _localizedStrings = _nvidiaBroadcastManager.GetLocalizedStrings();

            UpdateToggleStatus();

            connection.OnSendToPlugin += Connection_OnSendToPlugin;
        }

        private void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<SendToPlugin> e)
        {
            if (e.Event.Payload.TryGetValue("property_inspector", out var value) == true && value.Value<String>() == "propertyInspectorConnected")
                SendActiveToggleOptions();
        }


        private class ConfigurationOption
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Configured { get; set; }
        }

        private ConfigurationOption GetSelectionOption(NvidiaBroadcastResourceID section, NvidiaBroadcastResourceID option)
        {
            string sectionPart = section switch
            {
                NvidiaBroadcastResourceID.Camera => "C:",
                NvidiaBroadcastResourceID.Microphone => "M:",
                NvidiaBroadcastResourceID.Speakers => "S:",
                _ => throw new NotSupportedException(section.ToString())
            };

            string icon = section switch
            {
                NvidiaBroadcastResourceID.Camera => "📷",
                NvidiaBroadcastResourceID.Microphone => "🎤",
                NvidiaBroadcastResourceID.Speakers => "🔉",
                _ => throw new NotSupportedException(section.ToString())
            };

            var value = sectionPart + option.ToString();
           
            return new ConfigurationOption
            {
                Name = $"    {icon} {_localizedStrings[option]}",
                Value = value,
                Configured = _nvidiaBroadcastManager.IsToggleConfigured(value)
            };
        }

        private void SendActiveToggleOptions()
        {
            var localizedStrings = _nvidiaBroadcastManager.GetLocalizedStrings();

            var toggleData = new
            {
                SelectedToggle = _toggle,
                SelectedDevice = _device,
                SelectedResolution = _resolution,
                SelectedSlot = _slot,
                DefaultCameraSource = _nvidiaBroadcastManager.GetComboBoxText(SourceOption.CameraSource),
                DefaultCameraResolution = _nvidiaBroadcastManager.GetComboBoxText(SourceOption.CameraResolution),
                DefaultMicrophoneSource = _nvidiaBroadcastManager.GetComboBoxText(SourceOption.MicrophoneSource),
                DefaultSpeakerSource = _nvidiaBroadcastManager.GetComboBoxText(SourceOption.SpeakerSource),
                Sections = new[]
                {
                    new
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Camera],
                        SectionType = (int) TabButton.Camera,
                        Toggles = new[]
                        {
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.Vignette),
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.AutoFrame),
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.BackgroundBlur),
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.EyeContact),
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.BackgroundRemoval),
                            GetSelectionOption(NvidiaBroadcastResourceID.Camera, NvidiaBroadcastResourceID.BackgroundReplacement)
                        },
                        Devices = _nvidiaBroadcastManager.GetSourceOptions(SourceOption.CameraSource),
                        Resolutions = _nvidiaBroadcastManager.GetSourceOptions(SourceOption.CameraResolution)
                    },
                    new
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Microphone],
                        SectionType = (int) TabButton.Microphone,
                        Toggles = new[]
                        {
                            GetSelectionOption(NvidiaBroadcastResourceID.Microphone, NvidiaBroadcastResourceID.NoiseRemoval),
                            GetSelectionOption(NvidiaBroadcastResourceID.Microphone, NvidiaBroadcastResourceID.RoomEchoRemoval)
                        },
                        Devices = _nvidiaBroadcastManager.GetSourceOptions(SourceOption.MicrophoneSource),
                        Resolutions = new List<String>()
                    },
                    new
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Speakers],
                        SectionType = (int) TabButton.Speakers,
                        Toggles = new[]
                        {
                            GetSelectionOption(NvidiaBroadcastResourceID.Speakers, NvidiaBroadcastResourceID.NoiseRemoval),
                            GetSelectionOption(NvidiaBroadcastResourceID.Speakers, NvidiaBroadcastResourceID.RoomEchoRemoval)
                        },
                        Devices = _nvidiaBroadcastManager.GetSourceOptions(SourceOption.SpeakerSource),
                        Resolutions = new List<String>()
                    }
                }
            };

            var jobjectData = JObject.FromObject(toggleData);

            Connection.SendToPropertyInspectorAsync(jobjectData);
        }

        private string GetValueKeyForToggle(ToggleType toggle)
        {
            var effectValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\NVIDIA Corporation\\NVIDIA Broadcast\\Settings", toggle.ToString(), -1);
                

            switch (toggle)
            {
                case ToggleType.MicEffect1:
                case ToggleType.MicEffect2:
                    switch ((MicrophoneEffectType)effectValue)
                    {
                        case MicrophoneEffectType.NoiseRemoval:
                            return "M:" + NvidiaBroadcastResourceID.NoiseRemoval;

                        case MicrophoneEffectType.RoomEchoRemoval:
                            return "M:" + NvidiaBroadcastResourceID.RoomEchoRemoval;
                    }
                    break;

                case ToggleType.SpeakerEffect1:
                case ToggleType.SpeakerEffect2:
                    switch ((SpeakerEffectType)effectValue)
                    {
                        case SpeakerEffectType.NoiseRemoval:
                            return "S:" + NvidiaBroadcastResourceID.NoiseRemoval;

                        case SpeakerEffectType.RoomEchoRemoval:
                            return "S:" + NvidiaBroadcastResourceID.RoomEchoRemoval;
                    }
                    break;

                case ToggleType.CameraEffectSelected:
                case ToggleType.CameraEffectSelected2:
                    switch ((CameraEffectType)effectValue)
                    {
                        case CameraEffectType.EyeContact:
                            return "C:" + NvidiaBroadcastResourceID.EyeContact;

                        case CameraEffectType.BackgroundReplacement:
                            return "C:" + NvidiaBroadcastResourceID.BackgroundReplacement;

                        case CameraEffectType.BackgroundRemoval:
                            return "C:" + NvidiaBroadcastResourceID.EyeContact;

                        case CameraEffectType.AutoFrame:
                            return "C:" + NvidiaBroadcastResourceID.EyeContact;

                        case CameraEffectType.BackgroundBlur:
                            return "C:" + NvidiaBroadcastResourceID.EyeContact;

                        case CameraEffectType.Vignette:
                            return "C:" + NvidiaBroadcastResourceID.EyeContact;

                        case CameraEffectType.VideoNoiseRemoval:
                            return "C:" + NvidiaBroadcastResourceID.VideoNoiseRemoval;
                    }
                    break;
            }

            return String.Empty;
        }


        private object PrettyToggleName(string toggleName)
        {
            string returnValue = string.Empty;

            for(int i = 0; i < toggleName.Length; i++) 
            {
                if (i != 0 && (Char.IsUpper(toggleName[i]) == true || Char.IsDigit(toggleName[i]) == true))
                    returnValue += " ";

                returnValue += toggleName[i];
            }

            return returnValue;
        }

        private void UpdateSettings(JObject settings)
        {
            _toggle = settings.Value<String>("toggle");
            _device = settings.Value<String>("device");
            _resolution = settings.Value<String>("resolution");
            var slot = settings.Value<String>("slot");

            Int32.TryParse(slot, out _slot);
        }

        public override void Dispose()
        {
        }

        public override void KeyPressed(KeyPayload payload)
        {
            _nvidiaBroadcastManager.Toggle(_toggle, _device, _resolution, _slot);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() 
        {
            UpdateToggleStatus();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            UpdateSettings(payload.Settings);
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        private void UpdateToggleStatus()
        {
            if (String.IsNullOrWhiteSpace(_toggle) == true)
                return;

            if (_nvidiaBroadcastManager.IsToggleEnabled(_toggle) == true)
            {
                if (_wasEnabledOnLastTick == false)
                {
                    Connection.SetImageAsync(Tools.FileToBase64("Images\\nvidia_active.png", true));
                    _wasEnabledOnLastTick = true;
                }
            }
            else if (_wasEnabledOnLastTick == true)
            {
                Connection.SetImageAsync(Tools.FileToBase64("Images\\nvidia_inactive.png", true));
                _wasEnabledOnLastTick = false;
            }


            //if (_nvidiaBroadcastManager.IsToggleConfigured(_toggle) == false)
            //{
            //    if (_wasErrorOnLastTick == false)
            //    {
            //        SendActiveToggleOptions();
            //        Connection.SetImageAsync(Tools.FileToBase64("Images\\error.png", true));
            //        _wasErrorOnLastTick = true;
            //        _wasEnabledOnLastTick = false;
            //    }
            //}
            //else
            //{
            //    if (_wasErrorOnLastTick == true)
            //    {
            //        SendActiveToggleOptions();
            //        Connection.SetImageAsync(Tools.FileToBase64("Images\\nvidia_inactive.png", true));
            //        _wasErrorOnLastTick = false;
            //        _wasEnabledOnLastTick = false;
            //    }

            //    if (_nvidiaBroadcastManager.IsToggleEnabled(_toggle) == true)
            //    {
            //        if (_wasEnabledOnLastTick == false)
            //        {
            //            Connection.SetImageAsync(Tools.FileToBase64("Images\\nvidia_active.png", true));
            //            _wasEnabledOnLastTick = true;
            //        }
            //    }
            //    else if (_wasEnabledOnLastTick == true)
            //    {
            //        Connection.SetImageAsync(Tools.FileToBase64("Images\\nvidia_inactive.png", true));
            //        _wasEnabledOnLastTick = false;
            //    }
            //}
        }
    }
}