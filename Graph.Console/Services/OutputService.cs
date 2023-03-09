using CsvHelper;
using Graph.Console.Enums;
using Graph.Console.Options;
using Graph.Console.Outputters;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using System.Globalization;

namespace Graph.Console.Services;

public interface IOutputService
{
    Task Output<T>(List<T>? obj, string team, string channel, CancellationToken token = default)
        where T : notnull;
}

internal sealed class OutputService : IOutputService
{
    private readonly GeneralOptions generalOptions;
    private readonly ILogger<OutputService> logger;

    public OutputService(IOptions<GeneralOptions> generalOptions, ILogger<OutputService> logger)
    {
        this.generalOptions = generalOptions.Value ?? throw new ArgumentNullException(nameof(generalOptions));
        this.logger = logger;
    }

    public async Task Output<T>(List<T>? obj, string team, string channel, CancellationToken token = default)
        where T : notnull
    {
        if (obj is not { Count: > 0 }) return;

        string directory = GetValidIOString(team, true);
        string fileName = GetValidIOString(channel);

        string directoryPath = Path.Combine("Output", directory);
        CreateDirectoryIfNotExists(directoryPath);

        List<IOutputter> outputters = new(3);
#if DEBUG
        outputters.Add(new LoggerOutputter(logger));
#else
        if (generalOptions.OutputFormat.HasFlag(OutputFormat.Txt)) outputters.Add(new TxtOutputter());
        if (generalOptions.OutputFormat.HasFlag(OutputFormat.Csv)) outputters.Add(new CsvOutputter());
        if (generalOptions.OutputFormat.HasFlag(OutputFormat.Logger)) outputters.Add(new LoggerOutputter(logger));
#endif

        await Parallel.ForEachAsync(outputters, token, async (outputter, ctx) =>
        {
            string fullPath = await outputter.WriteAsync(obj, directoryPath, fileName, ctx).ConfigureAwait(false);
            logger.LogInformation("Wrote {0} record(s) to {1}", obj.Count, fullPath);
        });
    }

    private string GetValidIOString(string givenFileName, bool directory = false)
    {
        ReadOnlySpan<char> chars = givenFileName.AsSpan();

        if (chars.IsEmpty) return Path.GetTempFileName();

        char[] invalidChars = directory ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars();

        Span<char> replaced = givenFileName.Length < 255 
            ? stackalloc char[givenFileName.Length] 
            : new char[givenFileName.Length];

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            replaced[i] = invalidChars.Contains(c) ? '_' : c;
        }

        return new string(replaced);
    }

    private void CreateDirectoryIfNotExists(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
