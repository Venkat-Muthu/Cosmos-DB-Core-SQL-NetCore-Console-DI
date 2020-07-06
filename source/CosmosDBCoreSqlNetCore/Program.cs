using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDBCoreSqlNetCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = Startup.ConfigureServices();

            var provider = services.BuildServiceProvider();

            await provider.GetService<ConsoleApplication>().Run();

            Console.WriteLine("Completed");
        }
    }
}
