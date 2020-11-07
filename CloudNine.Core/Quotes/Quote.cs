using System;

namespace CloudNine.Core.Quotes
{
    public class Quote
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime SavedAt { get; set; }
        public string SavedBy { get; set; }
        public int Id { get; set; }
    }
}
