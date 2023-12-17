using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckTest2
{
	[PluginActionId("com.zaphop.nvidiabroadcast.eycontact")]
	public class PluginEyeContact : PluginToggleControlBase
	{
		public PluginEyeContact(SDConnection connection, InitialPayload payload) : base("Eye Contact", "GazeWarp", connection, payload)
		{
		}
	}
}
