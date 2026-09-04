using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Configs;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public const string ActiveLoanIndexName = "UX_Loan_BookId_Active";

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

        // Enforces "at most one active loan per book" at the database level.
        // The partial predicate matches how ILoanRepository.GetActiveLoanForBookAsync
        // considers a loan "active" (ReturnedAt IS NULL), and closes the race
        // between the availability check and the insert.
        builder
            .HasIndex(x => x.BookId)
            .IsUnique()
            .HasDatabaseName(LoanConfiguration.ActiveLoanIndexName)
            .HasFilter("\"ReturnedAt\" IS NULL");

        builder
            .HasIndex(x => x.UserId);
    }
}
