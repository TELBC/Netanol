using Microsoft.EntityFrameworkCore;
using Fennec;
using Fennec.Database;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);

// TODO: use proper services
builder.Host.UseDefaultServiceProvider(opts => opts.ValidateScopes = false);

startup.ConfigureServices(builder.Services);
startup.ConfigureHost(builder.Host);

var app = builder.Build();
startup.Configure(app, builder.Environment);
app.Run();