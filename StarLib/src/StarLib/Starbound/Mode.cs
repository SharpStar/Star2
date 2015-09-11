using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public enum Mode : byte
    {
        Channel = 0,
        Broadcast = 1,
        Whisper = 2,
        CommandResult = 3
    }
}
