using System.Reflection;
using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure;

public class BookCatalogDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Loan> Loans { get; set; }

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

        modelBuilder
            .Entity<User>()
            .ToTable("User")
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder
            .Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        modelBuilder
            .Entity<Loan>()
            .ToTable("Loan");
    }
}
