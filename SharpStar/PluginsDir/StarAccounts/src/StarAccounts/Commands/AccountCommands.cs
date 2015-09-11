using System.Linq;
using Microsoft.Framework.Logging;
using Star.Common.Extensions;
using StarCommands.Commands;
using StarLib;
using StarLib.Database;
using StarLib.Misc;
using StarLib.Security;
using StarLib.Server;
using StarLib.Starbound;

namespace StarAccounts.Commands
{
    public class AccountCommands : ISingleResolvable
    {
        private readonly IStar _star;
        private readonly ILogger _logger;
        private readonly StarDbContext _db;

        public AccountCommands(IStar star, ILogger<AccountCommands> logger, StarDbContext db)
        {
            _star = star;
            _logger = logger;
            _db = db;
        }

        public bool OnConsoleRegisterCommand(StarCommand command)
        {
            if (command.Arguments.Length != 3)
            {
                _logger.LogInformation("Syntax: register <username> <password> <confirm>");

                return false;
            }

            if (!RegisterUser(command.Arguments[0], command.Arguments[1]))
            {
                _logger.LogError($"The user {command.Arguments[0]} already exists!");
            }

            _logger.LogInformation($"The user {command.Arguments[0]} has been created!");

            return true;
        }

        public bool OnRegisterCommand(IStarProxy proxy, StarCommand command)
        {
            var resp = new ChatReceivedMessage
            {
                Context = new MessageContext
                {
                    Mode = Mode.CommandResult
                }
            };

            if (command.Arguments.Length != 3)
            {
                resp.Text = "Syntax: /register <username> <password> <confirm>";
                proxy.SendChatMessage(resp);

                return false;
            }

            if (!RegisterUser(command.Arguments[0], command.Arguments[1]))
            {
                resp.Text = $"The username {command.Arguments[0]} has already been taken!";
                proxy.SendChatMessage(resp);

                return false;
            }

            resp.Text = $"The account {command.Arguments[0]} has been created!";
            proxy.SendChatMessage(resp);

            return true;
        }

        private bool RegisterUser(string username, string password)
        {
            if (_db.Users.Any(p => p.Username == username))
                return false;

            byte[] salt = StarSecurity.GenerateSalt();
            byte[] hash = StarSecurity.GenerateHash(username, password, salt);

            _db.Users.Add(new User
            {
                Username = username,
                PasswordSalt = salt,
                PasswordHash = hash
            });

            _db.SaveChanges();

            return true;
        }
    }
}
