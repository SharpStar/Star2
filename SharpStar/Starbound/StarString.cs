using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using StarLib.Starbound;

namespace SharpStar.Starbound
{
    public class StarString : IStarString
    {
        public string Value { get; set; }

        public StarString()
        {
        }

        public StarString(string val)
        {
            Value = val;
        }

        public static implicit operator StarString(string val)
        {
            return new StarString { Value = val };
        }

        public static implicit operator string (StarString starStr)
        {
            return starStr.Value;
        }

        public void ReadFrom(IStarReader reader)
        {
            Value = reader.ReadString();
        }

        public void WriteTo(IStarWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
