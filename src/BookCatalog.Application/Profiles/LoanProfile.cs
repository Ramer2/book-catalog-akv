using AutoMapper;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Profiles;

public class LoanProfile : Profile
{
    public LoanProfile()
    {
        CreateMap<Loan, LoanResponse>();
    }
}
