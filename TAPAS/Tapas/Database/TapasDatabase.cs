using Microsoft.EntityFrameworkCore;

namespace Tapas.Database;

public class TapasDatabase
{
    private readonly TapasContext _db;

    public TapasDatabase(DbContextOptions<TapasContext> options)
    {
        _db = new TapasContext(options);
        Console.WriteLine("Connetion to database established");
    }
    
    public void Seed()
    {
        _db.Database.EnsureCreatedAsync();
        _db.SaveChanges();
    }
}
