using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;
using StarLib.Database;

namespace StarLib
{
    public class Startup
    {
        public void ConfigureServices()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<StarDbContext>();

            sc.AddTransient<StarDbContext>();

            sc.BuildServiceProvider();
        }

        public void Configure()
        {
        }
    }
}
