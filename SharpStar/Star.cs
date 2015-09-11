using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using SharpStar.Packets;
using SharpStar.Server;
using StarLib;
using StarLib.Server;
using Autofac.Framework.DependencyInjection;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime.Compilation;
using Microsoft.Dnx.Runtime.Loader;
using SharpStar.Events;
using SharpStar.Misc;
using SharpStar.Plugins;
using StarLib.Database;
using StarLib.Events;
using StarLib.Misc;
using StarLib.Packets;
using StarLib.Plugins;

namespace SharpStar
{
    public class Star : IStar
    {
        public static Star Instance
        {
            get
            {
                if (_container == null)
                    throw new NullReferenceException();

                return _container.Resolve<IStar>() as Star;
            }
        }

        public IStarServer Server { get; set; }

        public IConfiguration Configuration { get; set; }

        public event EventHandler<ConsoleInputEventArgs> ConsoleInput;

        private ILogger<IStar> _logger;

        private static ILifetimeScope _lifetimeScope;
        private static IContainer _container;

        internal Star()
        {
            ConfigureServices();
            AddDefaultPackets();
        }

        public void Start()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            LoadResolvables();
            LoadPlugins();

            _logger.LogInformation("Starting server...");

            Server.Start();
        }

        private void ConfigureServices()
        {
            IServiceProvider mainProv = CallContextServiceLocator.Locator.ServiceProvider;
            IApplicationEnvironment appEnv = mainProv.GetService<IApplicationEnvironment>();

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(IStar).GetTypeInfo().Assembly, typeof(Star).GetTypeInfo().Assembly)
                .AsImplementedInterfaces().InstancePerDependency();

            var starTypes = typeof(Star).GetTypeInfo().Assembly.GetTypes();
            var iStarTypes = typeof(IStar).GetTypeInfo().Assembly.GetTypes();

            var rSingleTypes = starTypes.Where(p => typeof(ISingleResolvable).IsAssignableFrom(p)).ToArray();
            var interfaces = iStarTypes.Where(p => p.GetTypeInfo().IsInterface);
            var classes = iStarTypes.Where(p => p.GetTypeInfo().IsClass).ToList();

            var impl = (from i in interfaces
                        from c in classes
                        where i.IsAssignableFrom(c)
                        where i.GetTypeInfo().IsGenericType || c.GetTypeInfo().IsGenericType
                        select new
                        {
                            Interface = i,
                            Class = c
                        }).ToList();

            var allTypes = classes.Select(p => new
            {
                Interface = (Type)null,
                Class = p
            }).Where(p => impl.All(x => x.Class != p.Class) && p.Class.GetTypeInfo().IsGenericType).Union(impl);

            foreach (var t in allTypes)
            {
                if (t.Interface != null)
                    builder.RegisterGeneric(t.Class).As(t.Interface).InstancePerDependency();
                else
                    builder.RegisterGeneric(t.Class).AsSelf().InstancePerDependency();
            }

            builder.RegisterTypes(rSingleTypes).AsSelf().SingleInstance();
            builder.RegisterType<StarEventManager>().As<IStarEventManager>().SingleInstance();
            builder.RegisterType<PacketTypeCollection>().As<IPacketTypeCollection>().SingleInstance();
            builder.RegisterType<StarServer>().As<IStarServer>().SingleInstance();

            builder.RegisterInstance(this).As<IStar>().SingleInstance().AutoActivate().OnActivated(e =>
            {
                e.Instance.Server = e.Context.Resolve<IStarServer>();
                e.Instance.Configuration = e.Context.Resolve<IConfiguration>();
            });

            builder.RegisterType<StarDbContext>().InstancePerDependency();
            builder.RegisterInstance(new SocketAsyncEventArgsPool(100, 150, 65536));

            var configBuilder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddIniFile("star.ini");

            configBuilder.AddEnvironmentVariables();
            Configuration = configBuilder.Build();

            builder.RegisterInstance(Configuration).SingleInstance();

            builder.Populate(Startup.Services);

            //#if !DEBUG
            builder.RegisterModule(Startup.ServiceProvider.GetService<PluginLoader>());
            //#endif

            _container = builder.Build();
            _lifetimeScope = _container.BeginLifetimeScope();

            _logger = Resolve<ILogger<IStar>>();

            _logger.LogInformation("Running " + appEnv.RuntimeFramework.FullName);
            _logger.LogDebug("Finished configuring services!");
        }

        private void LoadResolvables()
        {
            var events = _container.ComponentRegistry.Registrations
                .SelectMany(p => p.Services).OfType<IServiceWithType>()
                .Where(p => typeof(ISingleResolvable).IsAssignableFrom(p.ServiceType) && p.ServiceType != typeof(ISingleResolvable))
                .Select(p => p.ServiceType)
                .Distinct();

            foreach (Type rType in events)
            {
                _container.Resolve(rType); //activate each resolvable
            }
        }

        private void LoadPlugins()
        {
            var plugins = _container.ComponentRegistry.Registrations
                   .SelectMany(p => p.Services).OfType<IServiceWithType>()
                   .Where(p => typeof(IPlugin).IsAssignableFrom(p.ServiceType) && p.ServiceType != typeof(IPlugin))
                   .Select(p => p.ServiceType)
                   .Distinct();

            foreach (Type pluginType in plugins)
            {
                _container.Resolve(pluginType); //activate each plugin
            }
        }

        private void AddDefaultPackets()
        {
            IPacketTypeCollection packetTypes = Resolve<IPacketTypeCollection>();

            var types = typeof(Star).GetTypeInfo().Assembly.GetTypes()
                .Where(p => typeof(IPacket).IsAssignableFrom(p) && !typeof(Packet).IsAssignableFrom(p));

            foreach (Type type in types)
            {
                packetTypes.Add(type);
            }
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public T Resolve<T>(params Tuple<string, object>[] args)
        {
            return _container.Resolve<T>(args.Select(p => new NamedParameter(p.Item1, p.Item2)));
        }

        public T Resolve<T>(params object[] args)
        {
            return _container.Resolve<T>(args.Select(p => new TypedParameter(p.GetType(), p)));
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception.ToString());
        }

        public void Stop()
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

            Server.Stop();
            _lifetimeScope.Dispose();
        }

        internal void RaiseConsoleInputEvent(string input)
        {
            ConsoleInput?.Invoke(this, new ConsoleInputEventArgs(input));
        }
    }
}
