using System.Reflection;

namespace ProjectHestia.Discord;

public class DiscordService : IDisposable
{
    private readonly SlashCommandsConfiguration _slashCommandsConfiguration;
    private readonly InteractivityConfiguration _interactivityConfiguration;
    private readonly DiscordShardedClient _discordShardedClient;

    private bool disposedValue;

    public DiscordService(SlashCommandsConfiguration slashCommandsConfiguration, InteractivityConfiguration interactivityConfiguration,
        DiscordShardedClient discordShardedClient)
        => (_slashCommandsConfiguration, _interactivityConfiguration, _discordShardedClient) 
        = (slashCommandsConfiguration, interactivityConfiguration, discordShardedClient);

    /// <summary>
    /// Starts the Discord Service
    /// </summary>
    /// <returns>A task for this operation.</returns>
    public async Task InitalizeAsync()
    {
        var scfg = await _discordShardedClient.UseSlashCommandsAsync(_slashCommandsConfiguration);
        foreach(var slash in scfg.Values)
        {
            List<Type> parentGroups = new() { };
            List<Type> ignoreGrouping = new();
            foreach (var p in parentGroups)
                ignoreGrouping.AddRange(p.GetNestedTypes());

            var types = Assembly.GetAssembly(typeof(DiscordService))?.GetTypes();
            if (types is not null)
                foreach (var t in types)
                    if (t.IsSubclassOf(typeof(ApplicationCommandModule))
                        && !parentGroups.Any(x => t.IsSubclassOf(x))
                        && !ignoreGrouping.Contains(t))
#if DEBUG
                        slash.RegisterCommands(t, 0);
#else
                        slash.RegisterCommands(t);
#endif
        }

        await _discordShardedClient.UseInteractivityAsync(_interactivityConfiguration);

        await _discordShardedClient.StartAsync();

        await Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await _discordShardedClient.UpdateStatusAsync(
                new DiscordActivity($"/help | VERSION", ActivityType.Playing));
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
