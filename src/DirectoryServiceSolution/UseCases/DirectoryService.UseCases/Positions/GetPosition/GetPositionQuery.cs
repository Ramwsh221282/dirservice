using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Positions.GetPosition;

public sealed class GetPositionQuery : IQuery<GetPositionQueryResponse?>
{
    public Guid Id { get; set; }

    public GetPositionQuery(Guid id)
    {
        Id = id;
    }
}
