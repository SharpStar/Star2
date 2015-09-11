using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Packets
{
    public interface IEither<TLeft, TRight>
    {
        TLeft Left { get; set; }

        TRight Right { get; set; }
    }
}
