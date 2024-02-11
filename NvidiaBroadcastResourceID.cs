using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.zaphop.nvidiabroadcast
{
    /// <summary>
    /// These IDs are found by looking at the combobox in NVidia Broadcast under the camera effects tab, and then 
    /// searching the string table in Resource Hacker to get the resource ID. These IDs should be static over builds, so they shoulnd't change.
    /// </summary>
    public enum NvidiaBroadcastResourceID
    {
        BackgroundRemoval = 32832, // Background removal
        AutoFrame = 188, // Auto frame
        BackgroundBlur = 186, // Background blur
        BackgroundReplacement = 187, // Background replacement
        EyeContact = 32873, // Eye contact
        Vignette = 33048
    }
}
