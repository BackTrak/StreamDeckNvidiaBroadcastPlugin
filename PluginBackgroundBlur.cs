using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckTest2
{
    [PluginActionId("com.zaphop.nvidiabroadcast.backgroundblur")]
    public class PluginBackgroundBlur : PluginToggleControlBase
    {
        public PluginBackgroundBlur(SDConnection connection, InitialPayload payload) : base("Background blur", "BackgroundBlur", connection, payload)
        {
        }
    }
}