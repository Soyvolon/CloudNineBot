﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        public int Uses { get; set; }

        [JsonIgnore]
        public DiscordColor? Color
        {
            get
            {
                if (ColorValue is null) return null;

                return new DiscordColor((int)ColorValue);
            }

            set
            {
                if (value.HasValue)
                    ColorValue = value.Value.Value;
                else
                    ColorValue = null;
            }
        }

        public int? ColorValue { get; set; }

        public DiscordEmbedBuilder UseQuote()
        {
            Uses++;

            return BuildQuote();
        }

        public DiscordEmbedBuilder BuildQuote()
        {
            var embed = new DiscordEmbedBuilder()
                    .WithColor(Color ?? new DiscordColor(0x3498db))
                    .WithTitle($"Quote {(Id >= 0 ? Id : "")} - {Author}")
                    .WithDescription(Content)
                    .WithImageUrl(Attachment ?? "")
                    .WithFooter($"Saved by: {SavedBy} | Uses: {Uses}")
                    .WithTimestamp(SavedAt ?? DateTime.UtcNow);

            return embed;
        }
    }
}
