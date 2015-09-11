using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Starbound;

namespace StarLib.Characters
{
    public interface ICharacterDescription
    {
        string Name { get; set; }

        IUuid Uuid { get; set; }
    }
}
