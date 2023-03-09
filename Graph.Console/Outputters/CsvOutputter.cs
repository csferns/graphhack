using CsvHelper;
using System.Globalization;

namespace Graph.Console.Outputters;

internal sealed class CsvOutputter : IOutputter
{
    public async Task<string> WriteAsync<T>(List<T> list, string directoryPath, string fileName, CancellationToken token = default)
        where T : notnull
    {
        string fullPath = Path.Combine(directoryPath, $"{fileName}.csv");

        using FileStream fileStream = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
        using StreamWriter streamWriter = new StreamWriter(fileStream);
        using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        try
        {
            await csvWriter.WriteRecordsAsync(list, token);
        }
        finally
        {
            await csvWriter.FlushAsync();
        }

        return fullPath;
    }
}
