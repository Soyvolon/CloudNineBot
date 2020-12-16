using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SlashSubcommandGroupAttribute : Attribute
    {
        public string Name { get; set; }
        public SlashSubcommandGroupAttribute(string name)
        {
            Name = name;
        }
    }
}
