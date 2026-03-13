using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public partial class DepartmentAssertions
{
    // Успешное обновление локаций у департамента
    public async Task DepartmentUpdated(
        Result<Guid, Errors> result,
        int expectedLocationCount,
        LocationId[] oldLocIds,
        LocationId[] newLocIds,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(result.Value, department.Id.Value);

            var oldLocationIds = oldLocIds
                .Select(l => l.Value)
                .ToHashSet();

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            var expectedLocationIds = newLocIds
                .Select(l => l.Value)
                .ToHashSet();

            Assert.Equal(expectedLocationCount, actualLocationIds.Count);
            Assert.Equal(expectedLocationIds, actualLocationIds);
            Assert.NotEqual(oldLocationIds, actualLocationIds);
        });
    }

    // Неуспешное обновление локаций у департамента
    public async Task DepartmentDidNotUpdated(
        Result<Guid, Errors> result,
        DepartmentId departmentId,
        int expectedLocationCount,
        LocationId[] oldLocIds,
        ErrorType type,
        DateTime originalUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == type);

        await _db.ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentId, cancellationToken);

            var expectedLocationIds = oldLocIds
                .Select(l => l.Value)
                .ToHashSet();

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            Assert.Equal(expectedLocationCount, actualLocationIds.Count);
            Assert.Equal(expectedLocationIds, actualLocationIds);

            Assert.Equal(originalUpdatedAt, department.UpdatedAt);
        });
    }
}