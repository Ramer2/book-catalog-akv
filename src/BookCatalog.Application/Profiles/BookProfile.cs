using AutoMapper;
using BookCatalog.Application.Responses.Book;
using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Profiles;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookResponse>();
    }
}