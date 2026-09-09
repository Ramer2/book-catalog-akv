using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Responses.User;
using BookCatalog.Application.Services.User;
using MediatR;

namespace BookCatalog.Application.Handlers.User.Command;

public class UpdateUserByIdCommandHandler : IRequestHandler<UpdateUserByIdCommand, UserResponse>
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateUserByIdCommandHandler(IMapper mapper, IUserService userService, IUserRepository userRepository)
    {
        _mapper = mapper;
        _userService = userService;
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Handle(UpdateUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrThrowAsync(request.Id, cancellationToken);

        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.BirthDate = request.BirthDate;

        await _userRepository.SaveAsync(cancellationToken);
        return _mapper.Map<UserResponse>(user);
    }
}