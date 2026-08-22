using MediatR;

namespace BookCatalog.Application.Requests;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}