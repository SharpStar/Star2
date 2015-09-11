using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using SharpStar.Misc;
using StarLib.Misc;
using StarLib.Plugins;

namespace SharpStar.Plugins
{
    public class PluginLoader : Autofac.Module
    {
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;
        private readonly IApplicationEnvironment _appEnv;
        private readonly ILogger _logger;

        public string Name => GetType().Name;

        public PluginLoader(
            IApplicationEnvironment appEnv,
            IExtensionAssemblyLoader extensionAssemblyLoader,
            ILoggerFactory loggerFactory)
        {
            _appEnv = appEnv;
            _extensionAssemblyLoader = extensionAssemblyLoader;
            _logger = loggerFactory.CreateLogger<PluginLoader>();
        }

        protected override void Load(ContainerBuilder builder)
        {
            string plocation = Path.Combine(_appEnv.ApplicationBasePath, "PluginsDir");

            foreach (DirectoryInfo dInfo in new DirectoryInfo(plocation).GetDirectories("src", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly assembly = _extensionAssemblyLoader.WithPath(dInfo.FullName).Load(new AssemblyName(dInfo.Parent.Name));

                    var singleResolvables = assembly.ExportedTypes.Where(p => typeof(ISingleResolvable).IsAssignableFrom(p) && p.GetTypeInfo().IsClass)
                        .ToArray();

                    var pluginTypes = assembly.ExportedTypes.Where(p => typeof(IPlugin).IsAssignableFrom(p));

                    builder.RegisterTypes(singleResolvables).AsSelf().SingleInstance();
                    
                    //builder.RegisterTypes(perResolvables).AsSelf().InstancePerDependency();
                    //singleResolvables.ForEach(p => builder.RegisterType(p).AsSelf().SingleInstance());
                    //perResolvables.ForEach(p => builder.RegisterType(p).AsSelf().InstancePerDependency());

                    foreach (Type type in pluginTypes)
                    {
                        builder.RegisterType(type).AsSelf().SingleInstance().OnActivated(
                            e =>
                            {
                                try
                                {
                                    IPlugin plugin = (IPlugin)e.Context.Resolve(type);

                                    plugin.OnLoad();

                                    _logger.LogInformation("Plugin {0} has been loaded.", plugin.Name);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Error trying to load plugin {0}", dInfo.Parent.Name);

                                    StackTrace st = new StackTrace(ex, true);
                                    StackFrame[] frames = st.GetFrames();

                                    if (frames != null)
                                    {
                                        foreach (StackFrame f in frames)
                                        {
                                            _logger.LogError(f.ToString());
                                        }
                                    }

                                    _logger.LogError(ex.ToString());
                                }
                            });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error trying to load plugin {0}", dInfo.Parent.Name);

                    StackTrace st = new StackTrace(ex, true);
                    StackFrame[] frames = st.GetFrames();

                    if (frames != null)
                    {
                        foreach (StackFrame f in frames)
                        {
                            _logger.LogError(f.ToString());
                        }
                    }

                    _logger.LogError(ex.ToString());
                }
            }
        }
    }
}
