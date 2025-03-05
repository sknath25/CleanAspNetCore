using SResult;

namespace Application.Shared;

public interface IHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request);
}
