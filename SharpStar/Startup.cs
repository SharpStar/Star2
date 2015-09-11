using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using SharpStar.Misc;
using SharpStar.Packets;
using SharpStar.Plugins;
using SharpStar.Server;
using SharpStar.Starbound;
using StarLib;
using StarLib.Database;
using StarLib.Packets;
using StarLib.Server;
using StarLib.Starbound;

namespace SharpStar
{
    public class Startup
    {
        public static ServiceCollection Services { get; private set; }

        public static IServiceProvider ServiceProvider { get; private set; }
        
        public void ConfigureServices()
        {
            IServiceProvider mainProv = CallContextServiceLocator.Locator.ServiceProvider;
            IApplicationEnvironment appEnv = mainProv.GetService<IApplicationEnvironment>();
            IRuntimeEnvironment runtimeEnv = mainProv.GetService<IRuntimeEnvironment>();

            ILoggerFactory logFactory = new LoggerFactory();
            logFactory.AddConsole(LogLevel.Information);

            ServiceCollection sc = new ServiceCollection();
            sc.AddInstance(logFactory);
            sc.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            sc.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<StarDbContext>();

            sc.AddSingleton<ILibraryManager, LibraryManager>(factory => mainProv.GetService<ILibraryManager>() as LibraryManager);
            sc.AddSingleton<ICache, Cache>(factory => new Cache(new CacheContextAccessor()));
            sc.AddSingleton<IExtensionAssemblyLoader, ExtensionAssemblyLoader>();
            sc.AddSingleton<IStarLibraryManager, StarLibraryManager>();
            sc.AddSingleton<PluginLoader>();
            sc.AddSingleton(factory => mainProv.GetService<IAssemblyLoadContextAccessor>());
            sc.AddInstance(appEnv);
            sc.AddInstance(runtimeEnv);
            
            Services = sc;

            ServiceProvider = sc.BuildServiceProvider();
        }
    }
}
