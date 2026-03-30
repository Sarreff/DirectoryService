namespace DirectoryService.Contracts.Departments;

public record GetRootDepartmentsDto(List<DepartmentDto> RootDepartments, int TotalCount);