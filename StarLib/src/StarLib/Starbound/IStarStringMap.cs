﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public interface IStarStringMap : IReadableWritable
    {
        Dictionary<IStarString, IStarVariant> Map { get; set; }
    }
}
