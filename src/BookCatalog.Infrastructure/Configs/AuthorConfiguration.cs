using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Configs;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
    }
}
