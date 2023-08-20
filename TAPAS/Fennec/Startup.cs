using System.Reflection;
using Fennec.Collectors;
using Fennec.Database;
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
        services.AddDbContext<TapasContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection")));
        services.AddScoped<ITraceImportService, TraceImportService>();
        services.AddScoped<TraceRepository>();
        services.AddHostedService<NetFlow9Collector>(); // TODO: set exception behaviour
        services.AddControllers();
        services.AddAutoMapper(typeof(Program).Assembly);
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fennec API", Version = "v1" }); });
    }

    public void ConfigureHost(ConfigureHostBuilder host)
    {
        host.UseSerilog((context, _, configuration) =>
        {
            var sect = Configuration.GetRequiredSection("Elasticsearch");
            configuration
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
                            x.ServerCertificateValidationCallback((_, _, _, _) => true); // trust any certificate
                            x.BasicAuthentication(
                                sect["Username"] ??
                                throw new InvalidOperationException("Elasticsearch:Username is not defined."),
                                sect
                                    ["Password"] ?? // TODO: remove sensitive credentials from appsettings.Development.json
                                throw new InvalidOperationException("Elasticsearch:Password is not defined.")
                            );
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