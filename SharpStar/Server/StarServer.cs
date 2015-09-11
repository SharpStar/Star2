using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Framework.Logging;
using StarLib;
using StarLib.Server;

namespace SharpStar.Server
{
    public class StarServer : IStarServer
    {
        private readonly IStar _star;
        private readonly ILogger _logger;

        private readonly object _proxyLocker = new object();
        private readonly List<IStarProxy> _proxies = new List<IStarProxy>();

        public IStar Star
        {
            get
            {
                return _star;
            }
        }

        private long _serverRunning;

        private int _numConnected;
        private int _totalJoined;

        private readonly Socket _sock;

        private readonly string _host;
        private readonly int _port;

        public bool ServerRunning
        {
            get
            {
                return Interlocked.Read(ref _serverRunning) == 1;
            }
            set
            {
                Interlocked.CompareExchange(ref _serverRunning, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public List<IStarProxy> Proxies
        {
            get
            {
                lock (_proxyLocker)
                {
                    return _proxies.ToList();
                }
            }
        }

        public StarServer(IStar star, ILogger<IStarServer> logger)
        {
            _star = star;
            _logger = logger;

            _host = star.Configuration["BindHost"];
            _port = int.Parse(star.Configuration["BindPort"]);

            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#if DNX46
            _sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.ReuseUnicastPort, true);
#endif
        }

        public void Start()
        {
            IPAddress bindAddr;

            if (_host == "*")
                bindAddr = IPAddress.Any;
            else
                bindAddr = Dns.GetHostAddressesAsync(_host).Result.First();

            _sock.Bind(new IPEndPoint(bindAddr, _port));
            _sock.Listen(100);

            ServerRunning = true;

            StartListening();
        }

        public void Stop()
        {
            ServerRunning = false;

            _sock.Dispose();
        }

        private void StartListening()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += (s, e) => AcceptAsync(e);

            if (!_sock.AcceptAsync(args))
                AcceptAsync(args);
        }

        private void AcceptAsync(SocketAsyncEventArgs e)
        {
            Socket acceptSocket = e.AcceptSocket;
            e.AcceptSocket = null;

            if (!ServerRunning)
            {
                acceptSocket.Disconnect(false);
                acceptSocket.Dispose();

                return;
            }

            if (!_sock.AcceptAsync(e))
                AcceptAsync(e);

            if (!acceptSocket.Connected)
            {
                acceptSocket.Dispose();

                AcceptAsync(e);
            }

            Interlocked.Increment(ref _numConnected);
            Interlocked.Increment(ref _totalJoined);

            _logger.LogInformation("Client connected from {0}", acceptSocket.RemoteEndPoint);

            IStarProxy proxy = _star.Resolve<IStarProxy>(acceptSocket);
            proxy.ProxyClosed += (sender, args) =>
            {
                lock (_proxyLocker)
                {
                    _proxies.Remove(proxy);
                }

                _logger.LogInformation("Client {0} disconnected", proxy.ClientSocket.RemoteEndPoint);

                Interlocked.Decrement(ref _numConnected);

                proxy.Dispose();
            };

            lock (_proxyLocker)
            {
                _proxies.Add(proxy);
            }

            proxy.Start();
        }
    }
}
