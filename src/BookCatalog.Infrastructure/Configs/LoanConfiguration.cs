using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Configs;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(x => x.BookId)
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .IsRequired();

        builder
            .Property(x => x.BorrowedAt)
            .IsRequired();

        builder
            .Property(x => x.ReturnedAt)
            .IsRequired(false);

        builder
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Speeds up "does this book have an open loan" lookups.
        builder
            .HasIndex(x => new { x.BookId, x.ReturnedAt });

        builder
            .HasIndex(x => x.UserId);
    }
}
