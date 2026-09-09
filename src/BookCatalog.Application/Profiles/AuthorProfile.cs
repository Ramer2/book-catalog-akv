using AutoMapper;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Profiles;

public class AuthorProfile : Profile
{
    public AuthorProfile()
    {
        CreateMap<Author, AuthorResponse>();
    }
}
