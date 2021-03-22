using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Blog
{
    public class Showcase
    {
        [Key]
        public string Name { get; set; }
        public string Markdown { get; set; }
        public bool Enabled { get; set; }

        public Showcase()
        {
            Name = "";
            Markdown = "";
            Enabled = true;
        }
    }
}
