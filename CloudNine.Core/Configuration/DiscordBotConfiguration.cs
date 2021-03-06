﻿
using System.Collections.Generic;

using CloudNine.Core.Configuration;

using Newtonsoft.Json;

namespace CloudNine.Config.Bot
{
    public class DiscordBotConfiguration
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("trigger_bday_time")]
        public int TriggerBday { get; private set; }

        [JsonProperty("client_secret")]
        public string Secret { get; private set; }
        //[JsonProperty("slash_commands")]
        //public List<SlashCommandConfiguration> SlashCommands { get; private set; }
    }
}
