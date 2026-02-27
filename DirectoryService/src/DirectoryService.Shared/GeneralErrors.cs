namespace DirectoryService.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is invalid", name);
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" by Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "Record"} not found{forId}");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("length.is.invalid", $"Value{label}is required");
    }

    public static Error AlreadyExist()
    {
        return Error.Conflict("record.already.exist", "Record already exist");
    }

    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Server error");
    }

    public static Error DuplicatesFound(string message)
    {
        return Error.Conflict("duplicates", $"Duplicates found in {message}");
    }
}