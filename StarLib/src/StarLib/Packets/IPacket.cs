using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;

namespace StarLib.Packets
{
    /// <summary>
    /// Represents a Starbound Packet
    /// </summary>
    public interface IPacket
    {
        /// <summary>
        /// The packet's identifier
        /// </summary>
        byte PacketId { get; }

        /// <summary>
        /// Specifies whether this packet should be sent or not
        /// </summary>
        bool Ignore { get; set; }

        bool IsReceive { get; set; }

        /// <summary>
        /// Specifies whether this packet should be compressed
        /// </summary>
        bool AlwaysCompress { get; set; }

        /// <summary>
        /// The origin of this packet
        /// </summary>
        Origin Origin { get; set; }

        /// <summary>
        /// Invoked when this packet should be read
        /// </summary>
        /// <param name="reader">The stream to read from</param>
        void Read(IStarReader reader);

        /// <summary>
        /// Invoked when this packet should be written
        /// </summary>
        /// <param name="writer">The stream to write to</param>
        void Write(IStarWriter writer);
    }
}
