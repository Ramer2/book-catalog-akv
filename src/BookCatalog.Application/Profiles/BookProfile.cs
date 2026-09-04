using AutoMapper;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Responses.Book;
using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Profiles;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookResponse>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author));
        CreateMap<Author, AuthorResponse>();
    }
}