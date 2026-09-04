using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanActiveUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Loan_BookId_ReturnedAt",
                table: "Loan");

            migrationBuilder.CreateIndex(
                name: "UX_Loan_BookId_Active",
                table: "Loan",
                column: "BookId",
                unique: true,
                filter: "\"ReturnedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Loan_BookId_Active",
                table: "Loan");

            migrationBuilder.CreateIndex(
                name: "IX_Loan_BookId_ReturnedAt",
                table: "Loan",
                columns: new[] { "BookId", "ReturnedAt" });
        }
    }
}
