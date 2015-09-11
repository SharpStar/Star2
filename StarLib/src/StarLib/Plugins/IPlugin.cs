using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Plugins
{
    public interface IPlugin
    {
        string Name { get; }

        void OnLoad();

        void OnUnload();
    }
}
