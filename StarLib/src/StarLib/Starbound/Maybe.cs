using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Starbound
{
    public abstract class Maybe
    {
    }

    public class Maybe<T> : Maybe
    {
        public T Value { get; set; }
    }
}
