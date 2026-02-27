using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId,
    UpdateDepartmentLocationsRequest UpdateDepartmentLocationsRequest) : ICommand;