using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Linq;

namespace ReportGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add configuration
            if (args.Any())
            {
                var configPath = args[0];
                builder.Configuration.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), configPath));
            }

            if (!builder.Environment.IsDevelopment())
            {
                builder.Environment.ContentRootPath = AppContext.BaseDirectory;
            }

            // Add services to the container
            var startup = new Startup(builder.Environment, builder.Configuration);
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            startup.Configure(app);

            app.Run();
        }
    }
}
