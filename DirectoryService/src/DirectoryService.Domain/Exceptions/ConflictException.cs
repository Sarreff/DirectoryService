using DirectoryService.Shared;

namespace DirectoryService.Domain.Exceptions;

public class ConflictException : Exception
{
    public Error Error { get; } = null!;

    public ConflictException(Error error)
        : base(error.GetMessage())
    {
        Error = error;
    }

    public ConflictException()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}