using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.GetDepartment;

public record GetChildDepartmentsQuery(
    Guid ParentId,
    int Page,
    int Size) : IQuery;