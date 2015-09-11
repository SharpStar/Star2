using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.Logging;
using StarLib.Events;
using StarLib.Packets;
using StarLib.Server;


namespace SharpStar.Events
{
    public class StarEventManager : IStarEventManager
    {
        private static readonly ConcurrentDictionary<Type, List<Delegate>> PacketEvents = new ConcurrentDictionary<Type, List<Delegate>>();

        public void RegisterPacketEvent<T>(Action<IStarProxy, T> packetEvent) where T : IPacket
        {
            Type packetType = typeof(T);

            var delegates = PacketEvents.GetOrAdd(packetType, new List<Delegate>());

            ParameterExpression proxyExpr = Expression.Parameter(typeof(IStarProxy));
            ParameterExpression packetExpr = Expression.Parameter(typeof(IPacket));
            Expression expr = Expression.Invoke(Expression.Constant(packetEvent), proxyExpr, Expression.Convert(packetExpr, packetType));

            delegates.Add(Expression.Lambda<Action<IStarProxy, IPacket>>(expr, proxyExpr, packetExpr).Compile());
        }

        public bool UnregisterPacketEvent<T>(Action<IStarProxy, T> packetEvent) where T : IPacket
        {
            Type packetType = typeof(T);

            if (!PacketEvents.ContainsKey(packetType))
                return false;

            return PacketEvents[packetType].Remove(packetEvent);
        }

        public void CallPacketEvents<T>(IStarProxy proxy, T packet) where T : IPacket
        {
            Type pType = packet.GetType();

            var events = PacketEvents.Where(p => p.Key.IsAssignableFrom(pType)).SelectMany(p => p.Value).Cast<Action<IStarProxy, IPacket>>();
            foreach (Action<IStarProxy, IPacket> evt in events)
            {
                evt(proxy, packet);
            }
        }
    }
}
