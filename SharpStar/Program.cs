using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using SharpStar.Networking;
using SharpStar.Packets;
using SharpStar.Packets.Serialization;
using SharpStar.Server;
using StarLib;
using StarLib.Database;
using StarLib.Networking;
using SharpStar.Starbound;
using StarLib.Packets;
using StarLib.Server;
using StarLib.Starbound;

namespace SharpStar
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public void Main(string[] args)
        {
            Startup startup = new Startup();
            startup.ConfigureServices();

            Star star = new Star();
            star.Start();
            
            string input;
            while ((input = Console.ReadLine()) != null)
            {
                input = input.ToLower();

                star.RaiseConsoleInputEvent(input);

                if (input == "exit")
                    break;
            }

            star.Stop();
        }
    }
}
