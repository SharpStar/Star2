using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SharpStar.Networking;
using StarLib.DataTypes;
using StarLib.Packets;

namespace SharpStar.Packets
{
    public class PacketProcessor : IPacketProcessor
    {
        private readonly object _locker = new object();

        private byte[] _data;

        private byte _currentPacketId;
        private int? _packetLength;

        private int _position;

        private readonly Dictionary<byte, Func<IPacket>> _packetFactory;

        public PacketProcessor(IPacketTypeCollection initialPacketTypes)
        {
            _data = new byte[0];
            _packetFactory = new Dictionary<byte, Func<IPacket>>();
            _packetLength = null;

            foreach (Type packetType in initialPacketTypes)
            {
                AddPacketType(packetType);
            }
        }

        public bool AddPacketType<T>() where T : IPacket
        {
            return AddPacketType(typeof(T));
        }

        public bool AddPacketType(Type packetType)
        {
            IPacket tmpPacket = (IPacket)Activator.CreateInstance(packetType);

            if (_packetFactory.ContainsKey(tmpPacket.PacketId))
                return false;

            _packetFactory.Add(tmpPacket.PacketId, Expression.Lambda<Func<IPacket>>(Expression.New(packetType)).Compile());

            return true;
        }

        public bool RemovePacketType(Type type)
        {
            IPacket tmpPacket = (IPacket)Activator.CreateInstance(type);

            if (!_packetFactory.ContainsKey(tmpPacket.PacketId))
                return false;

            return _packetFactory.Remove(tmpPacket.PacketId);
        }

        public bool RemovePacketType<T>() where T : IPacket
        {
            return RemovePacketType(typeof(T));
        }

        public IEnumerable<IPacket> ProcessData(byte[] data, int offset, int count)
        {
            lock (_locker)
            {
                byte[] newData = new byte[_data.Length + count];
                Buffer.BlockCopy(_data, 0, newData, 0, _data.Length);
                Buffer.BlockCopy(data, offset, newData, _data.Length, count);

                _data = newData;
            }

            var packets = new List<IPacket>();
            var dataQueue = new Queue<byte[]>();
            dataQueue.Enqueue(_data);

            byte[] currentData = null;
            while (dataQueue.Count > 0)
            {
                currentData = dataQueue.Dequeue();

                using (StarReader reader = new StarReader(currentData))
                {
                    _currentPacketId = reader.ReadByte();

                    if (!_packetLength.HasValue)
                    {
                        bool success;

                        _packetLength = (int)VLQ.FromEnumerableSigned(currentData, 1, (int)reader.DataLeft, out _position, out success);
                        _position++;

                        if (!success)
                        {
                            _packetLength = null;

                            break;
                        }
                    }

                    bool compressed = _packetLength < 0;

                    int length = Math.Abs(_packetLength.Value);

                    reader.BaseStream.Seek(_position, SeekOrigin.Begin);

                    if (reader.DataLeft < length)
                        break;

                    byte[] result;
                    if (compressed)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (DeflateStream ds = new DeflateStream(reader.BaseStream, CompressionMode.Decompress, true))
                            {
                                reader.BaseStream.Seek(2, SeekOrigin.Current); //skip header

                                ds.CopyTo(ms);
                            }

                            result = ms.ToArray();
                        }
                    }
                    else
                    {
                        result = reader.ReadBytes(length);
                    }

                    _position += length;

                    IPacket packet;
                    if (_packetFactory.ContainsKey(_currentPacketId))
                    {
                        packet = _packetFactory[_currentPacketId]();
                    }
                    else
                    {
                        packet = new GenericPacket(_currentPacketId) { Data = result };
                    }

                    using (StarReader packetReader = new StarReader(result))
                    {
                        packet.Read(packetReader);
                        packets.Add(packet);
                    }

                    _packetLength = null;

                    int len = currentData.Length - _position;
                    if (len > 0)
                    {
                        byte[] nextData = new byte[len];
                        Buffer.BlockCopy(currentData, _position, nextData, 0, len);

                        dataQueue.Enqueue(nextData);
                    }
                    else
                    {
                        currentData = new byte[0];
                    }

                    _position = 0;
                }
            }

            if (currentData != null)
            {
                lock (_locker)
                {
                    _data = currentData;
                }
            }

            return packets;
        }
    }
}
