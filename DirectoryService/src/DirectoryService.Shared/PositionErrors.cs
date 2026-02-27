namespace DirectoryService.Shared;

public static class PositionErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict(new ErrorMessage(
            "position.name.conflict",
            $"Position with name {name} already exists",
            "name"));

    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage(
            "position.database.error",
            "Database error while handling position"));

    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("position.operation.cancelled", "Operation was cancelled"));

    public static Error DuplicateActiveName() =>
        Error.Conflict(new ErrorMessage("position.name.duplicate", "Duplicate active position name"));
}