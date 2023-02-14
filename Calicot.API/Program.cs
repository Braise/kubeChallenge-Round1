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
using Calicot.Shared;
using Calicot.Shared.Data;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Calicot.Shared.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

string databaseName = builder.Configuration.GetSection("CosmosDb").GetSection("DatabaseName").Value??"";
string containerName = builder.Configuration.GetSection("CosmosDb").GetSection("ContainerName").Value??"";
string account = builder.Configuration.GetSection("CosmosDb").GetSection("Account").Value??"";
string key = builder.Configuration.GetSection("CosmosDb").GetSection("Key").Value??"";
string connectionString = builder.Configuration.GetSection("CosmosDb").GetSection("ConnectionString").Value??"";


const string PolicyName = "CorsPolicy";
const string DevCorsPolicyName = "DevCorsPolicy";

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueCountLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue; //not recommended value
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});


builder.Services.AddControllers();
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContext<CalicotDB>(options =>
            options.UseCosmos(connectionString,
                        databaseName: databaseName));

builder.Services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(builder.Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
builder.Services.AddSingleton<IBlobStorageService>(new BlobStorageService(builder.Configuration));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddCors(options => options.AddPolicy(PolicyName, build =>
{
    build
        .SetPreflightMaxAge(TimeSpan.MaxValue)
        .WithOrigins("https://calicot.azurewebsites.net",
                    "https://calicotapi.azurewebsites.net",
                    "https://accounts.google.com",
                    "https://play.google.com",
                    "https://localhost")
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

// add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: DevCorsPolicyName,
        builder => builder.SetIsOriginAllowed(s => s.Contains("localhost"))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


builder.Services.AddAuthentication()
    .AddGoogle("google", opt =>
    {
        var googleAuth = builder.Configuration.GetSection("Authentication:Google");
        opt.ClientId = googleAuth["ClientId"]??"";
        opt.ClientSecret = googleAuth["ClientSecret"]??"";
        opt.SignInScheme = IdentityConstants.ExternalScheme;
    }).AddJwtBearer();

builder.Services.AddAuthorization();

// This is the tricky part to inject the configuration so the public key is ued to validate the JWT
builder.Services.AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Calicot.API", Version = "v1" });
//});

var app = builder.Build();

var blobStorageService = app.Services.GetRequiredService<IBlobStorageService>();
var cosmosDbService = app.Services.GetRequiredService<ICosmosDbService>();

IConfiguration configuration = app.Configuration;
IWebHostEnvironment env = app.Environment;

//4. Call the DataGenerator to create sample data
// if Database is not empty, will not add any other data.
DataGenerator.Initialize(app.Services, blobStorageService, env, cosmosDbService);

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


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

//app.UseSwagger();
//app.UseSwaggerUI();
//app.MapSwagger();


app.MapControllers();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
app.MapFallbackToFile("index.html");
app.MapGet("/", () => "Calicot, The comfiest clothing around!");

app.Run();


/// <summary>
/// Creates a Cosmos DB database and a container with the specified partition key. 
/// </summary>
/// <returns></returns>
static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
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


/// <summary>
/// If the SameSiteMode is set to None, then check the user agent to see if it's a browser that doesn't
/// support SameSite=None. If it is, then set the SameSiteMode to Lax
/// </summary>
/// <param name="HttpContext">The current HttpContext.</param>
/// <param name="CookieOptions">This is the options object that you pass to the SetCookie
/// method.</param>
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