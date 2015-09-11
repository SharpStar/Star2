using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarCommands.Commands;
using Star.Common.Extensions;
using StarLib.Database;
using StarLib.Misc;
using StarLib.Server;
using StarLib.Starbound;
using MoreLinq;

namespace StarAccounts.Commands
{
    public class AdminAccountCommands : ISingleResolvable
    {
        private readonly IStarServer _server;
        private readonly ILogger _logger;
        private readonly StarDbContext _db;

        public AdminAccountCommands(ILogger<AdminAccountCommands> logger, IStarServer server, StarDbContext db)
        {
            _logger = logger;
            _server = server;
            _db = db;
        }

        public bool OnBanUserCommand(IStarProxy proxy, StarCommand command)
        {
            if (!proxy.AuthenticatedUser.Admin)
                return false;

            ChatReceivedMessage message = new ChatReceivedMessage
            {
                Context = new MessageContext
                {
                    Mode = Mode.CommandResult
                }
            };

            if (command.Arguments.Length != 2)
            {
                message.Text = "Syntax: /banuser <uuid> \"<reason>\" <all characters? (optional)>";

                proxy.SendChatMessage(message);
            }

            string uuid = command.Arguments[0];
            string reason = command.Arguments[1];
            string allCharsStr = command.Arguments[1]?.ToLower();

            bool allChars = allCharsStr == "yes" || allCharsStr == "true" || allCharsStr == "1";

            if (ActivateBanHammer(proxy.Server, uuid, reason, allChars))
                message.Text = $"Player with uuid {uuid} has been banned!";
            else
                message.Text = $"Error while trying to ban {uuid}!";

            proxy.SendChatMessage(message);

            return true;
        }

        public bool OnConsoleBanCommand(StarCommand command)
        {
            if (command.Arguments.Length != 2)
            {
                _logger.LogInformation("Syntax: banuser <uuid> \"<reason>\" <all characters? (optional)>");

                return true;
            }

            string uuid = command.Arguments[0];
            string reason = command.Arguments[1];
            string allCharsStr = command.Arguments[1]?.ToLower();

            bool allChars = allCharsStr == "yes" || allCharsStr == "true" || allCharsStr == "1";

            if (ActivateBanHammer(_server, uuid, reason, allChars))
                _logger.LogInformation($"Player with uuid {uuid} has been banned!");
            else
                _logger.LogError($"Error banning player with uuid {uuid}");

            return true;
        }

        private bool ActivateBanHammer(IStarServer server, string uuid, string reason, bool includeUserChars, bool active = true)
        {
            Character banChar = _db.Characters.SingleOrDefault(p => p.Uuid == uuid);
            
            if (banChar == null)
                return false;

            IStarProxy banProxy = server.Proxies.SingleOrDefault(p => p.Character.CharacterId == banChar.CharacterId);

            User user = banProxy?.AuthenticatedUser;

            Ban ban;
            if (user != null)
            {
                ban = _db.Bans.SingleOrDefault(p => p.User.UserId == user.UserId);
            }
            else
            {
                ban = _db.Bans.SingleOrDefault(p => p.CharacterId == banChar.CharacterId);
            }
            
            if (ban == null)
            {
                Character[] chars = { banChar };

                if (user != null && includeUserChars)
                    chars = chars.Concat(user.Characters).DistinctBy(p => p.CharacterId).ToArray();
                
                foreach (Character userChar in chars)
                {
                    ban = new Ban
                    {
                        Character = userChar,
                        User = user,
                        Reason = reason,
                        Active = active,
                    };

                    _db.Bans.Add(ban);
                }
            }
            else
            {
                ban.Active = active;
                ban.Reason = reason;

                _db.Update(ban);
            }

            _db.SaveChanges();
            
            banProxy?.Kick(reason);

            return true;
        }
    }
}
