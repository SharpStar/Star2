using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Server
{
    public interface IStarServer
    {
        IStar Star { get; }

        List<IStarProxy> Proxies { get; }

        void Start();

        void Stop();
    }
}
