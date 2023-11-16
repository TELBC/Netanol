using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Fennec.Collectors;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Options;
using Fennec.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using OpenApiInfo = Microsoft.OpenApi.Models.OpenApiInfo;
using Serilog.Sinks.Grafana.Loki;

namespace Fennec;

public class Startup
{
    public Startup(IConfigurationRoot configuration, ILogger log)
    {
        Configuration = configuration;
        StartupOptions = Configuration.GetSection("Startup").Get<StartupOptions>() ??
                         throw new InvalidConstraintException(
                             "The `Startup` section is not defined in the configuration.");
        SecurityOptions = Configuration.GetSection("Security").Get<SecurityOptions>() ??
                          throw new InvalidConstraintException(
                              "The `Security` section is not defined in the configuration.");
        Log = log;
    }

    private IConfigurationRoot Configuration { get; }
    private StartupOptions StartupOptions { get; }
    private SecurityOptions SecurityOptions { get; }
    private ILogger Log { get; }

    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment environment)
    {
        // Options
        services.Configure<Netflow9CollectorOptions>(Configuration.GetSection("Collectors:Netflow9"));
        services.Configure<IpfixCollectorOptions>(Configuration.GetSection("Collectors:Ipfix"));
        services
            .AddOptions<SecurityOptions>()
            .Bind(Configuration.GetSection("Security"))
            .ValidateDataAnnotations();
        
        // Metric service
        services.AddSingleton<IMetricService, MetricService>();
        
        // Database services
        services.AddScoped<ITraceImportService, TraceImportService>();
        // services.AddScoped<ILayoutRepository, LayoutRepository>();
        services.AddScoped<ITraceRepository, TraceRepository>();
        services.AddSingleton<IMongoClient>(_ => new MongoClient(Configuration.GetConnectionString("MongoConnection")));
        services.AddSingleton<IMongoCollection<SingleTrace>>(
            s => s.GetRequiredService<IMongoClient>().GetDatabase("packrat")
                .GetCollection<SingleTrace>("singleTraces"));

        // Collector services
        services.AddHostedService<NetFlow9Collector>(); // TODO: set exception behaviour
        services.AddHostedService<IpFixCollector>(); // TODO: set exception behaviour
        
        // Web services
        services.AddControllers();
        services.AddAutoMapper(typeof(Program).Assembly);

        if (StartupOptions.AllowCors)
        {
            Log.Information("Adding CORS header which allow all origins, headers and methods");
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });
        } else 
            Log.Information("CORS is disabled... No CORS header will be added");

        if (StartupOptions.EnableSwagger)
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fennec API", Version = "v1" });

                var filePath = Path.Combine(AppContext.BaseDirectory, "Fennec.xml");
                c.IncludeXmlComments(filePath);
            });
        
        // Authentication services
        services.AddDbContext<AuthContext>(options =>
            options.UseSqlite("Data Source=Data/Identity.db"));
        services.AddIdentity<NetanolUser, NetanolRole>()
            .AddEntityFrameworkStores<AuthContext>()
            .AddDefaultTokenProviders();

        // Cookie configuration
        /*
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.OnAppendCookie = cookieContext =>
            {
                cookieContext.CookieOptions.Secure = true;
                cookieContext.CookieOptions.SameSite = SameSiteMode.Strict;
            };
        }); */

        services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.Events.OnRedirectToLogin = context =>
                {
                    context.RedirectUri = "/";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });
        
        // services.AddAuthorization();
        
        // Identity services
        services.Configure<IdentityOptions>(opts =>
        {
            // Password settings.
            opts.Password.RequireDigit = true;
            opts.Password.RequireLowercase = true;
            opts.Password.RequireNonAlphanumeric = true;
            opts.Password.RequireUppercase = true;
            opts.Password.RequiredLength = 6;
            opts.Password.RequiredUniqueChars = 1;
            
            // Lockout settings.
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            opts.Lockout.MaxFailedAccessAttempts = 5;
        });
    }

    public void ConfigureHost(ConfigureHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((context, _, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal) // ignore EF logs
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .WriteTo.GrafanaLoki(context.Configuration.GetConnectionString("Loki"))
                .WriteTo
                .Console(restrictedToMinimumLevel: LogEventLevel
                    .Debug); // TODO: behave different in different environment
        });
    }

    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AuthContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<NetanolUser>>();
            await context.Database.EnsureCreatedAsync();

            var secOpts = scope.ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>().Value;
            if (!userManager.Users.Any(u => u.UserName == secOpts.Access.Username))
            {
                Log.Information("No initial admin user found... Creating one");
                var adminUser = new NetanolUser { UserName = secOpts.Access.Username };
                var createAdminUserResult = await userManager.CreateAsync(adminUser, secOpts.Access.Password);

                if (!createAdminUserResult.Succeeded)
                {
                    Log.Error("Failed to create initial admin user... Exiting");
                    Environment.Exit(1);
                }
            } else
                Log.Information("Initial admin user already exists... Skipping creation");
        }
        
        app.UsePathBase("/api");
        app.UseSerilogRequestLogging();
        
        if (StartupOptions.EnableSwagger)
        {
            Log.Information("Enabling the Swagger developer interface");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Fennec API V1");
                c.RoutePrefix = "swagger";
            });
        } else
            Log.Information("Swagger is disabled... No Swagger UI will be available");

        if (StartupOptions.AllowCors) 
            app.UseCors();

        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}