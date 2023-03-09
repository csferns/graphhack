using Graph.Console.Enums;

namespace Graph.Console.Options;

internal sealed record GeneralOptions
{
    public OutputFormat OutputFormat { get; set; }
}
