namespace Graph.Console.Model;

internal record struct ContributionResult
{
    public required string UserDisplayName { get; set; }
    public required int Count { get; set; }
    public required decimal Percentage { get; set; }
    public required DateTimeOffset Date { get; set; }
}