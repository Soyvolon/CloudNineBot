using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
