﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch
{
    public enum FicStatus
    {
        Any = 0, // implys no preference
        InProgress,
        Complete
    }

    public static class FicStatusUtils
    {
        public static string GetString(this FicStatus? dir)
            => dir switch
            {
                FicStatus.Any => "Any",
                FicStatus.InProgress => "In Progress Only",
                FicStatus.Complete => "Complete Only",
                _ => "Not Specified"
            };
    }
}
