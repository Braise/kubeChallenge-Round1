using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Cosmos;
using Azure.Cosmos;
using Calicot.WebApp.Data;
using Calicot.WebApp.Services;

namespace Calicot.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                //3. Get the instance of CalicotDBcontext in our services layer
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<CalicotDB>();
                var blobStorageService = services.GetRequiredService<IBlobStorageService>();
                var cosmosDbService = services.GetRequiredService<ICosmosDbService>();
                IWebHostEnvironment env = host.Services.GetRequiredService<IWebHostEnvironment>();
                //4. Call the DataGenerator to create sample data
                DataGenerator.Initialize(services, blobStorageService, env, cosmosDbService);
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        
    }
}
