using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Configuration
{
    public static class CombineOptions
    {
        public static MultisearchConfigurationOptions Combine(this MultisearchConfigurationOptions guildOptions, MultisearchConfigurationOptions userOptions)
        {
            var options = new MultisearchConfigurationOptions()
            {
                CharacterTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                DefaultSearchOptions = guildOptions.DefaultSearchOptions.Combine(userOptions.DefaultSearchOptions),
                HideSensitiveContentDescriptions = guildOptions.HideSensitiveContentDescriptions || userOptions.HideSensitiveContentDescriptions,
                OverflowDescription = guildOptions.OverflowDescription || userOptions.OverflowDescription,
                RelationshipTagLimit = Math.Min(guildOptions.CharacterTagLimit, userOptions.CharacterTagLimit),
                TagLimit = Math.Min(guildOptions.TagLimit, userOptions.TagLimit)
            };

            return options;
        }

        public static SearchOptions Combine(this SearchOptions guildOptions, SearchOptions userOptions)
        {
            SearchOptions options = new()
            {
                AllowExplicit = guildOptions.AllowExplicit && userOptions.AllowExplicit,
                TreatWarningsNotUsedAsWarnings = guildOptions.TreatWarningsNotUsedAsWarnings || userOptions.TreatWarningsNotUsedAsWarnings,
                SearchConfiguration = userOptions.SearchConfiguration // guilds dont get to set this one.
            };

            return options;
        }
    }
}
