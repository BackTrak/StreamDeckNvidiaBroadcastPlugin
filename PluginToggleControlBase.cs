using BarRaider.SdTools;
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
    public class PluginToggleControlBase : KeypadBase
    {
        private readonly NvidiaBroadcastManager _nvidiaBroadcastManager;
        //private bool _wasEnabledOnLastTick = false;

        public PluginToggleControlBase(NvidiaBroadcastResourceID toggleResourceID, string configName, SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            _nvidiaBroadcastManager = new NvidiaBroadcastManager();
            UpdateToggleStatus();
        }

        public override void Dispose()
        {
        }

        public override void KeyPressed(KeyPayload payload)
        {
            //_nvidiaBroadcastManager.Toggle();
        }

        public override void KeyReleased(KeyPayload payload) { }

      
        public override void OnTick() 
        {
            UpdateToggleStatus();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        private void UpdateToggleStatus()
        {
            //if (_nvidiaBroadcastManager.IsToggleEnabled() == true)
            //{
            //    if (_wasEnabledOnLastTick == false)
            //    {
            //        Connection.SetImageAsync(Tools.FileToBase64("Images\\green72.png", true));
            //        _wasEnabledOnLastTick = true;
            //    }
            //}
            //else if (_wasEnabledOnLastTick == true)
            //{
            //    Connection.SetImageAsync(Tools.FileToBase64("Images\\gray72.png", true));
            //    _wasEnabledOnLastTick = false;
            //}
        }
    }
}