using System.Text.Json.Serialization;
using DirectoryService.Shared;

namespace DirectoryService.Presentation.Envelopes;

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? Errors { get; }

    public bool IsError => Errors != null && Errors.Any();

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(T? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) =>
        new(result, null);

    public static Envelope<T> Error(Errors errors) =>
        new(default, errors);
}