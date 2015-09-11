using StarLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using StarLib.Characters;
using StarLib.Database;
using StarLib.Packets;

namespace StarLib.Server
{
    public interface IStarProxy : IDisposable
    {
        Socket ClientSocket { get; set; }

        Socket ServerSocket { get; set; }

        IPacketProcessor ClientPacketProcessor { get; set; }

        IPacketProcessor ServerPacketProcessor { get; set; }

        IStarServer Server { get; set; }

        User AuthenticatedUser { get; set; }
        
        Character Character { get; set; }

        bool IsAlive { get; set; }

        bool ServerPaused { get; set; }

        bool ClientPaused { get; set; }

        event EventHandler<PacketEventArgs> PacketSent;

        event EventHandler<PacketEventArgs> PacketReceived;

        event EventHandler ProxyClosed;

        void SendPacket(IPacket packet, Destination dest);

        void Start();

        void Stop();

        void FlushPackets();
    }
}
