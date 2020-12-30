using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.State;

using DSharpPlus.CommandsNext;

namespace CloudNine.Atmo.Levels.Encounters
{
    public abstract class BaseEncounter : StateMachine
    {
        public BaseEncounter(CommandContext ctx, EncounterType type) : base(ctx) 
        {
            Type = type;
        }

        public EncounterType Type { get; init; }
        /// <summary>
        /// Load any variables from their respective static values here.
        /// </summary>
        /// <returns></returns>
        public abstract Task LoadAsync();
    }
}
