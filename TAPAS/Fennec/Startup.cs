using System.Reflection;
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
    }

    private IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Options
        services.Configure<Netflow9CollectorOptions>(Configuration.GetSection("Collectors:Netflow9"));

        // Database services
        services.AddScoped<ITraceImportService, TraceImportService>();
        services.AddScoped<ILayoutPresetRepository, LayoutPresetRepository>();
        services.AddScoped<ITraceRepository, TraceRepository>();
        services.AddDbContext<ITapasContext, TapasContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection")));

        // Collector services
        services.AddHostedService<NetFlow9Collector>(); // TODO: set exception behaviour

        // Web services
        services.AddControllers();
        services.AddAutoMapper(typeof(Program).Assembly);
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fennec API", Version = "v1" });
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

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fennec API V1");
                c.RoutePrefix = "swagger";
            });
        }
        else // return 404 for swagger in production
        {
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/swagger"), builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not found.");
                });
            });
        }

        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TapasContext>();

            // TODO: switch this for .Migrate and add migration support
            ctx.Database.EnsureCreated();
        }

        app.MapControllers();
    }
}