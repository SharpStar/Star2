using System;
using System.Text;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class Uuid : IUuid
    {
        private byte[] _data;

        public string Id
        {
            get
            {
                return BitConverter.ToString(_data, 0).Replace("-", string.Empty).ToLower();
            }
            set
            {
                byte[] data = Encoding.UTF8.GetBytes(value);

                if (data.Length != 16)
                    throw new InvalidOperationException("Invalid Uuid length");

                _data = data;
            }
        }

        public void ReadFrom(IStarReader reader)
        {
            _data = reader.ReadBytes(16);
        }

        public void WriteTo(IStarWriter writer)
        {
            writer.Write(_data);
        }
    }
}
