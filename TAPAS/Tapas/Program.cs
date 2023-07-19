using Microsoft.EntityFrameworkCore;
using Tapas.Database;
using Tapas.Listeners;

var builder = WebApplication.CreateBuilder(args);

var collection = builder.Services;
collection.AddDbContext<TapasContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("Tapas"));
collection.AddScoped<TraceRepository>();
collection.AddHostedService<NetFlow9TraceImporter>(); // TODO: set exception behaviour

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
var app = builder.Build();

app.MapControllers();

app.Run();