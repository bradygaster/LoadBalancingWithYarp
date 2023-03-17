public record DestinationOperation
{
    public Operation Operation { get; init; }

    public string? Name { get; init; }

    public string? Address { get; init; }
}