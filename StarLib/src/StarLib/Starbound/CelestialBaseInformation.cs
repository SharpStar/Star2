using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public struct CelestialBaseInformation
    {
        public int PlanetOrbitalLevels { get; set; }

        public int SatelliteOrbitalLevels { get; set; }

        public int ChunkSize { get; set; }

        public Vec2I XyCoordRange { get; set; }

        public Vec2I ZCoordRange { get; set; }
    }
}
