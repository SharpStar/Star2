using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Newtonsoft.Json;
using StarLib.Misc;

namespace StarCommands.Localization
{
    public class CommandLocalizationManager : ISingleResolvable
    {
        private readonly Dictionary<string, CommandLocalization> _config = new Dictionary<string, CommandLocalization>();

        public CommandLocalizationManager(IApplicationEnvironment appEnv)
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            string path = Path.Combine(appEnv.ApplicationBasePath, "command_localization.json");
            if (!File.Exists(path))
            {
                string def = JsonConvert.SerializeObject(new Dictionary<string, CommandLocalization>(), Formatting.Indented);

                File.WriteAllText(path, def);
            }

            configBuilder.AddJsonFile(path);
            
            IConfiguration config = configBuilder.Build();
            
            config.Bind(_config);
        }

        public CommandLocalization this[string commandName]
        {
            get
            {
                return _config.SingleOrDefault(p => p.Key.Equals(commandName, StringComparison.CurrentCultureIgnoreCase)).Value;
            }
            set
            {
                _config.Add(commandName, value);
            }
        }
    }
}
