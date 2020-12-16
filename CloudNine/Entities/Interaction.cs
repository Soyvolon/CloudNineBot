using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Enums;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace CloudNine.Entities
{
    public class Interaction
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
        [JsonProperty("type")]
        public InteractionType Type { get; internal set; }
        [JsonProperty("data")]
        public ApplicationCommandInteractionData? Data { get; internal set; }
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }
        [JsonProperty("member")]
        public DiscordMember Member { get; internal set; }
        [JsonProperty("token")]
        public string Token { get; internal set; }
        [JsonProperty("version")]
        public int Version { get; internal set; }
    }
}
