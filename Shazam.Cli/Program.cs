using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shazam.Framework;
using Shazam.Framework.Extensions;

namespace Shazam.Cli
{
    class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication<App>();
            var container = new ServiceCollection();
            ConfigureContainer(container);
            var provider = container.BuildServiceProvider();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(provider);
            provider.Dispose();

            return app.Execute(args);
        }

        private static void ConfigureContainer(IServiceCollection serviceCollection)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", true)
                .AddJsonFile("settings.dev.json", false)
                .Build();

            serviceCollection.TryAddSingleton<LoggerFactory>();
            serviceCollection.AddLogging(options =>
            {
                options.AddConsole();
                options.AddDebug();
            });

            serviceCollection.AddOptions();
            serviceCollection.Configure<SolutionSettings>(configuration.GetSection("Solution"));
            serviceCollection.Configure<AuthSettings>(configuration.GetSection("Connection"));

            serviceCollection.AddCdsClient();
        }
    }
}
