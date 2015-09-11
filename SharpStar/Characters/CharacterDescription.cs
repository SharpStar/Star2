using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Characters;
using StarLib.Starbound;

namespace SharpStar.Characters
{
    public class CharacterDescription : ICharacterDescription
    {
        public string Name { get; set; }

        public IUuid Uuid { get; set; }
    }
}
