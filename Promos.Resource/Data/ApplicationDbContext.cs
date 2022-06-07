using Microsoft.EntityFrameworkCore;

namespace Promos.Resource.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<ScannedBarcode> ScannedBarcodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    { 
        base.OnModelCreating(builder); 
    } 
}