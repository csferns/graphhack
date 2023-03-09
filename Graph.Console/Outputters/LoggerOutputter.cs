namespace Graph.Console.Outputters;

internal sealed class LoggerOutputter : IOutputter
{
    private readonly ILogger logger;

    public LoggerOutputter(ILogger logger)
    {
        this.logger = logger;
    }

    private const string Output = "Logger";
    private static readonly string Separator = ',' + Environment.NewLine;

    public Task<string> WriteAsync<T>(List<T> list, string directoryPath, string fileName, CancellationToken token = default) where T : notnull
    {
        string text = string.Join(Separator, list);

        logger.LogInformation("{0}: {1}{2}", directoryPath, Environment.NewLine, text);

        return Task.FromResult(Output);
    }
}
