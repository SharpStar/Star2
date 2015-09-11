using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Loader;
using Microsoft.Framework.Logging;

namespace SharpStar.Misc
{
    public interface IExtensionAssemblyLoader : IAssemblyLoader
    {
        IExtensionAssemblyLoader WithPath(string path);
    }

    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader
    {
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ICache _cache;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly IStarLibraryManager _libraryManager;
        private readonly ILogger _logger;

        private string _path;

        public ExtensionAssemblyLoader(
            IApplicationEnvironment applicationEnvironment,
            ICache cache,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IRuntimeEnvironment runtimeEnv,
            IStarLibraryManager libraryManager,
            ILogger<ExtensionAssemblyLoader> logger)
        {
            _applicationEnvironment = applicationEnvironment;
            _cache = cache;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _runtimeEnvironment = runtimeEnv;
            _libraryManager = libraryManager;
            _logger = logger;

#if !DNXCORE50
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif
        }

#if !DNXCORE50
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Load(new AssemblyName(args.Name));
        }
#endif

        public IExtensionAssemblyLoader WithPath(string path)
        {
            _path = path;
            return this;
        }

        public Assembly Load(AssemblyName assemblyName)
        {
            return _cache.Get<Assembly>(assemblyName.Name, cacheContext =>
            {
                var reference = _libraryManager.GetMetadataReference(assemblyName.Name);

                if (reference is MetadataFileReference)
                {
                    var fileReference = (MetadataFileReference)reference;

                    var assembly = _assemblyLoadContextAccessor
                        .Default
                        .LoadFile(fileReference.Path);

                    return assembly;
                }

                var projectPath = Path.Combine(_path, assemblyName.Name);

                if (!Project.HasProjectFile(projectPath))
                {
                    return null;
                }

                var moduleContext = new ModuleLoaderContext(
                    projectPath,
                    _applicationEnvironment.RuntimeFramework);

                foreach (var lib in moduleContext.LibraryManager.GetLibraries())
                {
                    _libraryManager.AddLibrary(lib);
                }

                var ctx = new CompilationEngineContext(
                    _applicationEnvironment,
                    _runtimeEnvironment,
                    _assemblyLoadContextAccessor.Default,
                    new CompilationCache());

                var engine = new CompilationEngine(ctx);

                var exporter = engine.CreateProjectExporter(
                    moduleContext.Project, moduleContext.TargetFramework, _applicationEnvironment.Configuration);

                var exports = exporter.GetAllExports(moduleContext.Project.Name);

                foreach (var metadataReference in exports.MetadataReferences)
                {
                    _libraryManager.AddMetadataReference(metadataReference);
                }

                //LibraryExport projectExport = exporter.GetExport(moduleContext.Project.Name);
                //IMetadataProjectReference projectRef = (IMetadataProjectReference)projectExport.MetadataReferences.First();

                //string exportDir = Path.Combine(moduleContext.Project.ProjectDirectory, "export");
                //projectRef.EmitAssembly(exportDir);

                //string assemblyPath = Path.Combine(exportDir, moduleContext.Project.Name + ".dll");

                Assembly pluginAssm = engine.LoadProject(
                    moduleContext.Project, moduleContext.FrameworkName, null, _assemblyLoadContextAccessor.Default,
                    new AssemblyName(moduleContext.Project.Name));

                IList<LibraryDependency> flattenedList = moduleContext
                    .Project
                    .Dependencies
                    .SelectMany(Flatten)
                    .Where(x => x.Library.Type == "Package")
                    .Distinct()
                    .ToList();

                //foreach (var dependency in flattenedList)
                //{
                //    foreach (var assemblyToLoad in dependency.Library.Assemblies)
                //    {
                //        Assembly.Load(new AssemblyName(assemblyToLoad));
                //    }
                //}

                return pluginAssm;
            });
        }

        public static IList<LibraryDependency> Flatten(LibraryDependency root)
        {
            var flattened = new List<LibraryDependency> { root };

            var children = root.Library.Dependencies;

            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(Flatten(child));
                }
            }

            return flattened;
        }
    }
}
