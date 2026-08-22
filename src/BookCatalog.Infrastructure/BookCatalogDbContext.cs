using System.Reflection;
using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure;

public class BookCatalogDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=temp.db");
    
    public BookCatalogDbContext(DbContextOptions<BookCatalogDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // apply configs
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // unique constraint
        modelBuilder
            .Entity<Book>()
            .ToTable("Book")
            .HasIndex(b => b.Isbn)
            .IsUnique();
    }
}