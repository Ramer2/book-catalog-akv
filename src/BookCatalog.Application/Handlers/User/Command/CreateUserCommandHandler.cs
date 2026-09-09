using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Responses.User;
using BookCatalog.Application.Services.User;
using MediatR;

namespace BookCatalog.Application.Handlers.User.Command;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IMapper mapper, IUserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new BookCatalog.Domain.Models.User(
            request.Email,
            request.PhoneNumber,
            request.FirstName,
            request.LastName,
            request.BirthDate);

        await _userRepository.InsertAsync(user, cancellationToken);
        return _mapper.Map<UserResponse>(user);
    }
}
