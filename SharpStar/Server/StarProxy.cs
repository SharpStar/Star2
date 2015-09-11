using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Logging;
using SharpStar.Networking;
using SharpStar.Packets;
using SharpStar.Zlib;
using StarLib.DataTypes;
using StarLib.Events;
using StarLib.Packets;
using StarLib.Packets.Starbound;
using StarLib.Server;
using System.Reflection;
using StarLib.Characters;
using StarLib.Database;

namespace SharpStar.Server
{
    public class StarProxy : IStarProxy
    {
        private bool _isDisposed;
        private long _isAlive;
        private long _serverPaused;
        private long _clientPaused;
        private SemaphoreSlim _clientSem;
        private SemaphoreSlim _serverSem;
        private CancellationTokenSource _cancelToken;

        private readonly IPEndPoint _remoteEndPoint;
        private readonly SocketAsyncEventArgsPool _socketArgsPool;
        private readonly ILogger _logger;
        private readonly IStarEventManager _eventManager;

        private readonly BlockingCollection<PacketBacklogItem> _packetBacklog;

        public Socket ClientSocket { get; set; }

        public Socket ServerSocket { get; set; }

        public IPacketProcessor ClientPacketProcessor { get; set; }

        public IPacketProcessor ServerPacketProcessor { get; set; }

        public IStarServer Server { get; set; }

        public User AuthenticatedUser { get; set; }
        public Character Character { get; set; }

        public bool IsAlive
        {
            get
            {
                return Interlocked.Read(ref _isAlive) == 1;
            }
            set
            {
                Interlocked.CompareExchange(ref _isAlive, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public bool ServerPaused
        {
            get
            {
                return Interlocked.Read(ref _serverPaused) == 1;
            }
            set
            {
                Interlocked.CompareExchange(ref _serverPaused, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public bool ClientPaused
        {
            get
            {
                return Interlocked.Read(ref _clientPaused) == 1;
            }
            set
            {
                Interlocked.CompareExchange(ref _clientPaused, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public event EventHandler<PacketEventArgs> PacketSent;

        public event EventHandler<PacketEventArgs> PacketReceived;

        public event EventHandler ProxyClosed;


        public StarProxy(IStarServer server, IConfiguration config, IPacketTypeCollection packetTypes, ILogger<StarProxy> logger,
            IStarEventManager evtManager, SocketAsyncEventArgsPool socketPool, Socket clientSocket)
        {
            if (clientSocket == null)
                throw new ArgumentNullException(nameof(clientSocket));

            Server = server;
            ClientPacketProcessor = new PacketProcessor(packetTypes);
            ServerPacketProcessor = new PacketProcessor(packetTypes);
            ClientSocket = clientSocket;
            _logger = logger;
            _eventManager = evtManager;
            _isDisposed = false;
            _isAlive = 0;
            _socketArgsPool = socketPool;
            _cancelToken = new CancellationTokenSource();
            _clientSem = new SemaphoreSlim(1);
            _serverSem = new SemaphoreSlim(1);
            _packetBacklog = new BlockingCollection<PacketBacklogItem>(new ConcurrentQueue<PacketBacklogItem>());
            _remoteEndPoint = new IPEndPoint(Dns.GetHostAddressesAsync(config["SBHost"]).Result.First(), int.Parse(config["SBPort"]));
        }

        public void Start()
        {
            SocketAsyncEventArgs clientArgs = _socketArgsPool.Get();
            clientArgs.UserToken = Destination.Client;
            clientArgs.Completed += SocketOperationCompleted;

            ClientSocket.NoDelay = true;

            if (!ClientSocket.ReceiveAsync(clientArgs))
                SocketOperationCompleted(this, clientArgs);

            SocketAsyncEventArgs serverArgs = _socketArgsPool.Get();
            serverArgs.Completed += SocketOperationCompleted;
            serverArgs.RemoteEndPoint = _remoteEndPoint;

            ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.NoDelay = true;

            ServerSocket.BeginConnect(_remoteEndPoint, ConnectServer, null);
        }

        private void ConnectServer(IAsyncResult iar)
        {
            ServerSocket.EndConnect(iar);

            IsAlive = true;

            FlushPackets();

            SocketAsyncEventArgs recvArgs = _socketArgsPool.Get();
            recvArgs.UserToken = Destination.Server;
            recvArgs.Completed += SocketOperationCompleted;

            if (!ServerSocket.ReceiveAsync(recvArgs))
                SocketOperationCompleted(this, recvArgs);
        }

        private async void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                //case SocketAsyncOperation.Connect:

                //    if (e.SocketError == SocketError.Success)
                //    {
                //        IsAlive = true;

                //        FlushPackets();

                //        _logger.LogInformation("Client {0} has connected", _remoteEndPoint.ToString());

                //        SocketAsyncEventArgs recvArgs = _socketArgsPool.Get();
                //        recvArgs.UserToken = Destination.Server;
                //        recvArgs.Completed += SocketOperationCompleted;

                //        if (!ServerSocket.ReceiveAsync(recvArgs))
                //            SocketOperationCompleted(this, recvArgs);
                //    }
                //    else
                //    {
                //        throw new Exception("Could not connect to server!");
                //    }
                //    break;
                case SocketAsyncOperation.Receive:
                    await ProcessReceive(e);

                    e.UserToken = null;
                    e.Completed -= SocketOperationCompleted;

                    _socketArgsPool.Add(e);
                    break;
                case SocketAsyncOperation.Send:

                    e.SetBuffer(null, 0, 0);
                    e.Completed -= SocketOperationCompleted;

                    Interlocked.CompareExchange(ref PacketSent, null, null)?.Invoke(this, new PacketEventArgs(this, e.UserToken as IPacket));

                    break;
            }
        }

        public async Task ProcessReceive(SocketAsyncEventArgs e)
        {
            Destination dest = (Destination)e.UserToken;

            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0 && !_cancelToken.IsCancellationRequested)
            {
                Func<SocketAsyncEventArgs, bool> recvFunc = args => dest == Destination.Client ? ClientSocket.ReceiveAsync(args)
                                           : ServerSocket.ReceiveAsync(args);

                SocketAsyncEventArgs newArgs = _socketArgsPool.Get();
                newArgs.Completed += SocketOperationCompleted;
                newArgs.UserToken = dest;

                if (!recvFunc.Invoke(newArgs))
                    SocketOperationCompleted(this, newArgs);

                if (dest == Destination.Client)
                    await _clientSem.WaitAsync(_cancelToken.Token);
                else
                    await _serverSem.WaitAsync(_cancelToken.Token);

                IPacketProcessor processor = dest == Destination.Client ? ClientPacketProcessor : ServerPacketProcessor;
                var packets = processor.ProcessData(e.Buffer, e.Offset, e.BytesTransferred);

                if (dest == Destination.Client)
                    _clientSem.Release();
                else
                    _serverSem.Release();

                foreach (IPacket packet in packets)
                {
                    Interlocked.CompareExchange(ref PacketReceived, null, null)?.Invoke(this, new PacketEventArgs(this, packet));

                    _eventManager.CallPacketEvents(this, packet);

                    if (!packet.Ignore)
                        SendPacket(packet, dest == Destination.Client ? Destination.Server : Destination.Client);
                }
            }
            else
            {
                Stop();
            }
        }

        public void SendPacket(IPacket packet, Destination dest)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");
            
            if (!IsAlive || (dest == Destination.Server && ServerPaused) || (dest == Destination.Client && ClientPaused))
            {
                _packetBacklog.Add(new PacketBacklogItem { Packet = packet, Destination = dest });

                return;
            }

            //FlushPackets();

            if (packet.Ignore)
                return;

            using (StarWriter writer = new StarWriter())
            {
                packet.Write(writer);

                bool compressed = packet.AlwaysCompress || writer.BaseStream.Length > 8192;

                byte[] buffer;
                if (compressed)
                {
                    writer.BaseStream.Seek(0, SeekOrigin.Begin);

                    buffer = ZlibUtils.Compress(writer.ToArray());
                }
                else
                {
                    buffer = writer.ToArray();
                }

                int length = compressed ? -buffer.Length : buffer.Length;
                byte[] lengthBuffer = VLQ.CreateSigned(length);

                var buffers = new List<ArraySegment<byte>>();
                buffers.Add(new ArraySegment<byte>(new[] { packet.PacketId }));
                buffers.Add(new ArraySegment<byte>(lengthBuffer));
                buffers.Add(new ArraySegment<byte>(buffer));

                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.UserToken = packet;
                sendArgs.Completed += SocketOperationCompleted;
                sendArgs.BufferList = buffers;

                switch (dest)
                {
                    case Destination.Server:
                        if (ServerSocket != null && !ServerSocket.SendAsync(sendArgs))
                            SocketOperationCompleted(this, sendArgs);
                        break;
                    case Destination.Client:
                        if (ClientSocket != null && !ClientSocket.SendAsync(sendArgs))
                            SocketOperationCompleted(this, sendArgs);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid direction!");
                }
            }
        }

        public void FlushPackets()
        {
            if (!IsAlive)
                return;

            PacketBacklogItem backlog;
            while (_packetBacklog.TryTake(out backlog))
            {
                SendPacket(backlog.Packet, backlog.Destination);
            }
        }

        public void Stop()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            _cancelToken.Cancel();

            try
            {
                ClientSocket.Disconnect(false);
                ServerSocket.Disconnect(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            finally
            {
                Interlocked.CompareExchange(ref ProxyClosed, null, null)?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;

            if (disposing)
            {
                ClientSocket.Dispose();
                ServerSocket.Dispose();

                _clientSem.Dispose();
                _serverSem.Dispose();

                _cancelToken.Dispose();
            }

            ClientSocket = null;
            ServerSocket = null;

            _clientSem = null;
            _serverSem = null;

            _cancelToken = null;
        }

        ~StarProxy()
        {
            Dispose(false);
        }
    }
}
