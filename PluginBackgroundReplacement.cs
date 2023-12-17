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
    [PluginActionId("com.zaphop.nvidiabroadcast.backgroundreplacement")]
    public class PluginBackgroundReplacement : PluginToggleControlBase
    {
        public PluginBackgroundReplacement(SDConnection connection, InitialPayload payload) : base("Background replacement", "BackgroundReplacement", connection, payload)
        {
        }
    }
}