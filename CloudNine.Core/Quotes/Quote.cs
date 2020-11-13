using System;

using DSharpPlus.Entities;

namespace CloudNine.Core.Quotes
{
    public class Quote
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime? SavedAt { get; set; }
        public string SavedBy { get; set; }
        public int Id { get; set; }
        public string? CustomId { get; set; }
        public string? Attachment { get; set; }
        public DiscordColor? Color { get; set; }

        public DiscordEmbedBuilder BuildQuote()
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(Color ?? new DiscordColor(0x3498db))
                    .WithTitle($"Quote {(Id >= 0 ? Id : "")} - {Author}")
                    .WithDescription(Content)
                    .WithImageUrl(Attachment ?? "")
                    .WithFooter($"Saved by: {SavedBy}")
                    .WithTimestamp(SavedAt ?? DateTime.UtcNow);

            return embed;
        }
    }
}
