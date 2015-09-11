using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public class MessageContext
    {
        public Mode Mode { get; set; }

        public string ChannelName { get; set; }

        public MessageContext()
        {
            ChannelName = string.Empty;
        }
    }
}
