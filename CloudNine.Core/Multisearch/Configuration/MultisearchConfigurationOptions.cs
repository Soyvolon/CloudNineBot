using Newtonsoft.Json;

namespace CloudNine.Core.Multisearch.Configuration
{
    public class MultisearchConfigurationOptions
    {
        [JsonProperty("overflow_description")]
        public bool OverflowDescription { get; set; } = true;
        [JsonProperty("hide_sensitive_content_descriptions")]
        public bool HideSensitiveContentDescriptions { get; set; } = true;
        [JsonProperty("tag_limit")]
        public int TagLimit { get; set; } = 0;
        [JsonProperty("character_tag_limit")]
        public int CharacterTagLimit { get; set; } = 0;
        [JsonProperty("relationship_tag_limit")]
        public int RelationshipTagLimit { get; set; } = 0;
        [JsonProperty("cache_fanfics")]
        public bool CacheFanfics { get; set; } = true;
        [JsonProperty("display_link_data")]
        public bool DisplayLinkData { get; set; } = true;
        [JsonProperty("search_options")]
        public SearchOptions DefaultSearchOptions { get; set; } = new();
    }
}
