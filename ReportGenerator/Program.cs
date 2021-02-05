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
            if (args.Any())
            {
                var configPath = args[0];
                var configuration =
                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configPath)
                        .AddEnvironmentVariables()
                        .Build();

                WebHost
                    .CreateDefaultBuilder(args)
                    .UseConfiguration(configuration)
                    .UseStartup<Startup>()
                    .Build()
                    .Run();
            }
            else
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
