namespace DirectoryService.Contracts.Departments;

public record GetChildDepartmentsDto(List<ChildDepartmentDto> ChildDepartments, int TotalCount);