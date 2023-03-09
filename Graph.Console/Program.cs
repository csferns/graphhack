using Graph.Console;
using Graph.Console.Options;
using Graph.Console.Services;
using Microsoft.Extensions.DependencyInjection;

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(static builder =>
    {
        builder.AddConsole();
    });

    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables()
        .Build();

    services.AddSingleton<IConfiguration>(configuration);

    services.AddOptions<GraphOptions>()
        .Bind(configuration.GetSection("Graph"))
        .Validate(opt => !string.IsNullOrEmpty(opt.ClientId));

    services.AddOptions<GeneralOptions>()
        .Bind(configuration.GetSection("General"));

    services.AddSingleton<IOutputService, OutputService>();
    services.AddSingleton<App>();
}

var services = new ServiceCollection();
ConfigureServices(services);

using ServiceProvider serviceProvider = services.BuildServiceProvider();

App app = serviceProvider.GetRequiredService<App>();
await app.RunAsync(args);

//Console.Read();