using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using StarLib.Database;
using StarLib.Events;
using StarLib.Server;

namespace StarLib
{
    public interface IStar
    {
        IStarServer Server { get; }

        IConfiguration Configuration { get; set; }

        event EventHandler<ConsoleInputEventArgs> ConsoleInput;

        void Start();

        void Stop();

        T Resolve<T>();

        T Resolve<T>(params Tuple<string, object>[] args);

        T Resolve<T>(params object[] args);
    }
}
