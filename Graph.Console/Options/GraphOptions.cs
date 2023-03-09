namespace Graph.Console.Options;

internal sealed record GraphOptions 
{
    public string? TenantId { get; set; }
    public required string ClientId { get; set; }
}