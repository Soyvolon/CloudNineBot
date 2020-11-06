
using Newtonsoft.Json;

namespace CloudNine.Config.Bot
{
    public struct DiscordBotConfiguration
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("trigger_bday_time")]
        public int TriggerBday { get; private set; }
    }
}
