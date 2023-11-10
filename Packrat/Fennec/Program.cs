using Fennec;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("FENNEC_");

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services, builder.Environment);
startup.ConfigureHost(builder.Host, builder.Configuration);

var app = builder.Build();
await startup.Configure(app, builder.Environment);
app.Run();