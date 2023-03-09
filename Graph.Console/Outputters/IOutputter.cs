namespace Graph.Console.Outputters;

internal interface IOutputter
{
    Task<string> WriteAsync<T>(List<T> list, string directoryPath, string fileName, CancellationToken token = default)
        where T : notnull;
}
