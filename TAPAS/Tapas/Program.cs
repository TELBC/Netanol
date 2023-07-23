using Microsoft.EntityFrameworkCore;
using Tapas.Database;
using Tapas.Listeners;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
var db = new TapasDatabase(new DbContextOptionsBuilder<TapasContext>().UseNpgsql(connectionString).Options);
db.Seed();

var collection = builder.Services;
// collection.AddDbContext<TapasContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("Tapas"));
collection.AddDbContext<TapasContext>(options => options.UseNpgsql(connectionString));
collection.AddScoped<TraceRepository>();
collection.AddHostedService<NetFlow9TraceImporter>(); // TODO: set exception behaviour

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();