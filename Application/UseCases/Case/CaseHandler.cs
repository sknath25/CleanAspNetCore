using Application.Shared;
using SResult;

namespace Application.UseCases.Case;

public class CaseHandler : IHandler<CaseRequest, CaseResponse>
{
    public async Task<Result<CaseResponse>> HandleAsync(CaseRequest request)
    {
        return Reason.Unavailable("This use case is not available.");
    }
}