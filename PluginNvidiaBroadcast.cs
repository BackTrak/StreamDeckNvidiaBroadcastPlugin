using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
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
        private ToggleType _toggle = ToggleType.None;

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
            var activeToggleList = _nvidiaBroadcastManager.GetAvailableToggleTypes();

            var toggleData = new 
            { 
                ToggleList = activeToggleList.Select(p => new { Name = PrettyToggleName(p.ToString()), ID = (int)p }), 
                SelectedToggle = _toggle
            };

            var jobjectData = JObject.FromObject(toggleData);

            Connection.SendToPropertyInspectorAsync(jobjectData);
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
            if (Enum.TryParse<ToggleType>(settings.Value<String>("toggle"), out _toggle) == false)
                _toggle = ToggleType.None;
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
            if (_toggle == ToggleType.None)
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