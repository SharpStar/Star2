﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public interface IStarVariantArray : IReadableWritable
    {
        List<IStarVariant> Array { get; set; }
    }
}
