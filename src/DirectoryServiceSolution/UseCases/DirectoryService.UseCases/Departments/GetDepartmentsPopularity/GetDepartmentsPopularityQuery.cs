using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.GetDepartmentsPopularity;

public sealed record GetDepartmentsPopularityQuery
    : IQuery<IEnumerable<GetDepartmentsPopularityResponse>>
{
    public string OrderMode { get; } = "DESC";

    public GetDepartmentsPopularityQuery(string? orderMode)
    {
        if (string.IsNullOrEmpty(orderMode))
            return;

        if (orderMode == "ASC")
            OrderMode = "ASC";
    }
}
