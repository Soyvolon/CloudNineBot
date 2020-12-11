using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace CloudNine.Core.Quotes
{
    public class QuoteModel
    {
        [StringLength(215, ErrorMessage = "Author value is too long")]
        public string? Author { get; set; } = null;
        [Required(ErrorMessage = "The quote must have some content!")]
        [StringLength(2048, ErrorMessage = "Quote content too long, max of 2040 characters.")]
        public string Content { get; set; }
        public DateTime? SavedAt { get; set; } = DateTime.UtcNow;

        [StringLength(2035, ErrorMessage = "Saved by content is too long")]
        public string? SavedBy { get; set; } = null;
        public string? CustomId { get; set; } = null;
        public string? Attachment { get; set; } = null;
        public string? ColorHex { get; set; } = "#3498db";

        public Quote Build(string? authorCallout = null)
        {
            var q = new Quote()
            {
                Author = Author ?? "unknown",
                Content = Content,
                SavedAt = SavedAt,
                SavedBy = SavedBy ?? $"unkown author?!" +
                $" Fill out the form completely next" +
                $" time{(authorCallout is null ? "!" : $" {authorCallout}!")}",
                CustomId = CustomId,
                Id = (CustomId is null || CustomId == "") ? 0 : -1,
                Attachment = Attachment,
                Color = new DiscordColor(ColorHex)
            };

            return q;
        }
    }
}
