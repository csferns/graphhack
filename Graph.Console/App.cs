using Graph.Console.Model;
using Graph.Console.Options;
using Graph.Console.Services;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models.ODataErrors;
using System.Diagnostics;

namespace Graph.Console;

internal sealed class App
{
    private static readonly string[] scopes = new string[5] { "User.Read", "Chat.Read", "Team.ReadBasic.All", "Channel.ReadBasic.All", "ChannelMessage.Read.All" };

    private readonly GraphOptions graphOptions;
    private readonly ILogger<App> logger;
    private readonly IOutputService outputService;

    public App(IOptions<GraphOptions> graphOptions, ILogger<App> logger, IOutputService outputService)
    {
        this.graphOptions = graphOptions.Value ?? throw new ArgumentNullException(nameof(graphOptions));
        this.logger = logger;
        this.outputService = outputService;
    }

    private GraphServiceClient GetClient()
    {
        InteractiveBrowserCredentialOptions interactiveBrowserCredentialOptions = new()
        {
            TenantId = graphOptions.TenantId is { Length: > 0 } tenantId ? tenantId : "common",
            ClientId = graphOptions.ClientId
        };

        InteractiveBrowserCredential tokenCredential = new(interactiveBrowserCredentialOptions);

        return new GraphServiceClient(tokenCredential, scopes);
    }

    public async Task RunAsync(string[] args, CancellationToken token = default)
    {
        GraphServiceClient client = GetClient();

        Stopwatch sw = Stopwatch.StartNew();

        await ContributionSummaryAsync(client, token);

        sw.Stop();

        logger.LogInformation("Completed in {0}", sw.Elapsed);
    }

    private async Task ContributionSummaryAsync(GraphServiceClient client, CancellationToken token = default)
    {
        TeamCollectionResponse? teamsResponse = await WrapAsync(client.Me.JoinedTeams.GetAsync(static opt => opt.QueryParameters.Select = new string[2] { "Id", "DisplayName" }, cancellationToken: token));

        if (teamsResponse?.Value is not { Count: > 0 } teams) 
        {
            return;
        }

        foreach (Team team in teams)
        {
            if (string.IsNullOrEmpty(team.Id)) continue;

            ChannelCollectionResponse? channelsResponse = await WrapAsync(client.Teams[team.Id].Channels.GetAsync(cancellationToken: token));

            if (channelsResponse?.Value is not { Count: > 0 } channels) continue;

            await Parallel.ForEachAsync(channels, token, async (channel, ctx) =>
            {
                ChatMessageCollectionResponse? messagesResponse = await WrapAsync(client.Teams[team.Id].Channels[channel.Id].Messages.GetAsync(cancellationToken: ctx));

                if (messagesResponse?.Value is not { Count: > 0 } messages) return;

                List<ChatMessage> filtered = messages.Where(x => x is { CreatedDateTime: not null, From.User.DisplayName.Length: > 0 }).ToList();

                if (!filtered.Any()) return;

                List<ContributionResult> grouped = 
                (
                    from f in filtered
                    group f by new { f.From!.User!.DisplayName, Date = f.CreatedDateTime!.Value.Date } into tmp
                    let count = tmp.Count()
                    orderby count descending
                    select new ContributionResult()
                    {
                        UserDisplayName = tmp.Key.DisplayName,
                        Date = tmp.Key.Date,
                        Count = count,
                        Percentage = Math.Round((100m * count) / filtered.Count),
                    }
                ).ToList();

                if (!grouped.Any()) return;

                await outputService.Output(grouped, team.DisplayName!, channel.DisplayName!, ctx);
            });
        }
    }

    private async Task<TResult?> WrapAsync<TResult>(Task<TResult> task) 
    {
        try
        {
            return await task;
        }
        catch (ODataError e) when (e.Error is { Message.Length: > 0 } error) 
        {
            logger.LogCritical(error.Message);
        }
        catch (ODataError e)
        {
            logger.LogCritical("An error occurred calling the Graph API", e);
        }
        catch 
        {
            throw;
        }

        return default;        
    }
}