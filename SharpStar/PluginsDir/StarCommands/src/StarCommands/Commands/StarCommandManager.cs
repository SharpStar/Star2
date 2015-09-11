using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarLib;
using StarLib.Misc;
using StarLib.Server;

namespace StarCommands.Commands
{
    public class StarCommandManager : ISingleResolvable
    {
        private readonly Dictionary<string, Func<IStarProxy, StarCommand, bool>> _commands
            = new Dictionary<string, Func<IStarProxy, StarCommand, bool>>();

        private readonly Dictionary<string, Func<StarCommand, bool>> _consoleCommands
            = new Dictionary<string, Func<StarCommand, bool>>();
        
        private readonly ILogger _logger;

        public StarCommandManager(IStar star, ILogger<StarCommandManager> logger)
        {
            _logger = logger;

            star.ConsoleInput += (s, e) => PassConsoleCommand(e.Input);
        }

        public bool PassCommand(IStarProxy proxy, string command)
        {
            StarCommand cmd = StarCommandParser.ParseCommand(command);

            if (cmd == null)
                return false;

            Func<IStarProxy, StarCommand, bool> func = _commands.SingleOrDefault(p => p.Key.Equals(cmd.CommandName, StringComparison.CurrentCultureIgnoreCase))
                .Value;
            
            return func != null && func(proxy, cmd);
        }

        public bool PassConsoleCommand(string command)
        {
            StarCommand cmd = StarCommandParser.ParseCommand(command);

            if (cmd == null)
                return false;

            Func<StarCommand, bool> func = _consoleCommands.SingleOrDefault(p => p.Key.Equals(cmd.CommandName, StringComparison.CurrentCultureIgnoreCase))
                .Value;

            return func != null && func(cmd);
        }

        public void RegisterConsoleCommand(string commandName, Func<StarCommand, bool> command)
        {
            _consoleCommands[commandName] = command;
        }

        public bool UnregisterConsoleCommand(string commandName)
        {
            return _commands.Remove(commandName);
        }

        public void RegisterCommand(string commandName, Func<IStarProxy, StarCommand, bool> command)
        {
            _commands[commandName] = command;
        }

        public bool UnregisterCommand(string commandName)
        {
            return _commands.Remove(commandName);
        }
    }
}
