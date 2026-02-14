using System.Text.Json.Serialization;
using DirectoryService.Shared;

namespace DirectoryService.Presentation.Envelopes;

public record Envelope
{
    public object? Result { get; }

    public Errors? Errors { get; }

    public bool IsError => Errors != null && Errors.Any();

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) =>
        new(result, null);

    public static Envelope Error(Errors errors) =>
        new(null, errors);
}