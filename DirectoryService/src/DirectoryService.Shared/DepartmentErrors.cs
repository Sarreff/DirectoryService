namespace DirectoryService.Shared;

public static class DepartmentErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict(new ErrorMessage(
            "department.name.conflict",
            $"Department with name {name} already exists",
            "name"));

    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage(
            "department.database.error",
            "Database error while handling department"));

    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("department.operation.cancelled", "Operation was cancelled"));
}