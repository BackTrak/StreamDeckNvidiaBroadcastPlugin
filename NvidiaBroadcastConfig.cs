using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.zaphop.nvidiabroadcast
{
    public class Effect
    {
        public string Name { get; set; }
        public bool PerfModeEnabled { get; set; }
    }

    public class NvidiaBroadcastConfig
    {
        public RTXCamera RTXCamera { get; set; }
    }

    public class RTXCamera
    {
        public List<Stream> Streams { get; set; }
    }

    public class Stream
    {
        public List<Effect> Effect { get; set; }
        public string Input { get; set; }
        public int InputFpsDenominator { get; set; }
        public int InputFpsNumerator { get; set; }
        public int InputHeight { get; set; }
        public string InputType { get; set; }
        public int InputWidth { get; set; }
        public string Layer { get; set; }
    }
}
