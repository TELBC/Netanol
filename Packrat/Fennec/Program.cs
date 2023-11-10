using Fennec;
using Serilog;

var log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "BOOT LOGGER: [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

log.Information("Starting Fennec... This is the boot logger and will be replaced by the actual logger once the configuration is loaded");
var builder = WebApplication.CreateBuilder(args);
// builder.Host.UseDefaultServiceProvider(opts => opts.ValidateScopes = false);
builder.Configuration.AddEnvironmentVariables("FENNEC_");

var startup = new Startup(builder.Configuration, log);

startup.ConfigureServices(builder.Services, builder.Environment);
startup.ConfigureHost(builder.Host, builder.Configuration);

var app = builder.Build();
await startup.Configure(app, builder.Environment);

log.Information("Fennec configured successfully... Starting the application and handing over to the actual logger");
app.Run();