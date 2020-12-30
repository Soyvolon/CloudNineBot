using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Atmo.Entities;

using DSharpPlus.CommandsNext;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Levels.Encounters
{
    public class CombatEncounter : BaseEncounter
    {
        [JsonProperty("hostile_mobs")]
        private Dictionary<string, Tuple<int, float>> MobNames { get; init; }
        [JsonProperty("start_message")]
        public string? StartMessage { get; internal set; }
        [JsonProperty("end_message")]
        public string? EndMessage { get; internal set; }


        [JsonIgnore]
        public Dictionary<LivingEntity, Tuple<int, float>> HostileMobs { get; init; }

        public CombatEncounter(CommandContext ctx) : base(ctx, EncounterType.Combat)
        {
            HostileMobs = new();
            MobNames = new();
        }

        public override Task LoadAsync() => throw new NotImplementedException();

        public override Task<bool> Execute(ulong userId, params string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
