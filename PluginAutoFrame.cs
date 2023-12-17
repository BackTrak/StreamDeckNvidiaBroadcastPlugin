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
    [PluginActionId("com.zaphop.nvidiabroadcast.autoframe")]
    public class PluginAutoFrame : PluginToggleControlBase
    {
        public PluginAutoFrame(SDConnection connection, InitialPayload payload) : base("Auto frame", "FaceZoom", connection, payload)
        {
        }
    }
}