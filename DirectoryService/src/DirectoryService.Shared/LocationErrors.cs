namespace DirectoryService.Shared;

public static class LocationErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict(new ErrorMessage(
            "location.name.conflict",
            $"Location with name {name} already exists",
            "name"));

    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage(
            "location.database.error",
            "Database error while handling location."));

    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("location.operation.cancelled", "Operation was cancelled"));

    public static Error AddressConflict() =>
        Error.Conflict(new ErrorMessage(
            "location.address.conflict",
            $"Location with this address already exists",
            "address"));
}