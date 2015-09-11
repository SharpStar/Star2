using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets;
using StarLib.Server;

namespace StarLib.Events
{
    public interface IStarEventManager
    {
        void RegisterPacketEvent<T>(Action<IStarProxy, T> packetEvent) where T : IPacket;

        bool UnregisterPacketEvent<T>(Action<IStarProxy, T> packetEvent) where T : IPacket;

        void CallPacketEvents<T>(IStarProxy proxy, T packet) where T : IPacket;
    }
}
