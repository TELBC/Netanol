using System.Data;
using System.Reflection;
using DotNetFlow.Ipfix;
using Fennec.Collectors;
using Fennec.Database;
using Fennec.Options;
using Fennec.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Fennec;

public class Startup
{
    public Startup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
        StartupOptions = Configuration.GetSection("Startup").Get<StartupOptions>() ??
                         throw new InvalidConstraintException(
                             "The Startup section is not defined in the configuration.");
    }

    private IConfigurationRoot Configuration { get; }
    private StartupOptions StartupOptions { get; }

    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment environment)
    {
        // Options
        // services.Configure<Netflow9CollectorOptions>(Configuration.GetSection("Collectors:Netflow9"));
        services.Configure<IpfixCollectorOptions>(Configuration.GetSection("Collectors:Ipfix"));

        // Database services
        services.AddScoped<IPackratContext, PackratContext>();
        services.AddScoped<ITraceImportService, TraceImportService>();
        services.AddScoped<ILayoutRepository, LayoutRepository>();
        services.AddScoped<ITraceRepository, TraceRepository>();
        services.AddDbContext<IPackratContext, PackratContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection") ??
                              throw new InvalidOperationException()));

        // Collector services
        services.AddHostedService<IpfixCollector>(); // TODO: set exception behaviour
        // services.AddHostedService<NetFlow9Collector>(); // TODO: set exception behaviour

        // Web services
        services.AddControllers();
        services.AddAutoMapper(typeof(Program).Assembly);

        if (StartupOptions.AllowCors)
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });


        if (!StartupOptions.EnableSwagger)
            return;

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fennec API", Version = "v1" });

            var filePath = Path.Combine(AppContext.BaseDirectory, "Fennec.xml");
            c.IncludeXmlComments(filePath);
        });
    }

    public void ConfigureHost(ConfigureHostBuilder host, IConfiguration configuration)
    {
        var options = new ElasticsearchOptions();
        configuration.GetSection("Elasticsearch").Bind(options);

        host.UseSerilog((context, _, loggerConfiguration) =>
        {
            var sect = Configuration.GetRequiredSection("Elasticsearch");
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo
                .Console(restrictedToMinimumLevel: LogEventLevel
                    .Debug) // TODO: behave different in different environments
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(sect["Uri"] ??
                                                         throw new InvalidOperationException(
                                                             "Elasticsearch:Uri is not defined.")))
                    {
                        ModifyConnectionSettings = x =>
                        {
                            // TODO: properly establish trust between components
                            // trust any certificate
                            x.ServerCertificateValidationCallback((_, _, _, _) => true);

                            if (options.Username == null || options.Password == null)
                                return x;
                            x.BasicAuthentication(options.Username, options.Password);
                            return x;
                        },
                        MinimumLogEventLevel = LogEventLevel.Verbose,
                        DetectElasticsearchVersion = true,
                        AutoRegisterTemplate = true,
                        IndexFormat =
                            $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLower().Replace(".", "-")}-" +
                            $"{context.HostingEnvironment.EnvironmentName.ToLower()}-" +
                            $"{DateTime.UtcNow:yyyy-MM}".ToLower(),
                        NumberOfReplicas = 1,
                        NumberOfShards = 2
                    });
        });
    }

    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UsePathBase("/api");
        
        if (StartupOptions.EnableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Fennec API V1");
                c.RoutePrefix = "swagger";
            });
        }

        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<PackratContext>();

            await ctx.Database.MigrateAsync();
        }

        if (StartupOptions.AllowCors) 
            app.UseCors();

        app.MapControllers();
    }
}