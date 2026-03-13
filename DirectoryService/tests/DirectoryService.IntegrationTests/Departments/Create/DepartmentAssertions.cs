using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public partial class DepartmentAssertions
{
    // Успешное создание департамента
    public async Task DepartmentCreated(
        Result<Guid, Errors> result,
        int expectedLocationCount,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.Equal(result.Value, department.Id.Value);

            Assert.Equal("Department", department.Name.Value);
            Assert.Equal("department", department.Identifier.Value);

            Assert.Equal(expectedLocationCount, department.DepartmentLocations.Count);
        });
    }

    // Неуспешное создание департамента
    public async Task DepartmentDidNotCreated(
        Result<Guid, Errors> result,
        ErrorType type,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        await _db.ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == type);
        });
    }
}