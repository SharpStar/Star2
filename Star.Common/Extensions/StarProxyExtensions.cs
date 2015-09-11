using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets.Starbound;
using StarLib.Server;
using StarLib.Starbound;

namespace Star.Common.Extensions
{
    public static class StarProxyExtensions
    {
        public static void SendChatMessage(this IStarProxy proxy, ChatReceivedMessage message)
        {
            IChatReceivePacket chatPacket = proxy.Server.Star.Resolve<IChatReceivePacket>();
            chatPacket.ReceivedMessage = message;

            proxy.SendPacket(chatPacket, Destination.Client);
        }

        public static void Kick(this IStarProxy proxy, string reason)
        {
            IServerDisconnectPacket disconnectPacket = proxy.Server.Star.Resolve<IServerDisconnectPacket>();
            disconnectPacket.Reason = reason;

            proxy.SendPacket(disconnectPacket, Destination.Client);
        }
    }
}
