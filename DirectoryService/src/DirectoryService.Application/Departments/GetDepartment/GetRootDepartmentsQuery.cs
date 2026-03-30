using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.GetDepartment;

public record GetRootDepartmentsQuery(
    int Page,
    int Size,
    int Prefetch) : IQuery;