using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarCommands.Localization
{
    public class CommandLocalization
    {
        public string CommandName { get; set; }

        public Dictionary<string, string> Localizations { get; set; }
    }
}
