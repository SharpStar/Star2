using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarCommands.Commands;
using StarLib.Misc;
using StarLib.Packets.Starbound;
using StarLib.Server;

namespace StarCommands.Events
{
    public class ChatEvents : ISingleResolvable
    {
        private readonly ILogger _logger;
        private readonly StarCommandManager _commandManager;

        public ChatEvents(ILogger<ChatEvents> logger, StarCommandManager cmdManager)
        {
            _logger = logger;
            _commandManager = cmdManager;
        }

        public void OnChatSent(IStarProxy proxy, IChatSentPacket packet)
        {
            if (packet.Message.StartsWith("/"))
            {
                if (_commandManager.PassCommand(proxy, packet.Message))
                    packet.Ignore = true;
            }
        }
    }
}
