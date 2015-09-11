using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public class ChatReceivedMessage
    {
        public MessageContext Context { get; set; }

        public int ClientId { get; set; }

        public string FromNick { get; set; }

        public string Text { get; set; }

        public ChatReceivedMessage()
        {
            Context = new MessageContext();
            FromNick = string.Empty;
            Text = string.Empty;
        }
    }
}
