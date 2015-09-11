using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpStar.Characters;
using StarLib;
using StarLib.Database;
using StarLib.Events;
using StarLib.Misc;
using StarLib.Packets.Starbound;
using StarLib.Server;

namespace SharpStar.Events.SharpStarEvents
{
    public class ClientConnectEvents : ISingleResolvable
    {
        private readonly IStar _star;
        private readonly StarDbContext _db;

        public ClientConnectEvents(IStar star, StarDbContext db, IStarEventManager evtManager)
        {
            _star = star;
            _db = db;

            evtManager.RegisterPacketEvent<IClientConnectPacket>(OnClientConnect);
        }

        private void OnClientConnect(IStarProxy proxy, IClientConnectPacket packet)
        {
            Character ch = _db.Characters.SingleOrDefault(p => p.Uuid == packet.Uuid.Id);
            bool update = ch != null;

            if (!update)
            {
                ch = new Character();
                _db.Characters.Add(ch);
            }

            ch.Name = packet.PlayerName;
            ch.Uuid = packet.Uuid.Id;

            string ip = ((IPEndPoint)proxy.ClientSocket.RemoteEndPoint).Address.ToString();

            CharacterIp chIp = _db.CharacterIps.SingleOrDefault(p => p.Address == ip && p.CharacterId == ch.CharacterId);

            if (chIp == null)
            {
                _db.CharacterIps.Add(new CharacterIp
                {
                    Character = ch,
                    Address = ip
                });
            }

            if (update)
                _db.Update(ch);

            _db.SaveChanges();

            proxy.Character = ch;
        }
    }
}
