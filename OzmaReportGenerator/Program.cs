using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ReportGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configurationBuilder =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
            if (args.Any())
            {
                var configPath = args[0];
                configurationBuilder
                    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), configPath));
            }
            configurationBuilder.AddEnvironmentVariables();

            WebHost
                .CreateDefaultBuilder(args)
        .UseContentRoot(AppContext.BaseDirectory)
                .UseConfiguration(configurationBuilder.Build())
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
