using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

using CloudNine.Commands;
using CloudNine.Core.Configuration;
using CloudNine.Entities;
using CloudNine.Enums;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace CloudNine.Services
{
    public class SlashCommandHandlingService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly ulong _bot_id;
        private ConcurrentDictionary<string, MethodInfo> Commands { get; init; }

        public SlashCommandHandlingService(ILogger<SlashCommandHandlingService> logger, HttpClient http)
        {
            _logger = logger;
            _client = http;
            _bot_id = Program.Discord.Client.CurrentApplication.Id;

            Commands = new();
            LoadCommandTree();
        }

        private async void LoadCommandTree()
        {
            var cmdType = typeof(SlashCommandBase);
            var commandMethods = cmdType.GetMethods();

            HashSet<string> cmdNames = new();
            foreach (var cmd in commandMethods)
            {
                SlashCommandAttribute? attr;
                if ((attr = cmd.GetCustomAttribute<SlashCommandAttribute>()) is not null)
                {
                    if (!cmdNames.Add(attr.Name))
                        throw new Exception("Cant have two methods with the same command name!");

                    if (cmd.GetParameters().Length <= 0 || cmd.GetParameters()[0].ParameterType != typeof(Interaction))
                        throw new Exception("Slash commands first paramater must be of type Interaction");
                }
            }

            HashSet<Tuple<string, MethodInfo>> toUpdate = new();
            HashSet<SlashCommandConfiguration> toDelete = new();

            _logger.LogInformation("Attempting to read slash state.");

            string json;
            using (FileStream fs = new($"sccfg_{_bot_id}.json", FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new(fs))
                {
                    json = await sr.ReadToEndAsync();
                }
            }

            List<SlashCommandConfiguration> slashCommands = new();
            if (json is not null && json != "")
            {
                slashCommands = JsonConvert.DeserializeObject<List<SlashCommandConfiguration>>(json);
            }

            foreach (var c in commandMethods)
            {
                SlashCommandAttribute? attr;
                if ((attr = c.GetCustomAttribute<SlashCommandAttribute>()) is not null)
                {
                    SlashCommandConfiguration? scfg;
                    if ((scfg = slashCommands.FirstOrDefault(x => x.Name == attr.Name)) is not null)
                    {
                        if (attr.Version != scfg.Version)
                        {
                            toUpdate.Add(new(attr.Name, c));                            
                        }
                        else
                        {
                            await RegisterCommand(c, attr);
                        }    
                    }
                    else
                    {
                        toUpdate.Add(new(attr.Name, c));
                    }    
                }
            }

            foreach (var slashC in slashCommands)
            {
                if (toUpdate.All(x => x.Item1 != slashC.Name))
                {
                    if (commandMethods.All(x =>
                        {
                            SlashCommandAttribute? attr;
                            if ((attr = x.GetCustomAttribute<SlashCommandAttribute>()) is not null)
                            {
                                return attr.Name != slashC.Name;
                            }

                            return false;
                        }))
                    {
                        toDelete.Add(slashC);
                    }
                }
            }

            await RemoveOldCommands(toDelete);
            await UpdateOrAddCommand(toUpdate);
        }

        private async Task RemoveOldCommands(HashSet<SlashCommandConfiguration> toRemove)
        {
            foreach (var scfg in toRemove)
            {
                HttpRequestMessage msg = new();
                msg.Headers.Authorization = new("Bot", Program.DiscordConfig?.Token);
                msg.Method = HttpMethod.Delete;

                if (scfg.GuildId is not null)
                {
                    msg.RequestUri = new Uri($"https://discord.com/applications/{_bot_id}/guilds/{scfg.GuildId}/commands/{scfg.CommandId}");
                }
                else
                {
                    msg.RequestUri = new Uri($"https://discord.com/applications/{_bot_id}/commands/{scfg.CommandId}");
                }

                var response = await _client.SendAsync(msg);

                if(response.IsSuccessStatusCode)
                {
                    Commands.TryRemove(scfg.Name, out _);
                }
            }
        }

        private async Task UpdateOrAddCommand(HashSet<Tuple<string, MethodInfo>> toUpdate)
        {
            foreach(var update in toUpdate)
            {
                HttpRequestMessage msg = new();
                msg.Headers.Authorization = new("Bot", Program.DiscordConfig?.Token);
                msg.Method = HttpMethod.Post;

                SlashCommandAttribute? attr;
                if ((attr = update.Item2.GetCustomAttribute<SlashCommandAttribute>()) is not null)
                {
                    if (attr.GuildId is not null)
                    {
                        msg.RequestUri = new Uri($"https://discord.com/applications/{_bot_id}/guilds/{attr.GuildId}/commands");
                    }
                    else
                    {
                        msg.RequestUri = new Uri($"https://discord.com/applications/{_bot_id}/commands");
                    }
                }
                else throw new Exception("Failed to get SlashCommandAttribute on update/add");

                string desc = update.Item2.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";

                var ps = update.Item2.GetParameters();

                List<ApplicationCommandOption> options = new();

                foreach (var p in ps)
                {
                    var op = new ApplicationCommandOption()
                    {
                        Name = p.Name ?? "unkown",
                        Description = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
                        Type = await GetOptionType(p) ?? throw new Exception("Failed to get valid option")
                    };
                }

                if (options.Count > 10) throw new Exception("Options must be less than 10 items long");

                var cmd = new ApplicationCommand()
                {
                    ApplicationId = _bot_id,
                    Description = desc,
                    Name = attr.Name,
                    Options = options.ToArray()
                };

                msg.Content = JsonContent.Create(cmd);


            }
        }

        private async Task<ApplicationCommandOptionType?> GetOptionType(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(string))
                return ApplicationCommandOptionType.String;
            else if (parameter.ParameterType == typeof(int))
                return ApplicationCommandOptionType.Integer;
            else if (parameter.ParameterType == typeof(bool))
                return ApplicationCommandOptionType.Boolean;
            else if (parameter.ParameterType == typeof(DiscordUser))
                return ApplicationCommandOptionType.User;
            else if (parameter.ParameterType == typeof(DiscordChannel))
                return ApplicationCommandOptionType.Channel;
            else if (parameter.ParameterType == typeof(DiscordRole))
                return ApplicationCommandOptionType.Role;

            return null;
        }

        private async Task RegisterCommand(MethodInfo method, SlashCommandAttribute attr)
        {

        }
    }
}
