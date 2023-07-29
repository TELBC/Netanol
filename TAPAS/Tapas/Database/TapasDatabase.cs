using Microsoft.EntityFrameworkCore;

namespace Tapas.Database;

public class TapasDatabase
{
    private readonly TapasContext _db;

    public TapasDatabase(DbContextOptions<TapasContext> options)
    {
        _db = new TapasContext(options);
    }
    
    public void Seed()
    {
        _db.Database.EnsureCreated();
        _db.SaveChanges();
    }
}
