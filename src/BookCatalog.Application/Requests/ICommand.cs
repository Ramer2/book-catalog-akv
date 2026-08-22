using MediatR;

namespace BookCatalog.Application.Requests;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : IRequest
{
}