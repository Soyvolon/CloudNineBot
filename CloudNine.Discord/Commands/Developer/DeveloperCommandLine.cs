using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Birthdays;
using CloudNine.Core.Configuration;
using CloudNine.Core.Database;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine.Discord.Commands.Developer
{
    public class DeveloperCommandLine : BaseCommandModule
    {
        private readonly CloudNineDatabaseModel _database;

        public DeveloperCommandLine(CloudNineDatabaseModel database)
        {
            this._database = database;
        }

        [Command("run")]
        [Description("Runs a developer command.")]
        [RequireOwner]
        public async Task DeveloperCommandLineParserAsync(CommandContext ctx, params string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    await ctx.RespondAsync("No command entered.");
                }
                else if (args[0] == "dbupdate")
                {
                    await DbUpdateCommandAsync(ctx, args[1..]);
                }
                else
                {
                    await ctx.RespondAsync("command not found.");
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"Command errored:\n{ex.Message}");
            }
        }

        private async Task DbUpdateCommandAsync(CommandContext ctx, params string[] args)
        {
            string folderPath = "ServerConfigs";

            var argsList = args.ToList();
            if (argsList.Contains("--path") || argsList.Contains("-p"))
            {
                int startPos = 0;
                if (argsList.Contains("--path"))
                    startPos = argsList.IndexOf("--path");
                else
                    startPos = argsList.IndexOf("-p");

                if (args.Length < startPos + 1)
                {
                    await ctx.RespondAsync("No path provided for the `--path | -p` argument.");
                    return;
                }

                string fullPath = "";
                try
                {
                    fullPath = Path.GetFullPath(args[startPos + 1]);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync($"Failed to get path string:\n{ex.Message}");
                    return;
                }

                await ctx.RespondAsync($"Attempting read from `{fullPath}`");

                if (Directory.Exists(fullPath))
                {
                    folderPath = fullPath;
                }
                else
                {
                    await ctx.RespondAsync("Path is not a directory.");
                    return;
                }
            }

            await ctx.RespondAsync($"Attempting read from `{folderPath}`");

            await ReadFromLocalFolder(ctx, folderPath);
        }

        private async Task ReadFromLocalFolder(CommandContext ctx, string folderPath)
        {
            foreach (var filePath in Directory.GetFiles(folderPath, "*"))
            {
                try
                {
                    if (!ulong.TryParse(Path.GetFileNameWithoutExtension(filePath), out ulong serverId))
                        continue;

                    var cfg = await _database.FindAsync<DiscordGuildConfiguration>(serverId);

                    if (cfg is null)
                    {
                        cfg = new DiscordGuildConfiguration()
                        {
                            Id = serverId,
                            Prefix = ctx.Prefix
                        };

                        await _database.AddAsync(cfg);
                        await _database.SaveChangesAsync();
                    }

                    using FileStream fs = new FileStream(filePath, FileMode.Open);
                    using StreamReader sr = new StreamReader(fs);
                    var json = await sr.ReadToEndAsync();

                    var newData = JsonConvert.DeserializeObject<BirthdayServerConfiguration>(json);

                    cfg.BirthdayConfiguration = newData;

                    await _database.SaveChangesAsync();

                    await ctx.RespondAsync($"Prased {serverId} from JSON to SQLite datbase.");
                }
                catch (Exception ex)
                {
                    ctx.Client.Logger.LogError(ex, "Failed to convert file to database.");
                    continue;
                }
            }
        }
    }
}
