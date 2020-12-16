using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNine.Attributes
{
    /// <summary>
    /// Defines a method as the default command for a command grouping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SlashSubcommandAttribute : Attribute
    {
        public string Name { get; init; }

        public SlashSubcommandAttribute(string name)
        {
            Name = name;
        }
    }
}
