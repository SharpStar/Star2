using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Packets;
using System.Reflection;

namespace SharpStar.Packets
{
    public class PacketTypeCollection : IPacketTypeCollection
    {
        private readonly List<Type> _types;

        public PacketTypeCollection()
        {
            _types = new List<Type>();
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _types.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Type item)
        {
            if (!typeof(IPacket).IsAssignableFrom(item))
                throw new InvalidOperationException("Type must be assignable from IPacket");

            _types.Add(item);
        }

        public void Clear()
        {
            _types.Clear();
        }

        public bool Contains(Type item)
        {
            return _types.Contains(item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            _types.CopyTo(array, arrayIndex);
        }

        public bool Remove(Type item)
        {
            return _types.Remove(item);
        }

        public int Count
        {
            get
            {
                return _types.Count;
            }
        }


        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}
