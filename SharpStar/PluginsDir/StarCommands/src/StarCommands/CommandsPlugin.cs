using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarCommands.Events;
using StarLib;
using StarLib.Events;
using StarLib.Packets.Starbound;
using StarLib.Plugins;

namespace StarCommands
{
    public class CommandsPlugin : IPlugin
    {
        public string Name => "Commands";

        private readonly IStar _star;
        private readonly ILogger _logger;
        private readonly IStarEventManager _eventManager;

        public CommandsPlugin(IStar star, ILogger<CommandsPlugin> logger, IStarEventManager evtManager)
        {
            _star = star;
            _logger = logger;
            _eventManager = evtManager;
        }

        public void OnLoad()
        {
            ChatEvents events = _star.Resolve<ChatEvents>();
            
            _eventManager.RegisterPacketEvent<IChatSentPacket>(events.OnChatSent);
        }

        public void OnUnload()
        {
        }
    }
}
