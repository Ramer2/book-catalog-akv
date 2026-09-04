using BookCatalog.Application.Interfaces.Persistence;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;

namespace BookCatalog.Tests.TestUtils;

internal static class LoanServiceFactory
{
    public static LoanService Create(ILoanRepository repository)
    {
        var interpreter = new Mock<IDbExceptionInterpreter>(MockBehavior.Strict);
        interpreter
            .Setup(i => i.IsUniqueViolation(It.IsAny<Exception>(), It.IsAny<string?>()))
            .Returns(false);
        return new LoanService(repository, interpreter.Object);
    }
}
