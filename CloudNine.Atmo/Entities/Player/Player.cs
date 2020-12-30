using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Atmo.Entities.Player
{
    public class Player : LivingEntity
    {
        [Key]
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        public Player(ulong id, string username) : base(username)
        {
            this.Id = id;
        }
    }
}
