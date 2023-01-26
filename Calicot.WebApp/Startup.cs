using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Cosmos;
using Calicot.WebApp.Data;
using Calicot.WebApp.Models;
using Calicot.WebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Calicot.WebApp.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Calicot.WebApp
{
    public class Startup
    {
        string connectionString = "";
        string databaseName = "";
        string containerName = "";
        string account = "";
        string key = "";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            databaseName = configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value??"";
            containerName = configuration.GetSection("CosmosDb").GetSection("ContainerName").Value??"";
            account = configuration.GetSection("CosmosDb").GetSection("Account").Value??"";
            key = configuration.GetSection("CosmosDb").GetSection("Key").Value??"";
            connectionString = configuration.GetSection("CosmosDb").GetSection("ConnectionString").Value??"";

        }

        public IConfiguration Configuration { get; }
        private const string PolicyName = "CorsPolicy";
        private const string DevCorsPolicyName = "DevCorsPolicy";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 10; //default 1024
                options.ValueLengthLimit = int.MaxValue; //not recommended value
                options.MultipartBodyLengthLimit = long.MaxValue; //not recommended value
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            services.AddControllersWithViews();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton<IBlobStorageService>(new BlobStorageService(Configuration));
            services.AddScoped<IUserService, UserService>();

            // In production, the Angular files will be served from this directory
            // services.AddSpaStaticFiles(configuration =>
            // {
            //     configuration.RootPath = "ClientApp/dist";
            // });

            services.AddCors(options => options.AddPolicy(PolicyName, build =>
            {
                build
                    .SetPreflightMaxAge(TimeSpan.MaxValue)
                    .WithOrigins("https://calicot-webapp.azurewebsites.net",
                                "https://accounts.google.com",
                                "https://play.google.com",
                                "https://localhost")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            // add cors
            services.AddCors(options =>
            {
                options.AddPolicy(name: DevCorsPolicyName,
                    builder => builder.SetIsOriginAllowed(s => s.Contains("localhost"))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddDbContext<CalicotDB>(options =>
                        options.UseCosmos(connectionString,
                                    databaseName: databaseName));

            // services
            //     .AddDefaultIdentity<User, IdentityRole>();


            services.AddAuthentication()
                .AddGoogle("google", opt =>
                {
                    var googleAuth = Configuration.GetSection("Authentication:Google");
                    opt.ClientId = googleAuth["ClientId"]??"";
                    opt.ClientSecret = googleAuth["ClientSecret"]??"";
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                }).AddJwtBearer();

            services.AddScoped<IUserService, UserService>();

            // This is the tricky part to inject the configuration so the public key is ued to validate the JWT
            services.AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        }

        void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // if (MyUserAgentDetectionLib.DisallowsSameSiteNone(userAgent))
                // {
                    options.SameSite = SameSiteMode.Lax;
                // }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors(DevCorsPolicyName);
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseCors(PolicyName);
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // // Configure the HTTP request pipeline.
            // if (!env.IsDevelopment())
            // {
            //     app.UseExceptionHandler("/Error");
            //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //     app.UseHsts();
            // }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapFallbackToFile("index.html");
            });


            

            // app.UseSpa(spa =>
            // {
            //     // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //     // see https://go.microsoft.com/fwlink/?linkid=864501

            //     spa.Options.SourcePath = "ClientApp";

            //     if (env.IsDevelopment())
            //     {
            //         spa.UseAngularCliServer(npmScript: "start");
            //     }
            // });
        }


        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetValue<string>("DatabaseName") ?? "";
            string containerName = configurationSection.GetSection("ContainerName").Value??"";
            string account = configurationSection.GetSection("Account").Value??"";
            string key = configurationSection.GetSection("Key").Value??"";
            
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }
    }
}
