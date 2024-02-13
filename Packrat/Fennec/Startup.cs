using System.Data;
using Fennec.Database;
using Fennec.Database.Domain.Layers;
using Fennec.Database.Graph;
using Fennec.Metrics;
using Fennec.Options;
using Fennec.Parsers;
using Fennec.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
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
        BsonSerializer.RegisterSerializer(typeof(ILayer), new MongoLayerSerializer());
        services.AddAutoMapper(typeof(MapperProfile));
        
        // Options
        // services.Configure<Netflow9ParserOptions>(Configuration.GetSection("Parsers:Netflow9"));
        // services.Configure<IpfixParserOptions>(Configuration.GetSection("Parsers:Ipfix"));
        services.Configure<DuplicateFlaggingOptions>(Configuration.GetSection("DuplicateFlagging"));
        services
            .AddOptions<SecurityOptions>()
            .Bind(Configuration.GetSection("Security"))
            .ValidateDataAnnotations();
        
        // Metric service
        services.AddSingleton<IMetricService, MetricService>();
        services.AddSingleton<IFlowImporterMetric, FlowImporterMetric>();
        
        // Database services
        // services.AddScoped<ILayoutRepository, LayoutRepository>();
        services.AddSingleton<ITraceRepository, TraceRepository>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddSingleton<IDuplicateFlaggingService, DuplicateFlaggingService>();
        services.AddSingleton<IGraphRepository, GraphRepository>();
        services.AddSingleton<ILayoutRepository, LayoutRepository>();
        services.AddSingleton<ILayerRepository, LayerRepository>();
        services.AddSingleton<IMetricRepository, MetricRepository>();
        services.AddSingleton<IMongoClient>(_ => new MongoClient(Configuration.GetConnectionString("MongoConnection")));
        services.AddSingleton<IMongoDatabase>(s => s.GetRequiredService<IMongoClient>().GetDatabase("packrat"));

        // DnsResolverService
        services.Configure<DnsCacheOptions>(Configuration.GetSection("DnsCache"));
        services.AddSingleton<DnsResolverService>();
        services.AddSingleton<DnsCacheCleanupService>();

        // Parser services
        services.AddSingleton<NetFlow9Parser>();
        services.AddSingleton<IpFixParser>(); 

        // Protocol multiplexer
        var multiplexerOptions = Configuration.GetSection("Multiplexers").Get<List<MultiplexerOptions>>();

        if (multiplexerOptions != null)
            services.AddHostedService<MultiplexerMonitorService>(s => 
                ActivatorUtilities.CreateInstance<MultiplexerMonitorService>(s, multiplexerOptions));
        else
            Log.Error("Failed to read multiplexer configuration... To run no multiplexers define an empty list");

        // Web services
        services.AddControllers(c =>
        {
            c.ModelBinderProviders.Insert(0, new LayerModelBinderProvider());
        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        });
        services.AddAutoMapper(typeof(Program).Assembly);

        if (StartupOptions.AllowCors)
        {
            Log.Information("Adding CORS header which allows all origins, headers and methods");
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
                .Console(restrictedToMinimumLevel: LogEventLevel.Debug); // TODO: behave different in different environment
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
        
        if (SecurityOptions.Enabled)
        {
            Log.Information("Security is enabled... To access protected endpoints authorization is required");
            app.MapControllers();
        }
        else
        {
            Log.Information("Security is disabled... All endpoints can be accessed without authorization");
            app.MapControllers().AllowAnonymous();
        }
    }
}