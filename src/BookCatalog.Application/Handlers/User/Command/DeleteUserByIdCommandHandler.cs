using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Services.User;
using MediatR;

namespace BookCatalog.Application.Handlers.User.Command;

public class DeleteUserByIdCommandHandler : IRequestHandler<DeleteUserByIdCommand>
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public DeleteUserByIdCommandHandler(IUserService userService, IUserRepository userRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrThrowAsync(request.Id, cancellationToken);
        await _userRepository.DeleteEntityAsync(user, cancellationToken);
    }
}