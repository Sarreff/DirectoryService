namespace DirectoryService.Shared;

public static class DepartmentErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict(new ErrorMessage(
            "department.name.conflict",
            $"Department with name {name} already exists",
            "name"));

    public static Error IdentifierConflict(string identifier) =>
        Error.Conflict(new ErrorMessage(
            "department.identifier.conflict",
            $"Department with identifier {identifier} already exists",
            "identifier"));

    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage(
            "department.database.error",
            "Database error while handling department"));

    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("department.operation.cancelled", "Operation was cancelled"));

    public static Error DepartmentsNotFound() =>
        Error.NotFound(new ErrorMessage(
            "departments.notfound",
            "No departments were found"));

    public static Error DepartmentsNotFoundOrInactive() =>
        Error.NotFound(new ErrorMessage(
            "departments.notfound.inactive",
            "Departments were not found or they are inactive"));

    public static Error DepartmentReferenceError() =>
        Error.Conflict(new ErrorMessage(
            "department.reference.conflict",
            "Department reference error while handling department"));

    public static Error DepartmentSelfReferenceError() =>
        Error.Conflict(new ErrorMessage(
            "department.self_reference.conflict",
            $"Department cannot refer to itself"));

    public static Error DepartmentCycleError() =>
        Error.Conflict(new ErrorMessage(
            "department.cycle.error",
            $"A cyclical dependency is created"));
}