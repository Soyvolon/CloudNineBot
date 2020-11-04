using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using CloudNine.Core.Birthdays;

namespace CloudNine.Core.Configuration
{
    public class DiscordServerConfiguration
    {
        [Key]
        public ulong Id { get; set; }

        public string Prefix { get; set; }

        public BirthdayServerConfiguration BirthdayConfiguration { get; set; }
    }
}
