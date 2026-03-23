using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.GetDepartment;

public class GetTopPositionsHandler : IQueryHandler<GetTopDepartmentsByPositionCountDto, GetTopPositionsQuery>
{
    private const int TOP_DEPARTMENTS_BY_POSITION_COUNT = 5;
    private readonly IReadDbContext _readDbContext;

    public GetTopPositionsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetTopDepartmentsByPositionCountDto, Errors>> Handle(
        GetTopPositionsQuery query,
        CancellationToken cancellationToken)
    {
        var departmentsQuery = _readDbContext.DepartmentRead;
        var departmentPositionQuery = _readDbContext.DepartmentPositionRead;

        var departments = await (
                from d in departmentsQuery
                join dp in departmentPositionQuery
                    on d.Id equals dp.DepartmentId into dpGroup
                where d.IsActive
                let positionCount = dpGroup.Count()
                orderby positionCount descending
                select new DepartmentWithPositionCountDto
                {
                    Id = d.Id.Value,
                    Name = d.Name.Value,
                    Identifier = d.Identifier.Value,
                    ParentId = d.ParentId == null ? null : d.ParentId.Value,
                    IsActive = d.IsActive,
                    PositionCount = positionCount,
                })
            .Take(TOP_DEPARTMENTS_BY_POSITION_COUNT)
            .ToListAsync(cancellationToken);

        return new GetTopDepartmentsByPositionCountDto(departments);
    }
}