using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Configs;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.Author)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.Isbn)
            .HasMaxLength(13)
            .IsRequired();
    }
}