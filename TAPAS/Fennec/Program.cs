using Fennec;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);

// TODO: use proper services
builder.Host.UseDefaultServiceProvider(opts => opts.ValidateScopes = false);

startup.ConfigureServices(builder.Services, builder.Environment);
startup.ConfigureHost(builder.Host, builder.Configuration);

var app = builder.Build();
startup.Configure(app, builder.Environment);
app.Run();