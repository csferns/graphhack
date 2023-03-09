namespace Graph.Console.Outputters;

internal sealed class TxtOutputter : IOutputter
{
    public async Task<string> WriteAsync<T>(List<T> list, string directoryPath, string fileName, CancellationToken token = default)
        where T : notnull
    {
        string text = string.Join($",{Environment.NewLine}", list);

        string fullPath = Path.Combine(directoryPath, $"{fileName}.txt");   
        
        using FileStream fileStream = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter streamWriter = new StreamWriter(fileStream);

        try
        {
            await streamWriter.WriteAsync(text);
        }
        finally
        {
            await streamWriter.FlushAsync();
        }

        return fullPath;
    }
}