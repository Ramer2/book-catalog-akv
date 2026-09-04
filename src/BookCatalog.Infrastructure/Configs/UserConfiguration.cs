using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Configs;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder
            .Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.BirthDate)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
    }
}
