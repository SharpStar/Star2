using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarLib;
using StarLib.Database;
using StarLib.Misc;
using StarLib.Packets.Starbound;
using StarLib.Security;
using StarLib.Server;

namespace StarAccounts.Events
{
    public class ClientConnectEvents : ISingleResolvable
    {
        private readonly ConcurrentDictionary<IStarProxy, AuthenticatingProxy> _authenticating
            = new ConcurrentDictionary<IStarProxy, AuthenticatingProxy>();

        private readonly ILogger _logger;
        private readonly IStar _star;
        private readonly StarDbContext _db;

        public ClientConnectEvents(IStar star, StarDbContext db, ILogger<ClientConnectEvents> logger)
        {
            _star = star;
            _db = db;
            _logger = logger;
        }

        public void OnClientConnect(IStarProxy proxy, IClientConnectPacket packet)
        {
            if (!string.IsNullOrEmpty(packet.Account))
            {
                User user = _db.Users.SingleOrDefault(p => p.Username == packet.Account);

                if (user != null)
                {
                    Ban ban = _db.Bans.SingleOrDefault(p => p.UserId == user.UserId);

                    if (ban != null && ban.Active)
                    {
                        IConnectFailurePacket failPacket = _star.Resolve<IConnectFailurePacket>();
                        failPacket.Reason = ban.Reason;

                        proxy.SendPacket(failPacket, Destination.Client);

                        return;
                    }
                }

                _authenticating[proxy] = new AuthenticatingProxy
                {
                    AccountName = packet.Account,
                    User = user
                };

                packet.Account = string.Empty;
                proxy.ServerPaused = true;

                IHandshakeChallengePacket hChallenge = _star.Resolve<IHandshakeChallengePacket>();

                if (user != null)
                    hChallenge.PasswordSalt = user.PasswordSalt;
                else
                    hChallenge.PasswordSalt = StarSecurity.EmptySalt;

                proxy.SendPacket(hChallenge, Destination.Client);
            }
            else
            {
                Ban ban = _db.Bans.SingleOrDefault(p => p.Character.Uuid == packet.Uuid.Id);

                if (ban != null && ban.Active)
                {
                    IConnectFailurePacket failPacket = _star.Resolve<IConnectFailurePacket>();
                    failPacket.Reason = ban.Reason;

                    proxy.SendPacket(failPacket, Destination.Client);
                }
            }
        }

        public void OnHandshakeChallenge(IStarProxy proxy, IHandshakeChallengePacket packet)
        {
            if (_authenticating.ContainsKey(proxy))
            {
                AuthenticatingProxy auth = _authenticating[proxy];

                if (auth.User != null)
                    packet.PasswordSalt = auth.User.PasswordSalt;
                else
                    packet.PasswordSalt = StarSecurity.EmptySalt;
            }
        }

        public void OnHandshakeResponse(IStarProxy proxy, IHandshakeResponsePacket packet)
        {
            if (_authenticating.ContainsKey(proxy))
            {
                packet.Ignore = true;

                AuthenticatingProxy auth = _authenticating[proxy];
                User user = auth.User;
                Character userChar = proxy.Character;

                if (user != null && userChar != null && user.PasswordHash.SequenceEqual(packet.PasswordHash))
                {
                    proxy.AuthenticatedUser = user;
                    proxy.ServerPaused = false;
                    proxy.FlushPackets();

                    user.LastLogin = DateTime.UtcNow;
                    userChar.UserId = user.UserId;

                    _db.Update(user);
                    _db.Update(userChar);
                    _db.SaveChanges();

                    AccountsPlugin.AuthenticatedProxies.AddOrUpdate(proxy, user, (p, u) => user);
                }
                else
                {
                    IConnectFailurePacket failPacket = _star.Resolve<IConnectFailurePacket>();
                    failPacket.Reason = "Invalid username or password!";

                    proxy.SendPacket(failPacket, Destination.Client);
                }

                while (!_authenticating.TryRemove(proxy, out auth))
                    Thread.Sleep(1);
            }
        }
    }
}
