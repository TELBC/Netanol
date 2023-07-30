using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Tapas.Database;
using Tapas.Listeners;

namespace Tapas;

public class Startup
{
    private IConfigurationRoot Configuration { get; }

    public Startup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TapasContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("Tapas"));
        services.AddDbContext<TapasContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection")));
        services.AddScoped<TraceRepository>();
        services.AddHostedService<NetFlow9TraceImporter>(); // TODO: set exception behaviour
        services.AddControllers();
        services.AddAutoMapper(typeof(Program).Assembly);
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
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
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
                                sect["Password"] ??  // TODO: remove sensitive credentials from appsettings.Development.json
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
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TapasContext>();

            // TODO: switch this for .Migrate and add migration support
            ctx.Database.EnsureCreated();
        }

        app.MapControllers();
    }
}