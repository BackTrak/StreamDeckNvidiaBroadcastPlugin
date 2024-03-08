using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
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
        private string _toggle = String.Empty;

        public PluginNvidiaBroadcast(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            UpdateSettings(payload.Settings);

            _nvidiaBroadcastManager = new NvidiaBroadcastManager();
            UpdateToggleStatus();

            connection.OnSendToPlugin += Connection_OnSendToPlugin;
        }

        private void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<SendToPlugin> e)
        {
            if (e.Event.Payload.TryGetValue("property_inspector", out var value) == true && value.Value<String>() == "propertyInspectorConnected")
                SendActiveToggleOptions();
        }

        private void SendActiveToggleOptions()
        {
            var localizedStrings = _nvidiaBroadcastManager.GetLocalizedStrings();

           

            var toggleData = new
            {
                SelectedToggle = _toggle,
                Sections = new[]
                {
                    new 
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Camera],
                        Toggles = new[]
                        {
                            new { Value = "C:" + NvidiaBroadcastResourceID.Vignette, Name = "    📷 " + localizedStrings[NvidiaBroadcastResourceID.Vignette] },
                            new { Value = "C:" + NvidiaBroadcastResourceID.AutoFrame, Name = "    📷 " + localizedStrings[NvidiaBroadcastResourceID.AutoFrame] },
                            new { Value = "C:" + NvidiaBroadcastResourceID.BackgroundBlur, Name = "    📷 " + localizedStrings[NvidiaBroadcastResourceID.BackgroundBlur] },
                            new { Value = "C:" + NvidiaBroadcastResourceID.EyeContact, Name = "    📷 " + localizedStrings[NvidiaBroadcastResourceID.EyeContact] },
                            new { Value = "C:" + NvidiaBroadcastResourceID.BackgroundRemoval, Name = "    📷 " + localizedStrings[NvidiaBroadcastResourceID.BackgroundRemoval] },
                            new { Value = "C:" + NvidiaBroadcastResourceID.BackgroundReplacement, Name = "     📷 " + localizedStrings[NvidiaBroadcastResourceID.BackgroundReplacement] }
                        }
                    },
                    new
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Microphone],
                        Toggles = new[]
                        {
                            new { Value = "M:" + NvidiaBroadcastResourceID.NoiseRemoval, Name = "    🎤 " + localizedStrings[NvidiaBroadcastResourceID.NoiseRemoval] },
                            new { Value = "M:" + NvidiaBroadcastResourceID.RoomEchoRemoval, Name = "    🎤 " + localizedStrings[NvidiaBroadcastResourceID.RoomEchoRemoval] },
                        }
                    },
                    new
                    {
                        SectionName = localizedStrings[NvidiaBroadcastResourceID.Speakers],
                        Toggles = new[]
                        {
                            new { Value = "S:" + NvidiaBroadcastResourceID.NoiseRemoval, Name = "    🔉 " + localizedStrings[NvidiaBroadcastResourceID.NoiseRemoval] },
                            new { Value = "S:" + NvidiaBroadcastResourceID.RoomEchoRemoval, Name = "    🔉 " + localizedStrings[NvidiaBroadcastResourceID.RoomEchoRemoval] },
                        }
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
        }

        public override void Dispose()
        {
        }

        public override void KeyPressed(KeyPayload payload)
        {
            _nvidiaBroadcastManager.Toggle(_toggle);
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
        }
    }
}