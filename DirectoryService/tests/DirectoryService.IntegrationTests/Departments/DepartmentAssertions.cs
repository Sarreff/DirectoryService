using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Helpers;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.IntegrationTests.Departments;

public partial class DepartmentAssertions
{
    private readonly IDbExecutor _db;

    public DepartmentAssertions(IDbExecutor db)
    {
        _db = db;
    }
}