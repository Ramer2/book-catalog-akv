using AutoMapper;
using BookCatalog.Application.Responses.User;
using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>();
    }
}
