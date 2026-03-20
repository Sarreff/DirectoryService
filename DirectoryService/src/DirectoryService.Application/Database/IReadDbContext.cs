using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentRead { get; }

    IQueryable<Location> LocationRead { get; }

    IQueryable<Position> PositionRead { get; }

    IQueryable<DepartmentLocation> DepartmentLocationRead { get; }

    IQueryable<DepartmentPosition> DepartmentPositionRead { get; }
}