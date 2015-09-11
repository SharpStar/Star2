using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarAccounts.Commands;
using StarAccounts.Events;
using StarCommands.Commands;
using StarLib;
using StarLib.Database;
using StarLib.Events;
using StarLib.Packets.Starbound;
using StarLib.Plugins;
using StarLib.Server;

namespace StarAccounts
{
    public class AccountsPlugin : IPlugin
    {
        public static ConcurrentDictionary<IStarProxy, User> AuthenticatedProxies { get; set; }
            = new ConcurrentDictionary<IStarProxy, User>();

        public static Dictionary<User, IStarProxy> AuthenticatedUsers => AuthenticatedProxies.ToDictionary(k => k.Value, v => v.Key);

        private readonly ILogger _logger;
        private readonly IStarEventManager _eventManager;
        private readonly StarCommandManager _commandManager;
        private readonly ClientConnectEvents _connectEvents;
        private readonly AccountCommands _accountCommands;
        private readonly AdminAccountCommands _adminCommands;

        public string Name => "Accounts";

        public AccountsPlugin(
            ILogger<AccountsPlugin> logger,
            IStarEventManager evtManager,
            StarCommandManager cmdManager,
            ClientConnectEvents connectEvents,
            AccountCommands accountCommands,
            AdminAccountCommands adminCommands
            )
        {
            _logger = logger;
            _eventManager = evtManager;
            
            _commandManager = cmdManager;
            _connectEvents = connectEvents;
            _accountCommands = accountCommands;
            _adminCommands = adminCommands;
        }

        public void OnLoad()
        {
            _commandManager.RegisterCommand("register", _accountCommands.OnRegisterCommand);
            _commandManager.RegisterConsoleCommand("register", _accountCommands.OnConsoleRegisterCommand);

            _commandManager.RegisterCommand("banuser", _adminCommands.OnBanUserCommand);
            _commandManager.RegisterConsoleCommand("banuser", _adminCommands.OnConsoleBanCommand);

            _eventManager.RegisterPacketEvent<IClientConnectPacket>(_connectEvents.OnClientConnect);
            _eventManager.RegisterPacketEvent<IHandshakeChallengePacket>(_connectEvents.OnHandshakeChallenge);
            _eventManager.RegisterPacketEvent<IHandshakeResponsePacket>(_connectEvents.OnHandshakeResponse);
        }

        public void OnUnload()
        {
        }
    }
}
