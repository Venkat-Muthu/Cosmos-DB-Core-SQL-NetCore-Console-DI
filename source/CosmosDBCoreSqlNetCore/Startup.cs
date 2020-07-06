using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CosmosDBCoreSqlNetCore
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
            {
                // TODO this could fall back to an environment, rather than exceptioning?
                throw new Exception("ASPNETCORE_ENVIRONMENT env variable not set.");
            }

            Console.WriteLine($"Bootstrapping application using environment {env}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            IConfiguration configuration = builder.Build();

            var services = new ServiceCollection()
                .AddLogging(logging => logging.Services.AddLogging());

            services.AddOptions();

            var configurationSection = configuration.GetSection(nameof(CosmosUtility));
            
            services.Configure<CosmosUtility>(options => configurationSection.Bind(options));
            services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            services.AddSingleton<IConfiguration>(configuration);

            services.AddTransient<IContactRepository, CosmosContactRepository>();
            services.AddTransient<ConsoleApplication>();

            services.AddSingleton<IServiceProvider>(services.BuildServiceProvider());

            return services;
        }
    }
}