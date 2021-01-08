using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Builders
{
    public class InvalidDateTimeConfigurationException : Exception
    {
        public InvalidDateTimeConfigurationException() : base() { }
        public InvalidDateTimeConfigurationException(string? message) : base(message) { }
    }
}
